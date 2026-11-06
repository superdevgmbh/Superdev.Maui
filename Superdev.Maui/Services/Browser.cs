using Microsoft.Extensions.Logging;
using Superdev.Maui.Resources.Styles;

namespace Superdev.Maui.Services
{
    public class Browser : IBrowser
    {
        private static readonly Lazy<IBrowser> Implementation = new Lazy<IBrowser>(CreateBrowser, LazyThreadSafetyMode.PublicationOnly);

        public static IBrowser Current => Implementation.Value;

        private static IBrowser CreateBrowser()
        {
            var serviceProvider = IPlatformApplication.Current!.Services;
            var logger = serviceProvider.GetRequiredService<ILogger<Browser>>();
            return new Browser(
                logger,
                Application.Current!,
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

        private Color? GetPreferredToolbarColor()
        {
            return this.GetThemeColor(ThemeConstants.Color.Primary);
        }

        private Color? GetPreferredControlColor()
        {
            return this.GetThemeColor(ThemeConstants.Color.OnPrimary);
        }

        /// <summary>
        /// Reads a theme color, or returns <c>null</c> if the application does not use the library's theme.
        /// </summary>
        /// <remarks>
        /// The <see cref="ResourceDictionary" /> indexer throws when a key is absent, and these keys only
        /// exist once an application applied a <see cref="ColorConfiguration" />. Looking them up with the
        /// indexer therefore made the browser fail to open in every application which does not.
        /// </remarks>
        private Color? GetThemeColor(string resourceKey)
        {
            return this.application.Resources.TryGetValue(resourceKey, out var value) && value is Color color
                ? color
                : null;
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