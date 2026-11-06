namespace Superdev.Maui.Navigation.Hosts
{
    /// <summary>
    /// Describes why a page was removed from a navigation or modal stack.
    /// </summary>
    public enum PageRemovalKind
    {
        /// <summary>
        /// The page was popped from a navigation stack.
        /// </summary>
        Popped,

        /// <summary>
        /// The page was removed because the navigation stack was popped to its root page.
        /// </summary>
        PoppedToRoot,

        /// <summary>
        /// The page was removed from the modal stack.
        /// </summary>
        ModalPopped,
    }
}