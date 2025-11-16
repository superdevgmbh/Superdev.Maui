using System.Globalization;

namespace Superdev.Maui.Tests.Localization
{
    public static class SupportedLanguages
    {
        public static CultureInfo English { get; } = new CultureInfo("en");

        public static CultureInfo German { get; } = new CultureInfo("de");

        public static CultureInfo French { get; } = new CultureInfo("fr");

        public static CultureInfo Italian { get; } = new CultureInfo("it");
    }
}