using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Localization;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Resources.Styles;
using Superdev.Maui.Services;
using IBrowser = Superdev.Maui.Services.IBrowser;
using IDeviceInfo = Superdev.Maui.Services.IDeviceInfo;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class ServiceDemoViewModel : BaseViewModel
    {
        private readonly ILogger logger;
        private readonly IStatusBarService statusBarService;
        private readonly IGeolocationSettings geolocationSettings;
        private readonly IDeviceInfo deviceInfo;
        private readonly IThemeHelper themeHelper;
        private readonly IDialogService dialogService;
        private readonly IBrowser browser;
        private readonly ILocalizer localizer;
        private readonly IViewModelErrorHandler viewModelErrorHandler;

        private IRelayCommand showGeolocationSettingsCommand;
        private IRelayCommand setStatusBarColorCommand;
        private IRelayCommand setNavigationBarColorCommand;
        private IRelayCommand resetNavigationBarColorCommand;
        private IRelayCommand setStatusBarStyleCommand;
        private Color currentStatusBarColor = Colors.Red;
        private Color currentNavigationBarColor = Colors.White;
        private StatusBarStyle currentStatusBarStyle = StatusBarStyle.Dark;
        private string deviceId;
        private bool useSystemTheme;
        private AppTheme platformAppTheme;
        private AppTheme userAppTheme;
        private AppTheme appTheme;
        private IRelayCommand resetThemeCommand;
        private IAsyncRelayCommand displayAlertCommand;
        private IAsyncRelayCommand displayActionSheetCommand;
        private IAsyncRelayCommand tryOpenUrlCommand;
        private IRelayCommand<string> setCurrentCultureCommand;
        private string currentCulture;
        private IRelayCommand resetCurrentCultureCommand;

        public ServiceDemoViewModel(
            ILogger<ServiceDemoViewModel> logger,
            IStatusBarService statusBarService,
            IGeolocationSettings geolocationSettings,
            IDeviceInfo deviceInfo,
            IThemeHelper themeHelper,
            IDialogService dialogService,
            IBrowser browser,
            ILocalizer localizer,
            IViewModelErrorHandler viewModelErrorHandler)
        {
            this.logger = logger;
            this.statusBarService = statusBarService;
            this.geolocationSettings = geolocationSettings;
            this.deviceInfo = deviceInfo;
            this.themeHelper = themeHelper;
            this.dialogService = dialogService;
            this.browser = browser;
            this.localizer = localizer;
            this.viewModelErrorHandler = viewModelErrorHandler;

            this.AppThemes = new[]
            {
                AppTheme.Unspecified,
                AppTheme.Light,
                AppTheme.Dark
            };
            this.appTheme = this.themeHelper.AppTheme;
            this.useSystemTheme = this.themeHelper.UseSystemTheme;
            this.themeHelper.ThemeChanged += this.OnThemeChanged;
            this.localizer.LanguageChanged += this.OnLanguageChanged;

            _ = this.InitializeAsync();
        }

        private void OnThemeChanged(object sender, AppTheme e)
        {
            this.RefreshThemeHelperValues();
        }

        private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            this.CurrentCulture = e.CultureInfo.Name;
        }

        public string CurrentCulture
        {
            get => this.currentCulture;
            private set => this.SetProperty(ref this.currentCulture, value);
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
                this.localizer.SupportedLanguages = SupportedLanguages.GetAll().ToArray();
                this.CurrentCulture = this.localizer.CurrentCulture.Name;
                this.DeviceId = this.deviceInfo.DeviceId;

                this.RefreshThemeHelperValues();
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        public IRelayCommand<string> SetCurrentCultureCommand
        {
            get => this.setCurrentCultureCommand ??= new RelayCommand<string>(this.SetCurrentCulture);
        }

        private void SetCurrentCulture(string locale)
        {
           this.localizer.CurrentCulture = new CultureInfo(locale);
        }

        public IRelayCommand ResetCurrentCultureCommand
        {
            get => this.resetCurrentCultureCommand ??= new RelayCommand(this.ResetCurrentCulture);
        }

        private void ResetCurrentCulture()
        {
           this.localizer.Reset();
        }

        public string DeviceId
        {
            get => this.deviceId;
            private set => this.SetProperty(ref this.deviceId, value);
        }

        public IRelayCommand SetStatusBarColorCommand
        {
            get => this.setStatusBarColorCommand ??= new RelayCommand(this.SetStatusBarColor);
        }

        private void SetStatusBarColor()
        {
            this.currentStatusBarColor = Equals(this.currentStatusBarColor, Colors.Yellow)
                ? Colors.Blue
                : Colors.Yellow;

            this.statusBarService.SetStatusBarColor(this.currentStatusBarColor);
        }

        public IRelayCommand SetNavigationBarColorCommand
        {
            get => this.setNavigationBarColorCommand ??= new RelayCommand(this.SetNavigationBarColor);
        }

        private void SetNavigationBarColor()
        {
            this.currentNavigationBarColor = Equals(this.currentNavigationBarColor, Colors.White)
                ? Colors.Magenta
                : Colors.White;

            this.statusBarService.SetNavigationBarColor(this.currentNavigationBarColor);
        }

        public IRelayCommand ResetNavigationBarColorCommand
        {
            get => this.resetNavigationBarColorCommand ??= new RelayCommand(this.ResetNavigationBarColor);
        }

        private void ResetNavigationBarColor()
        {
            this.statusBarService.SetNavigationBarColor(null);
        }

        public IRelayCommand SetStatusBarStyleCommand
        {
            get => this.setStatusBarStyleCommand ??= new RelayCommand(this.SetStatusBarStyle);
        }

        private void SetStatusBarStyle()
        {
            this.currentStatusBarStyle = this.currentStatusBarStyle == StatusBarStyle.Dark
                ? StatusBarStyle.Light
                : StatusBarStyle.Dark;

            this.statusBarService.SetStyle(this.currentStatusBarStyle);
        }

        private void RefreshThemeHelperValues()
        {
            this.useSystemTheme = this.themeHelper.UseSystemTheme;
            this.RaisePropertyChanged(nameof(this.UseSystemTheme));

            this.UserAppTheme = this.themeHelper.UserAppTheme;
            this.PlatformAppTheme = this.themeHelper.PlatformAppTheme;

            this.appTheme = this.themeHelper.AppTheme;
            this.RaisePropertyChanged(nameof(this.AppTheme));
        }

        public AppTheme[] AppThemes { get; }

        public AppTheme PlatformAppTheme
        {
            get => this.platformAppTheme;
            private set => this.SetProperty(ref this.platformAppTheme, value);
        }

        public AppTheme UserAppTheme
        {
            get => this.userAppTheme;
            private set => this.SetProperty(ref this.userAppTheme, value);
        }

        public bool UseSystemTheme
        {
            get => this.useSystemTheme;
            set
            {
                if (this.SetProperty(ref this.useSystemTheme, value))
                {
                    this.themeHelper.UseSystemTheme = value;
                    this.RefreshThemeHelperValues();
                }
            }
        }

        public AppTheme AppTheme
        {
            get => this.appTheme;
            set
            {
                if (this.SetProperty(ref this.appTheme, value))
                {
                    this.themeHelper.AppTheme = value;
                }
            }
        }

        public IRelayCommand ResetThemeCommand
        {
            get => this.resetThemeCommand ??= new RelayCommand(this.ResetTheme);
        }

        private void ResetTheme()
        {
            this.themeHelper.Reset();
            this.RefreshThemeHelperValues();
        }

        public IAsyncRelayCommand DisplayAlertCommand
        {
            get => this.displayAlertCommand ??= new AsyncRelayCommand(this.DisplayAlertAsync);
        }

        private async Task DisplayAlertAsync()
        {
            var result = await this.dialogService.DisplayAlertAsync("Title", "Message", "Confirm", "Cancel");
            this.logger.LogDebug($"DisplayAlertAsync: result={result}");
        }

        public IAsyncRelayCommand DisplayActionSheetCommand
        {
            get => this.displayActionSheetCommand ??= new AsyncRelayCommand(this.DisplayActionSheetAsync);
        }

        private async Task DisplayActionSheetAsync()
        {
            var buttons = new []
            {
                "Button1",
                "Button2"
            };
            var result = await this.dialogService.DisplayActionSheetAsync("Title", "Cancel", "Destruction", buttons);
            this.logger.LogDebug($"DisplayActionSheetAsync: result={result}");
        }

        public IRelayCommand ShowGeolocationSettingsCommand
        {
            get => this.showGeolocationSettingsCommand ??= new RelayCommand(this.ShowGeolocationSettings);
        }

        private void ShowGeolocationSettings()
        {
            this.geolocationSettings.ShowSettingsUI();
        }

        public IAsyncRelayCommand TryOpenUrlCommand
        {
            get => this.tryOpenUrlCommand ??= new AsyncRelayCommand(this.TryOpenUrlAsync);
        }

        private async Task TryOpenUrlAsync()
        {
            var result = await this.browser.TryOpenAsync("https://www.github.com/thomasgalliker");
            this.logger.LogDebug($"TryOpenUrlAsync: result={result}");
        }
    }
}