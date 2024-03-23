using Superdev.Maui.SampleApp.Views;

namespace Superdev.Maui.SampleApp
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();


/* Unmerged change from project 'Superdev.Maui.SampleApp (net8.0-android)'
Before:
            CrossPlatformLibrary.Forms.CrossPlatformLibrary.Init(this, "SampleApp.Theme");
After:
            Maui.CrossPlatformLibrary.Init(this, "SampleApp.Theme");
*/
            CrossPlatformLibrary.Init(this, "SampleApp.Theme");

            var mainPage = IPlatformApplication.Current.Services.GetService<MainPage>();
            this.MainPage = new NavigationPage(mainPage);

            Application.Current.UserAppTheme = AppTheme.Light;
            this.RequestedThemeChanged += (s, e) => { Application.Current.UserAppTheme = AppTheme.Light; };
        }
    }
}
