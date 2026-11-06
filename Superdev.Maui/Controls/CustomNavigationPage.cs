using Microsoft.Extensions.Logging;
using Superdev.Maui.Navigation;
using Superdev.Maui.Navigation.Hosts;

namespace Superdev.Maui.Controls
{
    /// <summary>
    /// A <see cref="NavigationPage"/> which routes back navigation triggered by the user
    /// through <see cref="INavigationService"/>.
    /// </summary>
    /// <remarks>
    /// Without this page, a back navigation triggered by the Android hardware back button bypasses the
    /// navigation service, so navigation guards cannot veto it. Pages are still torn down correctly in that
    /// case, but a page which implements <see cref="IConfirmNavigation"/> would not be asked.
    /// <para>
    /// This page is registered for navigation under the name <c>NavigationPage</c>, so a navigation URI
    /// such as <c>"/NavigationPage/HomePage"</c> uses it automatically.
    /// </para>
    /// </remarks>
    public class CustomNavigationPage : NavigationPage
    {
        /// <inheritdoc cref="CustomNavigationPage"/>
        public CustomNavigationPage()
        {
        }

        /// <inheritdoc cref="CustomNavigationPage"/>
        public CustomNavigationPage(Page root)
            : base(root)
        {
        }

        /// <inheritdoc />
        protected override bool OnBackButtonPressed()
        {
            var services = IPlatformApplication.Current?.Services;
            var navigationHost = services?.GetService<INavigationHost>();
            var logger = services?.GetService<ILogger<CustomNavigationPage>>();

            if (navigationHost == null)
            {
                logger?.LogDebug("Back button pressed, but no navigation host is registered; the page pops on its own");
                return base.OnBackButtonPressed();
            }

            // The navigation service decides: it evaluates the navigation guards and, if the navigation may
            // proceed, performs the pop through its own pipeline. If it takes over, the platform must not pop.
            // This also applies to the root page of the stack, which cannot be popped but may carry a guard.
            // When nothing is listening, the base implementation pops the stack, so a deep stack does not
            // background the application on the first back press.
            var handled = navigationHost.RequestBack(this.CurrentPage ?? this);
            logger?.LogDebug("Back button pressed on {PageType}: {Outcome}",
                (this.CurrentPage ?? this).GetType().Name, handled ? "taken over by the navigation service" : "not handled, the page pops on its own");

            return handled || base.OnBackButtonPressed();
        }
    }
}
