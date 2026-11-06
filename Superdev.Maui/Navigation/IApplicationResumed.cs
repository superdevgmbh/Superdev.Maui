namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Receives a callback when the application was resumed, i.e. when it returned to the foreground.
    /// </summary>
    /// <remarks>
    /// The callback is delivered to <b>every</b> page which is currently alive - all navigation stacks,
    /// the detail page of a flyout page, all children of a tabbed page, the modal stack and any popups -
    /// because returning to the foreground concerns every page which holds a resource, not just the visible one.
    /// <para>
    /// A view model which only wants to act while its page is the visible one can track that itself:
    /// set a flag in <see cref="INavigatedTo"/>, clear it in <see cref="INavigatedFrom"/> and check it here.
    /// </para>
    /// </remarks>
    public interface IApplicationResumed
    {
        /// <summary>
        /// Called when the application was resumed.
        /// </summary>
        Task OnResumedAsync();
    }
}
