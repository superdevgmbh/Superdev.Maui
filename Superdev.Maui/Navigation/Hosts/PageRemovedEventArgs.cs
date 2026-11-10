namespace Superdev.Maui.Navigation.Hosts
{
    /// <summary>
    /// Provides data for the <see cref="INavigationHost.PageRemovedExternally"/> event.
    /// </summary>
    public sealed class PageRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the page which was removed.
        /// </summary>
        public required Page Page { get; init; }

        /// <summary>
        /// Gets the reason why the page was removed.
        /// </summary>
        public required PageRemovalKind Kind { get; init; }
    }
}
