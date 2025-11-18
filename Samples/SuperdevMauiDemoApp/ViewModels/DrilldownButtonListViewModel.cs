using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;

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

        protected override async Task OnRefreshing()
        {
            await Task.Delay(2000);
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
            get => this.toggleSwitchCommand ??= new AsyncRelayCommand(this.ToggleSwitchAsync);
        }

        private async Task ToggleSwitchAsync()
        {
            try
            {
                this.IsBusy = true;

                await Task.Delay(2000);

                this.IsToggled = !this.IsToggled;
            }
            finally
            {
                this.IsBusy = false;
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
}