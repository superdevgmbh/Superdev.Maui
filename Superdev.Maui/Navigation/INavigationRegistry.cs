using System.Diagnostics.CodeAnalysis;

namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Provides the lookup from a navigation name or a view model type to its <see cref="PageRegistration"/>.
    /// </summary>
    public interface INavigationRegistry
    {
        /// <summary>
        /// Gets the registrations known so far, as a snapshot: the explicit ones, and the ones the convention
        /// scan resolved on demand.
        /// </summary>
        IReadOnlyList<PageRegistration> Registrations { get; }

        /// <summary>
        /// Gets the registration for the given navigation <paramref name="pageName"/>.
        /// </summary>
        /// <param name="pageName">The navigation name of the page.</param>
        /// <exception cref="PageResolveException">
        /// If no page is registered for <paramref name="pageName"/> and it cannot be resolved by convention,
        /// or if the name is ambiguous.
        /// </exception>
        PageRegistration GetRegistration(string pageName);

        /// <summary>
        /// Gets the registration whose <see cref="PageRegistration.ViewModelType"/> is <paramref name="viewModelType"/>.
        /// </summary>
        /// <param name="viewModelType">The view model type the page is registered with.</param>
        /// <exception cref="PageResolveException">
        /// If no page is registered for <paramref name="viewModelType"/>, or if more than one page is registered with it.
        /// </exception>
        PageRegistration GetRegistration(Type viewModelType);

        /// <summary>
        /// Tries to get the registration for the given navigation <paramref name="pageName"/>.
        /// </summary>
        /// <param name="pageName">The navigation name of the page.</param>
        /// <param name="registration">The resolved registration, if any.</param>
        /// <returns><c>true</c> if a registration was found; otherwise, <c>false</c>.</returns>
        bool TryGetRegistration(string pageName, [NotNullWhen(true)] out PageRegistration? registration);
    }
}
