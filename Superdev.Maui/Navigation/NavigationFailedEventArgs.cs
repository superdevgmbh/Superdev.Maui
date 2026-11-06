namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Provides data for the <see cref="INavigationService.NavigationFailed"/> event.
    /// </summary>
    public class NavigationFailedEventArgs : EventArgs
    {
        /// <inheritdoc cref="NavigationFailedEventArgs"/>
        public NavigationFailedEventArgs(NavigationInfo request, Exception exception)
        {
            this.Request = request;
            this.Exception = exception;
        }

        /// <summary>
        /// Gets the navigation operation which failed.
        /// </summary>
        public NavigationInfo Request { get; }

        /// <summary>
        /// Gets the exception which caused the failure.
        /// A <see cref="NavigationCancelledException"/> indicates that a navigation guard vetoed the navigation,
        /// which is an expected outcome rather than a defect.
        /// </summary>
        public Exception Exception { get; }
    }
}