namespace Superdev.Maui.Controls.Animations
{
    /// <summary>
    /// Animates a <see cref="PopupPage"/> when it appears and when it disappears.
    /// </summary>
    /// <remarks>
    /// The animation is applied to the content of the popup page, not to the page itself,
    /// so that the dimmed background can be faded independently.
    /// </remarks>
    public interface IPopupAnimation
    {
        /// <summary>
        /// Puts the content into its initial state, before the popup page is displayed.
        /// </summary>
        void Prepare(View content);

        /// <summary>
        /// Resets the content to its normal state, after the popup page was removed.
        /// </summary>
        void Reset(View content);

        /// <summary>
        /// Animates the content into view.
        /// </summary>
        Task AppearingAsync(View content, Page page);

        /// <summary>
        /// Animates the content out of view.
        /// </summary>
        Task DisappearingAsync(View content, Page page);
    }
}
