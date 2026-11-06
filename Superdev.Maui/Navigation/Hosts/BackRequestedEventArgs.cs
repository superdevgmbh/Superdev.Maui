namespace Superdev.Maui.Navigation.Hosts
{
    /// <summary>
    /// Provides data for the <see cref="INavigationHost.BackRequested"/> event.
    /// </summary>
    public sealed class BackRequestedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the page the back navigation would remove.
        /// </summary>
        public required Page Page { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether the back navigation is prevented.
        /// </summary>
        public bool Cancel { get; set; }
    }
}