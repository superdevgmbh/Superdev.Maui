using Microsoft.Extensions.Logging;

namespace Superdev.Maui.Services
{
    public class Browser : IBrowser
    {
        private static readonly Lazy<IBrowser> Implementation = new Lazy<IBrowser>(CreateBrowser, LazyThreadSafetyMode.PublicationOnly);

        public static IBrowser Current => Implementation.Value;

        private static IBrowser CreateBrowser()
        {
            var serviceProvider = IPlatformApplication.Current.Services;
            var logger = serviceProvider.GetRequiredService<ILogger<Browser>>();
            return new Browser(
                logger,
                Application.Current,
                Microsoft.Maui.ApplicationModel.Browser.Default);
        }

        private readonly ILogger logger;
        private readonly Application application;
        private readonly Microsoft.Maui.ApplicationModel.IBrowser browser;

        internal Browser(
            ILogger<Browser> logger,
            Application application,
            Microsoft.Maui.ApplicationModel.IBrowser browser)
        {
            this.logger = logger;
            this.application = application;
            this.browser = browser;
        }

        public async Task<bool> TryOpenAsync(string uri)
        {
            try
            {
                return await this.TryOpenAsync(new Uri(uri));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"TryOpenAsync failed with exception: uri={uri}");
                return false;
            }
        }

        public async Task<bool> TryOpenAsync(Uri uri)
        {
            try
            {

                var options = GetDefaultBrowserLaunchOptions();
                return await this.TryOpenAsync(uri, options);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"TryOpenAsync failed with exception: uri={uri}");
                return false;
            }
        }

        public async Task<bool> TryOpenAsync(string uri, BrowserLaunchOptions options)
        {
            try
            {
                return await this.TryOpenAsync(new Uri(uri), options);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"TryOpenAsync failed with exception: uri={uri}");
                return false;
            }
        }

        public async Task<bool> TryOpenAsync(Uri uri, BrowserLaunchOptions options)
        {
            try
            {
                options.PreferredToolbarColor ??= this.GetPreferredToolbarColor();
                options.PreferredControlColor ??= this.GetPreferredControlColor();

                var success = await this.browser.OpenAsync(uri, options);
                if (!success)
                {
                    this.logger.LogError($"TryOpenAsync returned success=false: uri={uri}");
                }

                return success;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"TryOpenAsync failed with exception: uri={uri}");
                return false;
            }
        }

        private Color GetPreferredToolbarColor()
        {
            return this.application.Resources["Theme.Color.Primary"] as Color;
        }

        private Color GetPreferredControlColor()
        {
            return this.application.Resources["Theme.Color.OnPrimary"] as Color;
        }

        private static BrowserLaunchOptions GetDefaultBrowserLaunchOptions()
        {

            var options = new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred,
                TitleMode = BrowserTitleMode.Show,
                Flags = BrowserLaunchFlags.None
            };
            return options;
        }
    }
}