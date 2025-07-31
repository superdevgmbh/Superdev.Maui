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
            throw new NotSupportedException($"Current platform {DeviceInfo.Current.Platform} is not supported.");
#endif

#if ANDROID
        void OnStart(Android.App.Activity activity);

        void OnResume();
#endif

        void SetStatusBarColor(Color color);

        void SetNavigationBarColor(Color color);

        void SetStyle(StatusBarStyle statusBarStyle);
    }
}