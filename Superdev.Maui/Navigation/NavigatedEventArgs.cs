namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Provides data for the <see cref="INavigationService.Navigated"/> event.
    /// </summary>
    public class NavigatedEventArgs : EventArgs
    {
        /// <inheritdoc cref="NavigatedEventArgs"/>
        public NavigatedEventArgs(NavigationInfo request, TimeSpan duration)
        {
            this.Request = request;
            this.Duration = duration;
        }

        /// <summary>
        /// Gets the navigation operation which was performed.
        /// </summary>
        public NavigationInfo Request { get; }

        /// <summary>
        /// Gets the time the navigation operation took, not counting the time it waited for another
        /// operation to finish.
        /// </summary>
        public TimeSpan Duration { get; }
    }
}