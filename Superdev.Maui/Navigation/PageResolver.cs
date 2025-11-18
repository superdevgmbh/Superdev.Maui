using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Superdev.Maui.Navigation
{
    public class PageResolver : IPageResolver
    {
        private static readonly Lazy<IPageResolver> Implementation = new Lazy<IPageResolver>(CreateInstance, LazyThreadSafetyMode.PublicationOnly);

        public static IPageResolver Current => Implementation.Value;

        private static IPageResolver CreateInstance()
        {
            var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<PageResolver>>();
            var serviceProvider = IPlatformApplication.Current.Services.GetRequiredService<IServiceProvider>();
            return new PageResolver(logger, serviceProvider);
        }

        private readonly HashSet<Assembly> cachedAssemblies = new HashSet<Assembly>();
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;

        internal PageResolver(
            ILogger<PageResolver> logger,
            IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public Page ResolvePage(string pageName)
        {
            return this.ResolvePage<Page>(pageName);
        }

        public TBindableObject ResolvePage<TBindableObject>(string pageName) where TBindableObject : BindableObject
        {
            Type pageType = null;
            Type viewModelType = null;

            var pageRegistration = this.serviceProvider.GetKeyedService<PageRegistration>(pageName);
            if (pageRegistration != null)
            {
                pageType = pageRegistration.PageType;
                viewModelType = pageRegistration.ViewModelType;
            }

            if (pageType == null)
            {
                var pageTypes = this.FindTypesWithName(pageName);
                if (pageTypes.Length == 0)
                {
                    throw new PageResolveException($"Page with name '{pageName}' not found");
                }

                if (pageTypes.Length > 1)
                {
                    throw new PageResolveException(
                        $"Multiple pages found for name '{pageName}': " +
                        $"{string.Join($"> {Environment.NewLine}", pageTypes.Select(t => t.FullName))}");
                }

                pageType = pageTypes[0];
            }

            var instance = this.serviceProvider.GetService(pageType);
            if (instance is not TBindableObject page)
            {
                throw new PageResolveException($"'{pageName}' is not registered or not of type {typeof(TBindableObject).Name}");
            }

            if (pageRegistration == null)
            {
                var pageNameIndex = pageName.LastIndexOf("Page", StringComparison.InvariantCultureIgnoreCase);
                if (pageNameIndex > 0)
                {
                    var viewModelName = pageName.Substring(0, pageNameIndex) + "ViewModel";
                    var viewModelTypes = this.FindTypesWithName(viewModelName);

                    if (viewModelTypes.Length == 0)
                    {
                        this.logger.LogInformation($"View model with name '{viewModelName}' not found");
                    }
                    else if (viewModelTypes.Length == 1)
                    {
                        viewModelType = viewModelTypes[0];
                    }
                    else
                    {
                        throw new PageResolveException(
                            $"Multiple view models found for name '{viewModelName}': " +
                            $"{string.Join($"> {Environment.NewLine}", viewModelTypes.Select(t => t.FullName))}");
                    }
                }
            }

            if (viewModelType != null)
            {
                var viewModel = this.serviceProvider.GetService(viewModelType);
                if (viewModel != null)
                {
                    page.BindingContext = viewModel;
                }
            }

            return page;
        }

        private Type[] FindTypesWithName(string typeName)
        {
            // Attempt to lookup type name in cached assemblies
            var types = FindTypesWithName(this.cachedAssemblies, typeName);

            if (types.Length == 0)
            {
                // If no assembly is found, scan all loaded assemblies for the type name
                types = FindTypesWithName(AppDomain.CurrentDomain.GetAssemblies(), typeName);

                foreach (var type in types)
                {
                    this.cachedAssemblies.Add(type.Assembly);
                }
            }

            return types;
        }

        private static Type[] FindTypesWithName(IEnumerable<Assembly> assemblies, string typeName)
        {
            var types = assemblies.SelectMany(a => a
                    .GetTypes()
                    .Where(t => string.Equals(t.Name, typeName, StringComparison.InvariantCultureIgnoreCase)))
                .ToArray();

            return types;
        }
    }

    public class PageResolveException : Exception
    {
        public PageResolveException(string message) : base(message)
        {
        }
    }
}