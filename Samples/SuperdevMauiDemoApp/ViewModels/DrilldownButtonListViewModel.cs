using System.Collections.ObjectModel;
using System.Windows.Input;
using Superdev.Maui.Controls;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using SuperdevMauiDemoApp.Services;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class DrilldownButtonListViewModel : BaseViewModel
    {
        private int numberOfLoads = 0;
        private ICommand toggleSwitchCommand;
        private bool isToggled;
        private bool isNavigatingToTermsAndConditions;
        private bool isNavigatingToPrivacyPolicy;
        private readonly IDialogService dialogService;

        public DrilldownButtonListViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            this.DrilldownItems = new ObservableCollection<BindableBase>
            {
                new DrilldownSwitchViewModel(dialogService, isToggled: true){ Title = "DrilldownSwitchView 1" },
                new DrilldownSwitchViewModel(dialogService, isToggled: false){ Title = "DrilldownSwitchView 2" },
                new DrilldownButtonViewModel(dialogService){ Title = "DrilldownButtonView 1"},
                new DrilldownButtonViewModel(dialogService){ Title = "DrilldownButtonView 2"},
                new CustomDrilldownViewModel(dialogService){ Title = "CustomDrilldownViewModel 1"},
                new CustomDrilldownViewModel(dialogService){ Title = "CustomDrilldownViewModel 2", IsBusy=true },
            };
        }

        public string RefreshButtonText => $"Refresh (count: {this.numberOfLoads})";

        protected override async Task OnRefreshList()
        {
            await Task.Delay(1000);
            this.numberOfLoads++;
            this.RaisePropertyChanged(nameof(this.RefreshButtonText));
        }

        public ICommand DisplayAlertCommand => new Command(this.DisplayAlert);

        private async void DisplayAlert()
        {
            await Task.Delay(2000);
            await this.dialogService.DisplayAlertAsync("DisplayAlert", "This is a test alert", "OK");
        }


        public bool IsToggled
        {
            get => this.isToggled;
            set
            {
                if (this.SetProperty(ref this.isToggled, value))
                {
                    this.RaisePropertyChanged(nameof(this.ToggleSwitchButtonText));
                }
            }
        }

        public string ToggleSwitchButtonText
        {
            get => this.IsToggled ? "IsToggled: Yes" : "IsToggled: No";
        }

        public ICommand ToggleSwitchCommand
        {
            get
            {
                return this.toggleSwitchCommand ??= new Command(() => { this.IsToggled = !this.IsToggled; });
            }
        }

        public ObservableCollection<BindableBase> DrilldownItems { get; set; }

        public bool IsNavigatingToTermsAndConditions
        {
            get => this.isNavigatingToTermsAndConditions;
            set => this.SetProperty(ref this.isNavigatingToTermsAndConditions, value);
        }

        public bool IsNavigatingToPrivacyPolicy
        {
            get => this.isNavigatingToPrivacyPolicy;
            set => this.SetProperty(ref this.isNavigatingToPrivacyPolicy, value);
        }
    }

    public abstract class DrilldownBaseViewModel : BaseViewModel, IDrilldownView
    {
        public DrilldownBaseViewModel(IDialogService dialogService)
        {
            this.Command = new Command(() => { dialogService.DisplayAlertAsync(this.Title, "Command executed", "OK"); });
            this.IsBusy = false;
        }

        public bool IsEnabled { get; set; } = true;

        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }
    }

    public class DrilldownSwitchViewModel : DrilldownBaseViewModel, IDrilldownSwitchView
    {
        private readonly IDialogService dialogService;
        private bool isToggled;

        public DrilldownSwitchViewModel(IDialogService dialogService, bool isToggled) : base(dialogService)
        {
            this.dialogService = dialogService;
            this.isToggled = isToggled;
        }

        public bool IsToggled
        {
            get => this.isToggled;
            set
            {
                if (this.SetProperty(ref this.isToggled, value, nameof(this.IsToggled)))
                {
                    if (this.IsNotBusy)
                    {
                        this.dialogService.DisplayAlertAsync(this.Title, $"IsToggled={this.IsToggled}", "OK");
                    }
                }
            }
        }
    }

    public class DrilldownButtonViewModel : DrilldownBaseViewModel, IDrilldownButtonView
    {
        public DrilldownButtonViewModel(IDialogService dialogService) : base(dialogService)
        {
        }
    }

    public class CustomDrilldownViewModel : BaseViewModel
    {
        public CustomDrilldownViewModel(IDialogService dialogService)
        {
            this.Command = new Command(() => { dialogService.DisplayAlertAsync(this.Title, "Command executed", "OK"); });
        }

        public ICommand Command { get; set; }
    }
}