using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Superdev.Maui.Services
{
    public interface IKeyboardService
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IKeyboardService"/>.
        /// </summary>
        public static IKeyboardService Current =>
#if ANDROID || IOS
            Superdev.Maui.Services.KeyboardService.Current;
#else
            throw new NotSupportedException($"Current platform {DeviceInfo.Current.Platform} is not supported.");
#endif

        void UseWindowSoftInputModeAdjust(object target, WindowSoftInputModeAdjust windowSoftInputModeAdjust);

        void ResetWindowSoftInputModeAdjust(object target);
    }
}