using Superdev.Maui.Controls;

namespace Superdev.Maui.Services
{
    public interface IActivityIndicatorService
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IActivityIndicatorService"/>.
        /// </summary>
        public static IActivityIndicatorService Current =>
#if ANDROID || IOS
            Superdev.Maui.Platforms.Services.ActivityIndicatorService.Current;
#else
            throw new NotSupportedException($"Current platform {DeviceInfo.Platform} is not supported.");
#endif

        void Init<T>(T activityIndicatorPage) where T : ContentPage, IActivityIndicatorPage;

        void ShowLoadingPage(string text);

        void HideLoadingPage();
    }
}