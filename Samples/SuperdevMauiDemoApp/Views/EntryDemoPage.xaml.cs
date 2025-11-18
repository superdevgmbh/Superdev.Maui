using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

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

            var platformElementConfiguration = App.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>();
            this.originalWindowSoftInputModeAdjust = platformElementConfiguration.GetWindowSoftInputModeAdjust();
            platformElementConfiguration.UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            var platformElementConfiguration = App.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>();
            platformElementConfiguration.UseWindowSoftInputModeAdjust(this.originalWindowSoftInputModeAdjust);
        }
    }
}