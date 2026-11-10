using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Mvvm;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class WebViewDemoViewModel : BaseViewModel
    {
        private readonly ILogger logger;
        private readonly IViewModelErrorHandler viewModelErrorHandler;

        private string? url;
        private IDictionary<string, string> headers = new Dictionary<string, string>();
        private Command<WebNavigatingEventArgs>? navigatingCommand;
        private Command<WebNavigatedEventArgs>? navigatedCommand;
        private bool allowMixedContent;
        private HtmlWebViewSource? mixedContentSource;

        public WebViewDemoViewModel(
            ILogger<WebViewDemoViewModel> logger,
            IViewModelErrorHandler viewModelErrorHandler)
        {
            this.logger = logger;
            this.viewModelErrorHandler = viewModelErrorHandler;
        }

        protected override Task InitializeAsync()
        {
            return this.LoadData();
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
                this.Headers = new Dictionary<string, string> { { "Accept-Language", "fr" } };
                this.Url = "https://www.google.com";
                this.MixedContentSource = CreateMixedContentSource();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to initialize viewmodel");
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        public string? Url
        {
            get => this.url;
            set => this.SetProperty(ref this.url, value);
        }

        public IDictionary<string, string> Headers
        {
            get => this.headers;
            set => this.SetProperty(ref this.headers, value);
        }

        public bool AllowMixedContent
        {
            get => this.allowMixedContent;
            set
            {
                if (this.SetProperty(ref this.allowMixedContent, value))
                {
                    // Reloads the page, so that its image request runs under the new setting.
                    this.MixedContentSource = CreateMixedContentSource();
                }
            }
        }

        public HtmlWebViewSource? MixedContentSource
        {
            get => this.mixedContentSource;
            private set => this.SetProperty(ref this.mixedContentSource, value);
        }

        /// <summary>
        /// A page with an https address which loads an image over http. Android blocks the image as mixed content
        /// unless <see cref="AllowMixedContent" /> is on; iOS loads it either way, since it has no such setting.
        /// </summary>
        private static HtmlWebViewSource CreateMixedContentSource()
        {
            return new HtmlWebViewSource
            {
                BaseUrl = "https://www.google.com/",
                Html = """
                    <html>
                    <body style="font-family: sans-serif">
                    <p id="result">http image: loading</p>
                    <img src="http://www.google.com/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png" style="display: none"
                         onload="document.getElementById('result').textContent = 'http image: loaded'"
                         onerror="document.getElementById('result').textContent = 'http image: not loaded'">
                    </body>
                    </html>
                    """
            };
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