namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Provides data for the <see cref="INavigationService.Navigating"/> event.
    /// </summary>
    public class NavigatingEventArgs : EventArgs
    {
        /// <inheritdoc cref="NavigatingEventArgs"/>
        public NavigatingEventArgs(NavigationInfo request)
        {
            this.Request = request;
        }

        /// <summary>
        /// Gets the navigation operation which is about to be performed.
        /// </summary>
        public NavigationInfo Request { get; }
    }
}