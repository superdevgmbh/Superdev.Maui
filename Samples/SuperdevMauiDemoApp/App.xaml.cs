using Superdev.Maui.Controls;
using Superdev.Maui.Extensions;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Resources.Styles;
using Superdev.Maui.Services;
using SuperdevMauiDemoApp.Views;

namespace SuperdevMauiDemoApp
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();

            var themeHelper = IThemeHelper.Current;
            themeHelper.OverrideStyles = true;
            themeHelper.ApplyTheme(
                lightTheme: "SampleApp.Theme.Light",
                darkTheme: "SampleApp.Theme.Dark");

            IActivityIndicatorService.Current.Init(new DefaultActivityIndicatorPage());

            var viewModelErrorRegistry = IViewModelErrorRegistry.Current;
            RegisterViewModelErrors(viewModelErrorRegistry);

            var mainPage = IPlatformApplication.Current.Services.GetService<MainPage>();
            this.MainPage = new NavigationPage(mainPage);
        }

        protected override void OnStart()
        {
            var statusBarService = IStatusBarService.Current;
            var statusBarColor = (Color)App.Current.Resources["PrimaryDark"];
            statusBarService.SetStatusBarColor(statusBarColor);
            statusBarService.SetStyle(StatusBarStyle.Dark);
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
