using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class BehaviorDemoViewModel : BaseViewModel
    {
        private readonly IDialogService dialogService;
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private IAsyncRelayCommand appearingCommand;

        public BehaviorDemoViewModel(
            IDialogService dialogService,
            IViewModelErrorHandler viewModelErrorHandler)
        {
            this.dialogService = dialogService;
            this.viewModelErrorHandler = viewModelErrorHandler;
        }

        public IAsyncRelayCommand AppearingCommand
        {
            get => this.appearingCommand ??= new AsyncRelayCommand(this.InitializeAsync);
        }

        private async Task InitializeAsync()
        {
            try
            {
                await this.dialogService.DisplayAlertAsync(
                    "Appearing",
                    "Appearing event has triggered AppearingCommand",
                    "OK");
                await this.LoadData();
            }
            finally
            {
                this.IsInitialized = true;
            }
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }
    }
}