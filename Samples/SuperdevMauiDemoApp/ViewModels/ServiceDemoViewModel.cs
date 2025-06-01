using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class ServiceDemoViewModel : BaseViewModel
    {
        private readonly IStatusBarService statusBarService;
        private readonly IGeolocationSettings geolocationSettings;
        private readonly IViewModelErrorHandler viewModelErrorHandler;

        private IRelayCommand showGeolocationSettingsCommand;

        public ServiceDemoViewModel(
            IStatusBarService statusBarService,
            IGeolocationSettings geolocationSettings,
            IViewModelErrorHandler viewModelErrorHandler)
        {
            this.statusBarService = statusBarService;
            this.geolocationSettings = geolocationSettings;
            this.viewModelErrorHandler = viewModelErrorHandler;

            _ = this.InitializeAsync();
        }
        private async Task InitializeAsync()
        {
            try
            {
                await this.LoadData();
            }
            finally
            {
                this.IsInitialized = true;
            }
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
                await Task.Delay(1000);
                this.statusBarService.SetStatusBarMode(StatusBarStyle.Dark);
                this.statusBarService.SetColor(Colors.Magenta);

                await Task.Delay(1000);
                this.statusBarService.SetStatusBarMode(StatusBarStyle.Light);
                this.statusBarService.SetColor(Colors.DeepSkyBlue);
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        public IRelayCommand ShowGeolocationSettingsCommand
        {
            get => this.showGeolocationSettingsCommand ??= new RelayCommand(this.ShowGeolocationSettings);
        }

        private void ShowGeolocationSettings()
        {
            this.geolocationSettings.ShowSettingsUI();
        }
    }
}