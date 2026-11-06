namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Indicates the direction in which a page is reached or left by a navigation operation.
    /// </summary>
    public enum NavigationDirection
    {
        /// <summary>
        /// The page newly entered the navigation stack or became active moving forward,
        /// e.g. by a push, a modal push, a root reset, a deep link or a tab selection.
        /// </summary>
        Forward,

        /// <summary>
        /// The page was revealed or removed by a backwards operation,
        /// e.g. by a pop, a modal dismissal, a pop to root or a system back navigation.
        /// </summary>
        Back,

        /// <summary>
        /// The operation does not move between pages, e.g. the delivery of an application lifecycle event.
        /// </summary>
        None,
    }
}
