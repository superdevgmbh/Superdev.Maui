using System.Globalization;
using System.Resources;
using Superdev.Maui.Internals;

namespace Superdev.Maui.Localization
{
    /// <summary>
    /// Resolves translation keys from a single <see cref="ResourceManager"/>.
    /// <remarks>
    /// Use <see cref="CompositeResxTranslationProvider"/> if you want to resolve translation keys from multiple <see cref="ResourceManager"/>s.
    /// </remarks>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class ResxTranslationProvider : ITranslationProvider
    {
        private readonly ResourceManager resourceManager;

        public ResxTranslationProvider(ResourceManager resourceManager)
        {
            this.resourceManager = resourceManager;
        }

        /// <inheritdoc/>
        public string Translate(string key, CultureInfo? cultureInfo = null)
        {
            var translatedValue = this.resourceManager.GetString(key, cultureInfo);
            if (translatedValue != null)
            {
                return translatedValue;
            }

            return $"#{key}#";
        }
    }
}