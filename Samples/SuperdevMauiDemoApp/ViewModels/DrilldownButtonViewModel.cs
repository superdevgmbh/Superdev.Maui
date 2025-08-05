using Superdev.Maui.Controls;
using Superdev.Maui.Services;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class DrilldownButtonViewModel : DrilldownBaseViewModel, IDrilldownButtonView
    {
        public DrilldownButtonViewModel(IDialogService dialogService) : base(dialogService)
        {
        }
    }
}