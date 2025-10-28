using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Superdev.Maui.Navigation
{
    public class PageResolver : IPageResolver
    {
        private readonly HashSet<Assembly> cachedAssemblies = new HashSet<Assembly>();
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;

        public PageResolver(
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

            var pageType = pageTypes[0];
            var instance = this.serviceProvider.GetRequiredService(pageType);
            if (instance is not TBindableObject page)
            {
                throw new PageResolveException($"'{pageName}' is not registered or not of type {typeof(TBindableObject).Name}");
            }

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
                    var viewModelType = viewModelTypes.Single();
                    var viewModel = this.serviceProvider.GetRequiredService(viewModelType);
                    page.BindingContext = viewModel;
                }
                else
                {
                    throw new PageResolveException(
                        $"Multiple view models found for name '{viewModelName}': " +
                        $"{string.Join($"> {Environment.NewLine}", viewModelTypes.Select(t => t.FullName))}");
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