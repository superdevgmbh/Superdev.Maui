using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;

namespace Superdev.Maui.Extensions
{
    public static class DialogServiceExtensions
    {
        /// <summary>
        /// Displays a message box with the content provided in <paramref name="viewModelError"/>.
        /// If the <paramref name="viewModelError"/> is retryable, the dialog can be canceled or retried.
        /// Otherwise, the dialog can only be accepted.
        /// </summary>
        /// <param name="dialogService">The dialog service.</param>
        /// <param name="viewModelError">The viewmodel error.</param>
        /// <param name="ok">The button text to be used when a non-retryable viewmodel error is shown.</param>
        /// <param name="cancel">The button text to be used when a retryable viewmodel error is shown.</param>
        public static async Task DisplayAlertAsync(this IDialogService dialogService, ViewModelError viewModelError, string ok, string cancel)
        {
            if (viewModelError.CanRetry)
            {
                var retry = await dialogService.DisplayAlertAsync(
                    viewModelError.Title,
                    viewModelError.Text,
                    viewModelError.RetryButtonText,
                    cancel);

                if (retry)
                {
                    await viewModelError.RetryAsync();
                }
            }
            else
            {
                await dialogService.DisplayAlertAsync(
                    viewModelError.Title,
                    viewModelError.Text,
                    ok);
            }
        }
    }
}