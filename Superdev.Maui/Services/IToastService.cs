namespace Superdev.Maui.Services
{
    public interface IToastService
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IToastService"/>.
        /// </summary>
        public static IToastService Current =>
#if ANDROID || IOS
            Superdev.Maui.Platforms.Services.ToastService.Current;
#else
            throw new NotSupportedException($"Current platform {DeviceInfo.Current.Platform} is not supported.");
#endif

        void LongAlert(string message);

        void ShortAlert(string message);
    }
}