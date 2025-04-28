namespace Superdev.Maui.Services
{
    public class DialogService : IDialogService
    {
        private readonly IMainThread mainThread;
        private readonly IDeviceInfo deviceInfo;

        public DialogService(
            IMainThread mainThread,
            IDeviceInfo deviceInfo)
        {
            this.mainThread = mainThread;
            this.deviceInfo = deviceInfo;
        }

        public Task DisplayAlertAsync(string title, string message, string cancel)
        {
            return this.mainThread.InvokeOnMainThreadAsync(
                () => GetMainPage().DisplayAlert(title, message, cancel));
        }

        public Task<bool> DisplayAlertAsync(string title, string message, string confirm, string cancel)
        {
            return this.mainThread.InvokeOnMainThreadAsync(
                () => GetMainPage().DisplayAlert(title, message, confirm, cancel));
        }

        public Task<bool> DisplayDestructiveAlertAsync(string title, string message, string destructiveButton, string cancelButton)
        {
            return this.mainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (this.deviceInfo.Platform == DevicePlatform.iOS)
                {
                    var result = await GetMainPage().DisplayActionSheet(message, cancelButton, destructiveButton);
                    return result == destructiveButton;
                }
                else
                {
                    var result = await GetMainPage().DisplayAlert(title, message, destructiveButton, cancelButton);
                    return result;
                }
            });
        }

        public Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons)
        {
            return this.mainThread.InvokeOnMainThreadAsync(
                () => GetMainPage().DisplayActionSheet(title, cancel, destruction, buttons));
        }

        private static Page GetMainPage()
        {
            return Application.Current.MainPage;
        }
    }
}