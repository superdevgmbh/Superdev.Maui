using Superdev.Maui.Internals;

namespace Superdev.Maui.Services
{
    /// <summary>
    /// Presents MAUI's page dialogs for <see cref="DialogService"/>. Without a platform a page shows no dialog and never
    /// completes one, so tests stand in for this class.
    /// </summary>
    internal class PageDialogs
    {
        public virtual Task DisplayAlertAsync(Page page, string title, string message, string cancel)
        {
            return page.DisplayAlertCompatAsync(title, message, cancel);
        }

        public virtual Task<bool> DisplayAlertAsync(Page page, string title, string message, string accept, string cancel)
        {
            return page.DisplayAlertCompatAsync(title, message, accept, cancel);
        }

        public virtual Task<string> DisplayActionSheetAsync(Page page, string title, string? cancel, string? destruction, string[] buttons)
        {
            return page.DisplayActionSheetCompatAsync(title, cancel, destruction, buttons);
        }

        public virtual Task<string?> DisplayPromptAsync(
            Page page,
            string title,
            string message,
            string accept,
            string cancel,
            string? placeholder,
            int maxLength,
            Keyboard keyboard,
            string initialValue)
        {
            return page.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, keyboard, initialValue);
        }
    }
}
