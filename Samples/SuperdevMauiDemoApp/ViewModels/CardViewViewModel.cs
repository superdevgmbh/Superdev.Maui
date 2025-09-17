using Microsoft.Extensions.Logging;
using Superdev.Maui.Mvvm;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class CardViewViewModel : BaseViewModel
    {
        private readonly ILogger logger;
        private readonly IViewModelErrorHandler viewModelErrorHandler;

        public CardViewViewModel(
            ILogger<CardViewViewModel> logger,
            IViewModelErrorHandler viewModelErrorHandler)
        {
            this.logger = logger;
            this.viewModelErrorHandler = viewModelErrorHandler;

            this.PeriodicTask = new PeriodicTaskViewModel();

            _ = this.InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
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
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "LoadData failed with exception");
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        public PeriodicTaskViewModel PeriodicTask { get; private set; }
    }
}