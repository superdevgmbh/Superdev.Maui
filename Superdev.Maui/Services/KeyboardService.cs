using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Application = Microsoft.Maui.Controls.Application;

namespace Superdev.Maui.Services
{
    public class KeyboardService : IKeyboardService
    {
        private readonly ILogger logger;
        private readonly IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.Android, Application> platformElementConfiguration;
        private readonly object lockObj = new object();
        private readonly HashSet<string> windowSoftInputModeAdjusts = new HashSet<string>();
        private readonly WindowSoftInputModeAdjust? originalWindowSoftInputModeAdjust;

        public KeyboardService(ILogger<KeyboardService> logger)
            : this(logger, Application.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>())
        {
        }

        internal KeyboardService(
            ILogger<KeyboardService> logger,
            IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.Android, Application> platformElementConfiguration)
        {
            this.logger = logger;
            this.platformElementConfiguration = platformElementConfiguration;

            this.originalWindowSoftInputModeAdjust = platformElementConfiguration?.GetWindowSoftInputModeAdjust();
        }

        public void UseWindowSoftInputModeAdjust(object target, WindowSoftInputModeAdjust windowSoftInputModeAdjust)
        {
            if (this.originalWindowSoftInputModeAdjust == null)
            {
                return;
            }

            var targetName = GetKey(target);

            lock (this.lockObj)
            {
                this.windowSoftInputModeAdjusts.Add(targetName);

                this.logger.LogDebug($"UseWindowSoftInputModeAdjust: target={targetName}, windowSoftInputModeAdjust={windowSoftInputModeAdjust}");
                this.platformElementConfiguration.UseWindowSoftInputModeAdjust(windowSoftInputModeAdjust);
            }
        }

        public void ResetWindowSoftInputModeAdjust(object target)
        {
            if (this.originalWindowSoftInputModeAdjust is not WindowSoftInputModeAdjust originalWindowSoftInputModeAdjust)
            {
                return;
            }

            var targetName = GetKey(target);

            lock (this.lockObj)
            {
                if (this.windowSoftInputModeAdjusts.Remove(targetName))
                {
                    // Check if the creation date of the entry is older than anything else in the adjustments list:
                    if (this.windowSoftInputModeAdjusts.Count != 0)
                    {
                        this.logger.LogDebug($"ResetWindowSoftInputModeAdjust: target={targetName} --> Reset not required");
                    }
                    else
                    {
                        this.logger.LogDebug($"ResetWindowSoftInputModeAdjust: target={targetName}");
                        this.platformElementConfiguration.UseWindowSoftInputModeAdjust(originalWindowSoftInputModeAdjust);
                    }
                }
                else
                {
                    this.logger.LogDebug($"ResetWindowSoftInputModeAdjust: target={targetName} --> Target not found");
                }
            }
        }

        private static string GetKey(object target)
        {
            return target.GetType().Name;
        }
    }
}