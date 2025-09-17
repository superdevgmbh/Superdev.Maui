using Superdev.Maui.Services;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class CustomDrilldownViewModel : DrilldownBaseViewModel
    {
        public CustomDrilldownViewModel(IDialogService dialogService)
            : base(dialogService)
        {
        }
    }
}