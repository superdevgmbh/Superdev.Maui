namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Receives a callback every time the page stops being the active page.
    /// </summary>
    public interface INavigatedFrom
    {
        /// <summary>
        /// Called when the page stopped being the active page.
        /// </summary>
        /// <param name="navigationDirection">
        /// <see cref="NavigationDirection.Forward"/> if the page is covered by another page or replaced,
        /// <see cref="NavigationDirection.Back"/> if the page is removed by a backwards navigation.
        /// </param>
        Task NavigatedFromAsync(NavigationDirection navigationDirection);
    }
}
