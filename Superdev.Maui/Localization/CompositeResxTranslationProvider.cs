using System.Globalization;
using System.Resources;
using Superdev.Maui.Internals;

namespace Superdev.Maui.Localization
{
    /// <summary>
    /// Resolves translation keys across multiple <see cref="ResourceManager"/>s in order.
    /// </summary>
    [Preserve(AllMembers = true)]
    public sealed class CompositeResxTranslationProvider : ITranslationProvider
    {
        private readonly ResourceManager[] resourceManagers;

        public CompositeResxTranslationProvider(ResourceManager[] resourceManagers)
        {
            this.resourceManagers = resourceManagers;
        }

        /// <inheritdoc/>
        public string Translate(string key, CultureInfo? cultureInfo)
        {
            foreach (var resourceManager in this.resourceManagers)
            {
                var translatedValue = resourceManager.GetString(key, cultureInfo);
                if (translatedValue != null)
                {
                    return translatedValue;
                }
            }

            return $"#{key}#";
        }
    }
}
