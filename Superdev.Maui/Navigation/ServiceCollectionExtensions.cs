using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Superdev.Maui.Navigation
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterForNavigation<TView>(this IServiceCollection serviceCollection, string? name = null) where TView : VisualElement
        {
            return serviceCollection.RegisterForNavigation(typeof(TView), null, name);
        }

        public static IServiceCollection RegisterForNavigation<TView, TViewModel>(this IServiceCollection serviceCollection, string? name = null) where TView : VisualElement
        {
            return serviceCollection.RegisterForNavigation(typeof(TView), typeof(TViewModel), name);
        }

        /// <summary>
        /// Registers a <paramref name="pageType"/> for navigation and associates it with a <paramref name="viewModelType"/>.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="pageType">The page/view type to register.</param>
        /// <param name="viewModelType">The associated view model type.</param>
        /// <param name="name">The key used for navigation. If not given, the page name is used as navigation key.</param>
        public static IServiceCollection RegisterForNavigation(this IServiceCollection serviceCollection, Type pageType, Type? viewModelType = null, string? name = null)
        {
            ArgumentNullException.ThrowIfNull(pageType);

            if (string.IsNullOrWhiteSpace(name))
            {
                name = pageType.Name;
            }

            var pageRegistration = new PageRegistration
            {
                Name = name,
                PageType = pageType,
                ViewModelType = viewModelType
            };

            serviceCollection.AddKeyedSingleton(name, pageRegistration);
            serviceCollection.TryAddTransient(pageType);

            if (viewModelType is not null)
            {
                serviceCollection.TryAddTransient(viewModelType);
            }

            return serviceCollection;
        }
    }
}