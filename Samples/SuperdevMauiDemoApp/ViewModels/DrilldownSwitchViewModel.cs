using Superdev.Maui.Controls;
using Superdev.Maui.Services;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class DrilldownSwitchViewModel : DrilldownBaseViewModel, IDrilldownSwitchView
    {
        private readonly IDialogService dialogService;
        private bool isToggled;

        public DrilldownSwitchViewModel(IDialogService dialogService, bool isToggled)
            : base(dialogService)
        {
            this.dialogService = dialogService;
            this.isToggled = isToggled;
        }

        public bool IsToggled
        {
            get => this.isToggled;
            set
            {
                if (this.SetProperty(ref this.isToggled, value))
                {
                    if (this.IsNotBusy)
                    {
                        this.dialogService.DisplayAlertAsync(this.Title, $"IsToggled={this.IsToggled}", "OK");
                    }
                }
            }
        }
    }
}