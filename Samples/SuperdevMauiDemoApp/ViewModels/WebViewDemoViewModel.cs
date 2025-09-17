using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Mvvm;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class WebViewDemoViewModel : BaseViewModel
    {
        private readonly ILogger logger;
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private string url;
        private IDictionary<string, string> headers;
        private Command<WebNavigatingEventArgs> navigatingCommand;
        private Command<WebNavigatedEventArgs> navigatedCommand;

        public WebViewDemoViewModel(
            ILogger<WebViewDemoViewModel> logger,
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
                this.Headers = new Dictionary<string, string> { { "Accept-Language", "fr" } };
                this.Url = "https://www.google.com";
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to initialize viewmodel");
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        public string Url
        {
            get => this.url;
            set => this.SetProperty(ref this.url, value);
        }

        public IDictionary<string, string> Headers
        {
            get => this.headers;
            set => this.SetProperty(ref this.headers, value);
        }

        public ICommand NavigatingCommand
        {
            get => this.navigatingCommand ??= new Command<WebNavigatingEventArgs>(this.Navigating);
        }

        private void Navigating(WebNavigatingEventArgs args)
        {
            this.logger.LogDebug($"Navigating: {args.Url}");
        }

        public ICommand NavigatedCommand
        {
            get => this.navigatedCommand ??= new Command<WebNavigatedEventArgs>(this.Navigated);
        }

        private void Navigated(WebNavigatedEventArgs args)
        {
            this.logger.LogDebug($"Navigated: {args.Url}");
        }
    }
}