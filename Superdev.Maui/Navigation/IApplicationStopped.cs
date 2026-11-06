namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Receives a callback when the application was stopped, i.e. when it is no longer visible
    /// because it was sent to the background.
    /// </summary>
    /// <remarks>
    /// The callback is delivered to <b>every</b> page which is currently alive - all navigation stacks,
    /// the detail page of a flyout page, all children of a tabbed page, the modal stack and any popups -
    /// so that a page below the visible one can release timers, subscriptions or hardware it holds.
    /// </remarks>
    public interface IApplicationStopped
    {
        /// <summary>
        /// Called when the application was sent to the background.
        /// </summary>
        Task OnStoppedAsync();
    }
}
