using Microsoft.Extensions.Logging;
using Superdev.Maui.Navigation.Internals;

namespace Superdev.Maui.Navigation
{
    /// <inheritdoc cref="IPageFactory"/>
    internal sealed class PageFactory : IPageFactory
    {
        private readonly ILogger logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly INavigationRegistry navigationRegistry;
        private readonly IEnumerable<IPageBehaviorFactory> pageBehaviorFactories;

        public PageFactory(
            ILogger<PageFactory> logger,
            IServiceScopeFactory serviceScopeFactory,
            INavigationRegistry navigationRegistry,
            IEnumerable<IPageBehaviorFactory>? pageBehaviorFactories = null)
        {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
            this.navigationRegistry = navigationRegistry;
            this.pageBehaviorFactories = pageBehaviorFactories ?? Array.Empty<IPageBehaviorFactory>();
        }

        public Page CreatePage(string pageName)
        {
            var registration = this.navigationRegistry.GetRegistration(pageName);
            return this.CreatePage(registration);
        }

        public Page CreatePage(PageRegistration registration)
        {
            ArgumentNullException.ThrowIfNull(registration);

            var serviceScope = this.serviceScopeFactory.CreateAsyncScope();
            var ownedInstances = new List<object>();

            try
            {
                var page = CreateInstance<Page>(serviceScope.ServiceProvider, registration.PageType, ownedInstances);

                if (registration.ViewModelType is { } viewModelType)
                {
                    var viewModel = CreateInstance<object>(serviceScope.ServiceProvider, viewModelType, ownedInstances);

                    if (!ReferenceEquals(page.BindingContext, viewModel))
                    {
                        page.BindingContext = viewModel;
                    }
                }

                foreach (var pageBehaviorFactory in this.pageBehaviorFactories)
                {
                    pageBehaviorFactory.ApplyBehaviors(page);
                }

                PageNavigationContext.Attach(page, new PageNavigationContext(serviceScope, ownedInstances));

                this.logger.LogDebug(
                    "Created page {PageType} for navigation name \"{PageName}\"",
                    registration.PageType.Name, registration.Name);

                return page;
            }
            catch (Exception ex)
            {
                // The page never made it onto the navigation stack, so nothing would ever dispose its scope.
                serviceScope.Dispose();

                if (ex is PageResolveException)
                {
                    throw;
                }

                throw new PageResolveException(
                    $"Failed to create page \"{registration.Name ?? registration.PageType.Name}\"", ex);
            }
        }

        private static T CreateInstance<T>(
            IServiceProvider serviceProvider, Type type, ICollection<object> ownedInstances) where T : class
        {
            var instance = serviceProvider.GetService(type);

            if (instance == null)
            {
                // Types which are not registered explicitly are created from the page's scope, so that their
                // dependencies still participate in the per-page scope. The container does not know such an
                // instance and would therefore never dispose it, so the page context takes ownership of it.
                instance = ActivatorUtilities.CreateInstance(serviceProvider, type);
                ownedInstances.Add(instance);
            }

            if (instance is not T typedInstance)
            {
                throw new PageResolveException(
                    $"Type \"{type.Name}\" is not of type {typeof(T).Name}");
            }

            return typedInstance;
        }
    }
}
