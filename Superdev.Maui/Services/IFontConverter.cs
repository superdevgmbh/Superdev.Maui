namespace Superdev.Maui.Services
{
    public interface IFontConverter : IDisposable
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IFontConverter"/>.
        /// </summary>
        public static IFontConverter Current =>
#if ANDROID || IOS
            Superdev.Maui.Platforms.Services.FontConverter.Current;
#else
            throw new NotSupportedException($"Current platform {DeviceInfo.Current.Platform} is not supported.");
#endif

        event EventHandler FontScalingChanged;

        double GetScaledFontSize(double fontSize);
    }
}