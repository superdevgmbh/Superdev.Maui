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
        private IRelayCommand setStatusBarColorCommand;
        private IRelayCommand setStatusBarModeCommand;
        private Color currentStatusBarColor = Colors.Red;
        private StatusBarStyle currentStatusBarStyle = StatusBarStyle.Dark;

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
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        public IRelayCommand SetStatusBarColorCommand
        {
            get => this.setStatusBarColorCommand ??= new RelayCommand(this.SetStatusBarColor);
        }

        private void SetStatusBarColor()
        {
            this.currentStatusBarColor = Equals(this.currentStatusBarColor, Colors.Red) ? Colors.Blue : Colors.Red;
            this.statusBarService.SetColor(this.currentStatusBarColor);
        }

        public IRelayCommand SetStatusBarModeCommand
        {
            get => this.setStatusBarModeCommand ??= new RelayCommand(this.SetStatusBarMode);
        }

        private void SetStatusBarMode()
        {
            this.currentStatusBarStyle = this.currentStatusBarStyle == StatusBarStyle.Dark ? StatusBarStyle.Light : StatusBarStyle.Dark;
            this.statusBarService.SetStyle(this.currentStatusBarStyle);
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