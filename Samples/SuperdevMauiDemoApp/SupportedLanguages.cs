using System.Globalization;

namespace SuperdevMauiDemoApp
{
    public static class SupportedLanguages
    {
        public static CultureInfo English { get; } = new CultureInfo("en");

        public static CultureInfo German { get; } = new CultureInfo("de");

        public static IEnumerable<CultureInfo> GetAll()
        {
            yield return English;
            yield return German;
        }
    }
}