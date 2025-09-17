using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using SuperdevMauiDemoApp.Translations;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class ActivityIndicatorDemoViewModel : BaseViewModel
    {
        private readonly ILogger logger;
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private readonly IActivityIndicatorService activityIndicatorService;

        private IAsyncRelayCommand busyCommand;
        private bool isContentBusy;
        private IAsyncRelayCommand contentBusyCommand;
        private IAsyncRelayCommand serviceBusyCommand;

        public ActivityIndicatorDemoViewModel(
            ILogger<ActivityIndicatorDemoViewModel> logger,
            IViewModelErrorHandler viewModelErrorHandler,
            IActivityIndicatorService activityIndicatorService)
        {
            this.logger = logger;
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.activityIndicatorService = activityIndicatorService;

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
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to initialize viewmodel");
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        public IAsyncRelayCommand BusyCommand
        {
            get => this.busyCommand ??= new AsyncRelayCommand(this.BusyAsync);
        }

        private async Task BusyAsync()
        {
            this.IsBusy = true;
            await Task.Delay(3000);
            this.IsBusy = false;
        }

        public IAsyncRelayCommand ContentBusyCommand
        {
            get => this.contentBusyCommand ??= new AsyncRelayCommand(this.ContentBusyAsync);
        }

        private async Task ContentBusyAsync()
        {
            this.IsContentBusy = true;
            await Task.Delay(3000);
            this.IsContentBusy = false;
        }

        public bool IsContentBusy
        {
            get => this.isContentBusy;
            private set => this.SetProperty(ref this.isContentBusy, value);
        }

        public IAsyncRelayCommand ServiceBusyCommand
        {
            get => this.serviceBusyCommand ??= new AsyncRelayCommand(this.ServiceBusyAsync);
        }

        private async Task ServiceBusyAsync()
        {
            this.activityIndicatorService.ShowLoadingPage(Strings.LoadingText);
            await Task.Delay(2000);
            this.activityIndicatorService.ShowLoadingPage("Another loading message...");
            await Task.Delay(2000);
            this.activityIndicatorService.HideLoadingPage();
        }
    }
}