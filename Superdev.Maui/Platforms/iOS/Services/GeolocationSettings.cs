using Foundation;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Services;
using UIKit;

namespace Superdev.Maui.Platforms.Services
{
    public class GeolocationSettings : IGeolocationSettings
    {
        private readonly ILogger logger;

        public GeolocationSettings(
            ILogger<GeolocationSettings> logger)
        {
            this.logger = logger;
        }

        public void ShowSettingsUI()
        {
            try
            {
                var uri = UIApplication.OpenSettingsUrlString;
                var canOpenUrl = TryOpenUrl(uri);
                if (!canOpenUrl)
                {
                    this.logger.LogDebug($"ShowSettingsUI failed to open URL: {uri}");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "ShowSettingsUI failed with exception");
            }
        }

        private static bool TryOpenUrl(string uri)
        {
            try
            {
                var nsUrl = new NSUrl(uri);
                var canOpenUrl = UIApplication.SharedApplication.CanOpenUrl(nsUrl);
                if (canOpenUrl)
                {
                    return UIApplication.SharedApplication.OpenUrl(nsUrl);
                }
            }
            catch (Exception e)
            {
                // Ignored
            }

            return false;
        }
    }
}