using Superdev.Maui;
using Superdev.Maui.Mvvm;
using SuperdevMauiDemoApp.Translations;
using SuperdevMauiDemoApp.Views;

namespace SuperdevMauiDemoApp
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();

            CrossPlatformLibrary.Init(this, "SampleApp.Theme");

            var activityIndicatorService = IPlatformApplication.Current.Services.GetService<IActivityIndicatorService>();
            activityIndicatorService.Init(new DefaultActivityIndicatorPage());

            var viewModelErrorRegistry = IPlatformApplication.Current.Services.GetService<IViewModelErrorRegistry>();
            RegisterViewModelErrors(viewModelErrorRegistry);

            var mainPage = IPlatformApplication.Current.Services.GetService<MainPage>();
            this.MainPage = new NavigationPage(mainPage);

            Application.Current.UserAppTheme = AppTheme.Light;
            this.RequestedThemeChanged += (s, e) => { Application.Current.UserAppTheme = AppTheme.Light; };
        }

        private static void RegisterViewModelErrors(IViewModelErrorRegistry viewModelErrorRegistry)
        {
            viewModelErrorRegistry.SetDefaultFactory(
                _ => new ViewModelError(
                    "rectangle_magenta_192",
                    "Strings.ViewModelError_DefaultError_Title",
                    "Strings.ViewModelError_DefaultError_Text",
                    "Strings.ViewModelError_RetryButtonText"));

            viewModelErrorRegistry.RegisterException(ex => ex is HttpRequestException,
                () => new ViewModelError(
                    "rectangle_magenta_192",
                    "Strings.ErrorMessage_ApiClientError_Title",
                    "Strings.ErrorMessage_ApiClientError_Body",
                    "Strings.ViewModelError_RetryButtonText"));
        }
    }
}
