using Superdev.Maui.Services;

namespace Superdev.Maui.Platforms.Services
{
    public class FontConverter : FontConverterBase
    {
        private static readonly Lazy<IFontConverter> Implementation = new Lazy<IFontConverter>(CreateInstance, LazyThreadSafetyMode.PublicationOnly);

        public static IFontConverter Current => Implementation.Value;

        private static IFontConverter CreateInstance()
        {
            return new FontConverter();
        }

        private FontConverter()
        {
        }
    }
}