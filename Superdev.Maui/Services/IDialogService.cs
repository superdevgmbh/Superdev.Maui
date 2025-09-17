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
        public static IDialogService Current { get; } = DialogService.Current;

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
    }
}