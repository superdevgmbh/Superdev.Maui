using Superdev.Maui;
using Superdev.Maui.Controls;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
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

            IActivityIndicatorService.Current.Init(new DefaultActivityIndicatorPage());

            var viewModelErrorRegistry = IViewModelErrorRegistry.Current;
            RegisterViewModelErrors(viewModelErrorRegistry);

            var mainPage = IPlatformApplication.Current.Services.GetService<MainPage>();
            this.MainPage = new NavigationPage(mainPage);

            Application.Current.UserAppTheme = AppTheme.Light;
            this.RequestedThemeChanged += (s, e) => { Application.Current.UserAppTheme = AppTheme.Light; };
        }

        private static void RegisterViewModelErrors(IViewModelErrorRegistry viewModelErrorRegistry)
        {
            viewModelErrorRegistry.SetDefaultFactory(
                ex => new ViewModelError(
                    "rectangle_magenta_192",
                    ex.Message,
                    $"{ex}",
                    "Retry"));

            viewModelErrorRegistry.RegisterException(ex => ex is HttpRequestException,
                () => new ViewModelError(
                    "rectangle_magenta_192",
                    "Strings.ErrorMessage_ApiClientError_Title",
                    "Strings.ErrorMessage_ApiClientError_Body",
                    "Retry"));
        }
    }
}
