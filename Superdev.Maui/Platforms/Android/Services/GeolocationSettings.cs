using Android.Content;
using Android.Provider;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Services;

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
                var intent = new Intent(Settings.ActionLocationSourceSettings);
                intent.AddCategory(Intent.CategoryDefault);
                intent.SetFlags(ActivityFlags.NewTask);

                var currentActivity = Platform.CurrentActivity;
                currentActivity.StartActivityForResult(intent, 0);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "ShowSettingsUI failed with exception");
            }
        }
    }
}