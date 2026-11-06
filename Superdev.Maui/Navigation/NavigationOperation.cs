namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Describes the kind of a navigation operation.
    /// </summary>
    public enum NavigationOperation
    {
        /// <summary>A page was pushed onto the navigation stack.</summary>
        Push,

        /// <summary>A page was pushed onto the modal stack.</summary>
        PushModal,

        /// <summary>A page was popped from the navigation stack.</summary>
        Pop,

        /// <summary>A page was popped from the modal stack.</summary>
        PopModal,

        /// <summary>The navigation stack was popped to its root page.</summary>
        PopToRoot,

        /// <summary>
        /// A back navigation to a named page of the navigation stack.
        /// </summary>
        PopTo,

        /// <summary>
        /// The removal of a page which is not the visible one.
        /// </summary>
        RemovePage,

        /// <summary>A multi-segment navigation was performed.</summary>
        Navigate,

        /// <summary>Another tab of a tabbed page was selected.</summary>
        SelectTab,

        /// <summary>A page was removed by a back navigation which the user triggered.</summary>
        SystemBack,

        /// <summary>
        /// The application returned to the foreground and <see cref="IApplicationResumed"/> was delivered
        /// to every live page. The operation does not move between pages.
        /// </summary>
        ApplicationResumed,

        /// <summary>
        /// The application was sent to the background and <see cref="IApplicationStopped"/> was delivered
        /// to every live page. The operation does not move between pages.
        /// </summary>
        ApplicationStopped,
    }
}