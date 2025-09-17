using Microsoft.Extensions.Logging;
using Superdev.Maui.Mvvm;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class ViewModelErrorDemoViewModel : BaseViewModel
    {
        private readonly ILogger logger;
        private readonly IViewModelErrorHandler viewModelErrorHandler;

        public ViewModelErrorDemoViewModel(
            ILogger<ViewModelErrorDemoViewModel> logger,
            IViewModelErrorHandler viewModelErrorHandler)
        {
            this.logger = logger;
            this.viewModelErrorHandler = viewModelErrorHandler;

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
                await Task.Delay(1000);
                throw new InvalidOperationException("Test");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to initialize viewmodel");
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

    }
}