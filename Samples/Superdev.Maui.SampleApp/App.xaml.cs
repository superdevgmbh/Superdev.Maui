using Superdev.Maui.SampleApp.Views;

namespace Superdev.Maui.SampleApp
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();

            var mainPage = IPlatformApplication.Current.Services.GetService<MainPage>();
            this.MainPage = new NavigationPage(mainPage);

            Application.Current.UserAppTheme = AppTheme.Light;
            this.RequestedThemeChanged += (s, e) => { Application.Current.UserAppTheme = AppTheme.Light; };
        }
    }
}
