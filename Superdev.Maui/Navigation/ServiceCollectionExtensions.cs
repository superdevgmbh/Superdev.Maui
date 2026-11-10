using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Extension methods to register pages and view models for navigation.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Pages and view models are constructed at runtime, so a trimmed build has to keep their
        /// constructors.
        /// </summary>
        private const DynamicallyAccessedMemberTypes RegisteredTypeMembers =
            DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors;

        /// <summary>
        /// Registers a page type for navigation.
        /// </summary>
        /// <typeparam name="TView">The page/view type to register.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="name">The navigation name. If not given, the page type name is used.</param>
        public static IServiceCollection RegisterForNavigation<
            [DynamicallyAccessedMembers(RegisteredTypeMembers)] TView>(
            this IServiceCollection serviceCollection, string? name = null)
            where TView : Page
        {
            return serviceCollection.RegisterForNavigation(typeof(TView), null, name);
        }

        /// <summary>
        /// Registers a page type for navigation and associates it with a view model type.
        /// </summary>
        /// <typeparam name="TView">The page/view type to register.</typeparam>
        /// <typeparam name="TViewModel">The associated view model type.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="name">The navigation name. If not given, the page type name is used.</param>
        public static IServiceCollection RegisterForNavigation<
            [DynamicallyAccessedMembers(RegisteredTypeMembers)] TView,
            [DynamicallyAccessedMembers(RegisteredTypeMembers)] TViewModel>(
            this IServiceCollection serviceCollection, string? name = null)
            where TView : Page
        {
            return serviceCollection.RegisterForNavigation(typeof(TView), typeof(TViewModel), name);
        }

        /// <summary>
        /// Registers a <paramref name="pageType"/> for navigation and associates it with a <paramref name="viewModelType"/>.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="pageType">The page/view type to register.</param>
        /// <param name="viewModelType">The associated view model type.</param>
        /// <param name="name">The navigation name. If not given, the page type name is used.</param>
        /// <exception cref="ArgumentException">If <paramref name="pageType"/> is not a <see cref="Page"/>.</exception>
        /// <remarks>
        /// The page and its view model are registered with a scoped lifetime. The navigation service creates
        /// one service scope per navigated page, so the page, its view model and all scoped services they depend on
        /// share a single scope which is disposed as soon as the page leaves the navigation stack.
        /// </remarks>
        public static IServiceCollection RegisterForNavigation(
            this IServiceCollection serviceCollection,
            [DynamicallyAccessedMembers(RegisteredTypeMembers)] Type pageType,
            [DynamicallyAccessedMembers(RegisteredTypeMembers)] Type? viewModelType = null,
            string? name = null)
        {
            ArgumentNullException.ThrowIfNull(pageType);

            if (!typeof(Page).IsAssignableFrom(pageType))
            {
                // Rejected here rather than by the page factory at navigation time, where the registration
                // would have compiled and failed on the first navigation to it.
                throw new ArgumentException(
                    $"Type \"{pageType.Name}\" is not a page. Only pages can be registered for navigation.", nameof(pageType));
            }

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
            serviceCollection.TryAddScoped(pageType);

            if (viewModelType is not null)
            {
                serviceCollection.TryAddScoped(viewModelType);
            }

            return serviceCollection;
        }
    }
}
