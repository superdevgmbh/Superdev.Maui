using Superdev.Maui.Localization;
using Superdev.Maui.Navigation;

namespace Superdev.Maui
{
    public class SuperdevMauiOptions
    {
        /// <summary>
        /// Configures the navigation behavior.
        /// </summary>
        public NavigationOptions Navigation { get; } = new NavigationOptions();

        /// <summary>
        /// Sets <see cref="Layout.IgnoreSafeArea" /> to <c>true</c> for all layouts, so content can extend under the
        /// system bars. If you ignore safe area for all layouts, it's your responsibility to add safe area padding
        /// where necessary. Background: https://github.com/dotnet/maui/issues/12417.
        /// Default: <c>false</c>
        /// </summary>
        public bool IgnoreSafeArea { get; set; } = false;

        /// <summary>
        /// When enabled, automatically clears Behaviors, Triggers, and Effects
        /// from all UI elements once a page is no longer in use.
        /// </summary>
        public bool AutoCleanupPage { get; set; } = true;

        /// <summary>
        /// Defines the translation provider used to resolve translation keys to localized strings.
        /// </summary>
        public ITranslationProvider TranslationProvider { get; set; } = new DefaultTranslationProvider();
    }
}