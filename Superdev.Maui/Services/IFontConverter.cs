namespace Superdev.Maui.Services
{
    public interface IFontConverter : IDisposable
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IFontConverter"/>.
        /// </summary>
        public static IFontConverter Current => NullFontConverter.Current;

        event EventHandler FontScalingChanged;

        double GetScaledFontSize(double fontSize);
    }
}