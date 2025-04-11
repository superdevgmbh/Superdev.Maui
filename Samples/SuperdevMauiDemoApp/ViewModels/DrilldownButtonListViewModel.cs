using System.Collections.ObjectModel;
using System.Windows.Input;
using Superdev.Maui.Controls;
using Superdev.Maui.Mvvm;
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
        private readonly IDisplayService displayService;

        public DrilldownButtonListViewModel(IDisplayService displayService)
        {
            this.displayService = displayService;

            this.DrilldownItems = new ObservableCollection<BindableBase>
            {
                new DrilldownSwitchViewModel(displayService, isToggled: true){ Title = "DrilldownSwitchView 1" },
                new DrilldownSwitchViewModel(displayService, isToggled: false){ Title = "DrilldownSwitchView 2" },
                new DrilldownButtonViewModel(displayService){ Title = "DrilldownButtonView 1"},
                new DrilldownButtonViewModel(displayService){ Title = "DrilldownButtonView 2"},
                new CustomDrilldownViewModel(displayService){ Title = "CustomDrilldownViewModel 1"},
                new CustomDrilldownViewModel(displayService){ Title = "CustomDrilldownViewModel 2", IsBusy=true },
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
            await this.displayService.DisplayAlert("DisplayAlert", "This is a test alert");
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
        public DrilldownBaseViewModel(IDisplayService displayService)
        {
            this.Command = new Command(() => { displayService.DisplayAlert(this.Title, "Command executed"); });
            this.IsBusy = false;
        }

        public bool IsEnabled { get; set; } = true;

        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }
    }

    public class DrilldownSwitchViewModel : DrilldownBaseViewModel, IDrilldownSwitchView
    {
        private readonly IDisplayService displayService;
        private bool isToggled;

        public DrilldownSwitchViewModel(IDisplayService displayService, bool isToggled) : base(displayService)
        {
            this.displayService = displayService;
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
                        this.displayService.DisplayAlert(this.Title, $"IsToggled={this.IsToggled}");
                    }
                }
            }
        }
    }

    public class DrilldownButtonViewModel : DrilldownBaseViewModel, IDrilldownButtonView
    {
        public DrilldownButtonViewModel(IDisplayService displayService) : base(displayService)
        {
        }
    }

    public class CustomDrilldownViewModel : BaseViewModel
    {
        public CustomDrilldownViewModel(IDisplayService displayService)
        {
            this.Command = new Command(() => { displayService.DisplayAlert(this.Title, "Command executed"); });
        }

        public ICommand Command { get; set; }
    }
}