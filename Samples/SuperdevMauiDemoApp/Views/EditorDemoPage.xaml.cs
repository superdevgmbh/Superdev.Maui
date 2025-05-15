using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Superdev.Maui.Services;

namespace SuperdevMauiDemoApp.Views
{
    public partial class EditorDemoPage : ContentPage
    {
        private readonly IKeyboardService keyboardService;

        public EditorDemoPage(IKeyboardService keyboardService)
        {
            this.keyboardService = keyboardService;
            this.InitializeComponent();
        }

        protected override void OnAppearing()
        {
            this.keyboardService.UseWindowSoftInputModeAdjust(this, WindowSoftInputModeAdjust.Resize);
        }

        protected override void OnDisappearing()
        {
            this.keyboardService.ResetWindowSoftInputModeAdjust(this);
        }
    }
}