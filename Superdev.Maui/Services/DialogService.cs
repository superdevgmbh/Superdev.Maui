namespace Superdev.Maui.Services
{
    public class DialogService : IDialogService
    {
        private static readonly Lazy<IDialogService> Implementation = new Lazy<IDialogService>(CreateDialogService, LazyThreadSafetyMode.PublicationOnly);

        public static IDialogService Current => Implementation.Value;

        private static IDialogService CreateDialogService()
        {
            return new DialogService(IMainThread.Current, DeviceInfo.Current);
        }

        private readonly IMainThread mainThread;
        private readonly IDeviceInfo deviceInfo;

        private DialogService(
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