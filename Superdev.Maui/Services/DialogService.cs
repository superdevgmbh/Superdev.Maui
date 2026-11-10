namespace Superdev.Maui.Services
{
    public class DialogService : IDialogService
    {
        private static readonly Lazy<IDialogService> Implementation = new Lazy<IDialogService>(CreateDialogService, LazyThreadSafetyMode.PublicationOnly);

        public static IDialogService Current => Implementation.Value;

        private static IDialogService CreateDialogService()
        {
            return new DialogService(IMainThread.Current, DeviceInfo.Current, GetCurrentPage, new PageDialogs());
        }

        private readonly IMainThread mainThread;
        private readonly IDeviceInfo deviceInfo;
        private readonly Func<Page> currentPage;
        private readonly PageDialogs pageDialogs;

        /// <summary>
        /// The library uses a single instance, <see cref="Current"/>. Tests create their own with a page and page
        /// dialogs that stand in for the platform.
        /// </summary>
        internal DialogService(
            IMainThread mainThread,
            IDeviceInfo deviceInfo,
            Func<Page> currentPage,
            PageDialogs pageDialogs)
        {
            this.mainThread = mainThread;
            this.deviceInfo = deviceInfo;
            this.currentPage = currentPage;
            this.pageDialogs = pageDialogs;
        }

        public Task DisplayAlertAsync(string title, string message, string cancel)
        {
            return this.mainThread.InvokeOnMainThreadAsync(
                () => this.pageDialogs.DisplayAlertAsync(this.currentPage(), title, message, cancel));
        }

        public Task<bool> DisplayAlertAsync(string title, string message, string confirm, string cancel)
        {
            return this.mainThread.InvokeOnMainThreadAsync(
                () => this.pageDialogs.DisplayAlertAsync(this.currentPage(), title, message, confirm, cancel));
        }

        public Task<bool> DisplayDestructiveAlertAsync(string title, string message, string destructiveButton, string cancelButton)
        {
            return this.mainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (this.deviceInfo.Platform == DevicePlatform.iOS)
                {
                    // An action sheet has a single title, so the title and the message are both shown there.
                    var sheetTitle = string.IsNullOrEmpty(title) ? message
                        : string.IsNullOrEmpty(message) ? title
                        : $"{title}{Environment.NewLine}{message}";

                    var result = await this.pageDialogs.DisplayActionSheetAsync(this.currentPage(), sheetTitle, cancelButton, destructiveButton, []);
                    return result == destructiveButton;
                }
                else
                {
                    var result = await this.pageDialogs.DisplayAlertAsync(this.currentPage(), title, message, destructiveButton, cancelButton);
                    return result;
                }
            });
        }

        public Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons)
        {
            return this.mainThread.InvokeOnMainThreadAsync(
                () => this.pageDialogs.DisplayActionSheetAsync(this.currentPage(), title, cancel, destruction, buttons));
        }

        public Task<IActionSheetButton?> DisplayActionSheetAsync(string title, params IActionSheetButton[] buttons)
        {
            ArgumentNullException.ThrowIfNull(buttons);

            var cancelButtons = buttons.Where(b => b.IsCancel).ToArray();
            var destroyButtons = buttons.Where(b => b.IsDestroy).ToArray();

            if (cancelButtons.Length > 1 || destroyButtons.Length > 1)
            {
                throw new ArgumentException(
                    "An action sheet can have at most one cancel button and at most one destructive button.",
                    nameof(buttons));
            }

            var cancelButton = cancelButtons.FirstOrDefault();
            var destroyButton = destroyButtons.FirstOrDefault();
            var otherButtons = buttons.Where(b => !b.IsCancel && !b.IsDestroy).ToArray();

            return this.mainThread.InvokeOnMainThreadAsync(async () =>
            {
                var pressedText = await this.pageDialogs.DisplayActionSheetAsync(
                    this.currentPage(),
                    title,
                    cancelButton?.Text,
                    destroyButton?.Text,
                    otherButtons.Select(b => b.Text).ToArray());

                // The platform returns null when the action sheet was dismissed without a choice.
                if (pressedText == null)
                {
                    return null;
                }

                var pressedButton = buttons.FirstOrDefault(b => b.Text == pressedText);
                if (pressedButton != null)
                {
                    await pressedButton.PressAsync();
                }

                return pressedButton;
            });
        }

        public Task<string?> DisplayPromptAsync(
            string title,
            string message,
            string accept = "OK",
            string cancel = "Cancel",
            string? placeholder = null,
            int maxLength = -1,
            Keyboard? keyboard = null,
            string initialValue = "")
        {
            return this.mainThread.InvokeOnMainThreadAsync(
                () => this.pageDialogs.DisplayPromptAsync(
                    this.currentPage(), title, message, accept, cancel, placeholder, maxLength, keyboard ?? Keyboard.Default, initialValue));
        }

        /// <summary>
        /// Gets the page the dialog is displayed on: the page the user currently sees, which is the topmost
        /// popup or modal page if one is displayed. This is the same page the navigation service operates on,
        /// so that a dialog is never presented from a page which is covered by another one.
        /// </summary>
        private static Page GetCurrentPage()
        {
            return Navigation.INavigationService.Current.CurrentPage
                ?? Application.Current?.Windows.ElementAtOrDefault(0)?.Page
                ?? throw new InvalidOperationException("Failed to resolve the current page");
        }
    }
}