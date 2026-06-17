using System.Globalization;
using Superdev.Maui.Internals;

namespace Superdev.Maui.Localization
{
    /// <summary>
    /// Default translation provider does not resolve any translation keys.
    /// </summary>
    [Preserve(AllMembers = true)]
    internal class DefaultTranslationProvider : ITranslationProvider
    {
        internal DefaultTranslationProvider()
        {
        }

        /// <inheritdoc/>
        public string Translate(string key, CultureInfo? cultureInfo = null)
        {
            return $"#{key}#";
        }
    }
}