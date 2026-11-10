namespace Superdev.Maui.Services
{
    /// <summary>
    /// A button of an action sheet, so that the caller does not have to compare the returned text
    /// in order to find out what the user picked.
    /// </summary>
    public interface IActionSheetButton
    {
        /// <summary>
        /// The text which is displayed on the button.
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Indicates whether this is the button which dismisses the action sheet.
        /// </summary>
        bool IsCancel { get; }

        /// <summary>
        /// Indicates whether this button performs a destructive action, which the platform highlights.
        /// </summary>
        bool IsDestroy { get; }

        /// <summary>
        /// Runs the action of this button. Does nothing if the button was created without one.
        /// </summary>
        Task PressAsync();
    }
}
