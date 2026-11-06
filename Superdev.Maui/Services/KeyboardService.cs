using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Application = Microsoft.Maui.Controls.Application;

namespace Superdev.Maui.Services
{
    public class KeyboardService : IKeyboardService
    {
        private static readonly Lazy<IKeyboardService> Implementation = new Lazy<IKeyboardService>(
            CreateKeyboardService, LazyThreadSafetyMode.PublicationOnly);

        public static IKeyboardService Current => Implementation.Value;

        private static IKeyboardService CreateKeyboardService()
        {
            var logger = IPlatformApplication.Current!.Services.GetRequiredService<ILogger<KeyboardService>>();
            var platformElementConfiguration = Application.Current!.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>();
            return new KeyboardService(logger, platformElementConfiguration);
        }

        private readonly ILogger logger;
        private readonly IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.Android, Application> platformElementConfiguration;
        private readonly object lockObj = new object();

        /// <summary>
        /// The targets which currently need the adjusted input mode, by instance: two pages of the same type
        /// are two targets, and the original mode is only restored once the last of them resets.
        /// </summary>
        private readonly HashSet<object> targets = new HashSet<object>(ReferenceEqualityComparer.Instance);
        private readonly WindowSoftInputModeAdjust? originalWindowSoftInputModeAdjust;

        internal KeyboardService(
            ILogger<KeyboardService> logger,
            IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.Android, Application> platformElementConfiguration)
        {
            this.logger = logger;
            this.platformElementConfiguration = platformElementConfiguration;
            this.originalWindowSoftInputModeAdjust = platformElementConfiguration.GetWindowSoftInputModeAdjust();
        }

        public void UseWindowSoftInputModeAdjust(object target, WindowSoftInputModeAdjust windowSoftInputModeAdjust)
        {
            ArgumentNullException.ThrowIfNull(target);

            if (this.originalWindowSoftInputModeAdjust == null)
            {
                return;
            }

            lock (this.lockObj)
            {
                this.targets.Add(target);

                this.logger.LogDebug($"UseWindowSoftInputModeAdjust: target={GetTargetName(target)}, windowSoftInputModeAdjust={windowSoftInputModeAdjust}");
                this.platformElementConfiguration.UseWindowSoftInputModeAdjust(windowSoftInputModeAdjust);
            }
        }

        public void ResetWindowSoftInputModeAdjust(object target)
        {
            ArgumentNullException.ThrowIfNull(target);

            if (this.originalWindowSoftInputModeAdjust is not WindowSoftInputModeAdjust originalWindowSoftInputModeAdjust)
            {
                return;
            }

            lock (this.lockObj)
            {
                if (this.targets.Remove(target))
                {
                    if (this.targets.Count != 0)
                    {
                        this.logger.LogDebug($"ResetWindowSoftInputModeAdjust: target={GetTargetName(target)} --> Reset not required, {this.targets.Count} target(s) still adjusted");
                    }
                    else
                    {
                        this.logger.LogDebug($"ResetWindowSoftInputModeAdjust: target={GetTargetName(target)}");
                        this.platformElementConfiguration.UseWindowSoftInputModeAdjust(originalWindowSoftInputModeAdjust);
                    }
                }
                else
                {
                    this.logger.LogDebug($"ResetWindowSoftInputModeAdjust: target={GetTargetName(target)} --> Target not found");
                }
            }
        }

        private static string GetTargetName(object target)
        {
            return target.GetType().Name;
        }
    }
}