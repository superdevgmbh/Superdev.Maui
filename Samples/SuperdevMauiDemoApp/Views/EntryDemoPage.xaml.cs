using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Application = Microsoft.Maui.Controls.Application;

namespace SuperdevMauiDemoApp.Views
{
    public partial class EntryDemoPage : ContentPage
    {
        private WindowSoftInputModeAdjust originalWindowSoftInputModeAdjust;

        public EntryDemoPage()
        {
            this.InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var platformElementConfiguration = Application.Current!.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>();
            this.originalWindowSoftInputModeAdjust = platformElementConfiguration.GetWindowSoftInputModeAdjust();
            platformElementConfiguration.UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            var platformElementConfiguration = Application.Current!.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>();
            platformElementConfiguration.UseWindowSoftInputModeAdjust(this.originalWindowSoftInputModeAdjust);
        }
    }
}