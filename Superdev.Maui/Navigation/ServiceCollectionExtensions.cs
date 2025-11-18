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

        public static IServiceCollection RegisterForNavigation(this IServiceCollection serviceCollection, Type view, Type? viewModel, string? name = null)
        {
            ArgumentNullException.ThrowIfNull(view);

            if (string.IsNullOrWhiteSpace(name))
            {
                name = view.Name;
            }

            var pageRegistration = new PageRegistration
            {
                Name = name,
                PageType = view,
                ViewModelType = viewModel
            };

            serviceCollection.AddKeyedSingleton(name, pageRegistration);
            serviceCollection.TryAddTransient(view);

            if (viewModel is not null)
            {
                serviceCollection.TryAddTransient(viewModel);
            }

            return serviceCollection;
        }
    }
}