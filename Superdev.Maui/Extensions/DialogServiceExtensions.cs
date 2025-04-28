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
        public static async Task ShowDialogAsync(this IDialogService dialogService, ViewModelError viewModelError, string cancelOkButtonText)
        {
            if (viewModelError.CanRetry)
            {
                var retry = await dialogService.ShowDialogAsync(
                    viewModelError.Title,
                    viewModelError.Text,
                    viewModelError.RetryButtonText,
                    cancelOkButtonText);

                if (retry)
                {
                    await viewModelError.RetryAsync();
                }
            }
            else
            {
                await dialogService.ShowDialogAsync(
                    viewModelError.Title,
                    viewModelError.Text,
                    cancelOkButtonText);
            }
        }
    }
}