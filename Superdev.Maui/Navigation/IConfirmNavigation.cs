namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Allows a page to veto a navigation which would remove it from the navigation stack,
    /// e.g. in order to ask the user to save pending changes.
    /// </summary>
    /// <remarks>
    /// The guard is evaluated on every page an operation would remove: a backwards one (pop, modal
    /// dismissal, pop to root, <c>"../"</c> segments), including system back navigation, as well as a
    /// flyout detail replacement and an absolute navigation which replaces the whole page tree - a deep
    /// link, typically. It is not evaluated for forward navigation, because being covered by another page
    /// is not leaving.
    /// <para>
    /// If several pages are removed at once, all of them are evaluated, topmost first,
    /// and evaluation stops at the first veto.
    /// </para>
    /// <para>
    /// On iOS the interactive swipe back gesture cannot be canceled once it started.
    /// Therefore, the gesture is disabled for pages implementing this interface,
    /// see <see cref="NavigationOptions.DisableSwipeBackOnGuardedPages"/>, and a modal page implementing it
    /// is presented full screen rather than as a sheet which could be swiped away,
    /// see <see cref="NavigationOptions.PresentGuardedModalPagesFullScreen"/>.
    /// </para>
    /// </remarks>
    public interface IConfirmNavigation
    {
        /// <summary>
        /// Determines whether the page may be navigated away from.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the navigation may proceed; <c>false</c> to cancel the whole navigation,
        /// in which case the caller receives a <see cref="NavigationCancelledException"/>
        /// and no lifecycle callback is invoked.
        /// </returns>
        Task<bool> CanNavigateAsync();
    }
}
