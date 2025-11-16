using System.Globalization;

namespace Superdev.Maui.Localization
{
    public interface ITranslationProvider
    {
        /// <summary>
        /// Translates the specified <paramref name="key"/> to a localized string resource.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns>Localized string.</returns>
        string Translate(string key, CultureInfo cultureInfo = null);
    }
}