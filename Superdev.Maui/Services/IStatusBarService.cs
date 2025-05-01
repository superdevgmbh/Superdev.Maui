namespace Superdev.Maui.Services
{
    public interface IStatusBarService
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IStatusBarService"/>.
        /// </summary>
        public static IStatusBarService Current =>
#if ANDROID || IOS
            Superdev.Maui.Platforms.Services.StatusBarService.Current;
#else
            throw new NotSupportedException($"Current platform {DeviceInfo.Platform} is not supported.");
#endif

        void SetHexColor(string hexColor);

        void SetColor(Color color);

        void SetStatusBarMode(StatusBarStyle statusBarMode);
    }
}