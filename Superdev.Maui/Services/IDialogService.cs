namespace Superdev.Maui.Services
{
    /// <summary>
    /// Defines methods for displaying dialogs and action sheets asynchronously.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IDialogService"/>.
        /// </summary>
        public static IDialogService Current => DialogService.Current;

        /// <summary>
        /// Displays a simple dialog with a title, message, and a single cancel button.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message content of the dialog.</param>
        /// <param name="cancel">The text for the cancel button.</param>
        Task DisplayAlertAsync(string title, string message, string cancel);

        /// <summary>
        /// Displays a confirmation dialog with confirm and cancel options.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message content of the dialog.</param>
        /// <param name="confirm">The text for the confirm button.</param>
        /// <param name="cancel">The text for the cancel button.</param>
        /// <returns>A task that returns true if confirmed, otherwise false.</returns>
        Task<bool> DisplayAlertAsync(string title, string message, string confirm, string cancel);

        /// <summary>
        /// Displays a destructive action dialog with destructive and cancel options.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message content of the dialog.</param>
        /// <param name="destructiveButton">The text for the destructive action button.</param>
        /// <param name="cancelButton">The text for the cancel button.</param>
        /// <returns>A task that returns true if destructive action is confirmed, otherwise false.</returns>
        Task<bool> DisplayDestructiveAlertAsync(string title, string message, string destructiveButton, string cancelButton);

        /// <summary>
        /// Displays an action sheet allowing the user to select from multiple options.
        /// </summary>
        /// <param name="title">The title of the action sheet.</param>
        /// <param name="cancel">The text for the cancel button.</param>
        /// <param name="destruction">The text for the destructive action button.</param>
        /// <param name="buttons">An array of additional action options.</param>
        /// <returns>A task that returns the selected option as a string.</returns>
        Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons);

        /// <summary>
        /// Displays an action sheet whose buttons carry their own role and action, so that the caller does
        /// not have to compare the returned text.
        /// </summary>
        /// <param name="title">The title of the action sheet.</param>
        /// <param name="buttons">
        /// The buttons to display. At most one of them may be the cancel button and at most one of them may
        /// be the destructive button.
        /// </param>
        /// <returns>
        /// The button the user pressed, or <c>null</c> if the action sheet was dismissed without a choice.
        /// The action of the pressed button ran before this method returns.
        /// </returns>
        Task<IActionSheetButton?> DisplayActionSheetAsync(string title, params IActionSheetButton[] buttons);

        /// <summary>
        /// Displays a dialog which asks the user for a single line of text.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message of the dialog.</param>
        /// <param name="accept">The text of the accept button.</param>
        /// <param name="cancel">The text of the cancel button.</param>
        /// <param name="placeholder">The placeholder of the text field, or <c>null</c> for none.</param>
        /// <param name="maxLength">The maximum number of characters, or <c>-1</c> for no limit.</param>
        /// <param name="keyboard">The keyboard to display, or <c>null</c> for the default keyboard.</param>
        /// <param name="initialValue">The text the field starts with.</param>
        /// <returns>The text the user entered, or <c>null</c> if the dialog was cancelled.</returns>
        Task<string?> DisplayPromptAsync(
            string title,
            string message,
            string accept = "OK",
            string cancel = "Cancel",
            string? placeholder = null,
            int maxLength = -1,
            Keyboard? keyboard = null,
            string initialValue = "");
    }
}