namespace Superdev.Maui.Services
{
    /// <summary>
    /// <seealso cref="IFontConverter"/> which does not change the scale of font sizes.
    /// </summary>
    public class NullFontConverter : IFontConverter
    {
        private static readonly Lazy<IFontConverter> Implementation = new Lazy<IFontConverter>(CreateLocalizer, LazyThreadSafetyMode.PublicationOnly);

        public static IFontConverter Current => Implementation.Value;

        private static IFontConverter CreateLocalizer()
        {
            return new NullFontConverter();
        }

        private NullFontConverter()
        {
        }

        public event EventHandler FontScalingChanged;

        public double GetScaledFontSize(double fontSize)
        {
            return fontSize;
        }

        public void Dispose()
        {
        }
    }
}