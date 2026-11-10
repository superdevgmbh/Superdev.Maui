namespace Superdev.Maui.Navigation.Hosts
{
    /// <summary>
    /// Provides data for the <see cref="INavigationHost.TabSelectedExternally"/> event.
    /// </summary>
    public sealed class TabSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the tabbed page whose selected tab changed.
        /// </summary>
        public required TabbedPage TabbedPage { get; init; }

        /// <summary>
        /// Gets the tab which was selected before, if any.
        /// </summary>
        public Page? PreviousTab { get; init; }
    }
}
