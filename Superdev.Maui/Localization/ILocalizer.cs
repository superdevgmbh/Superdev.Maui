using System.Globalization;

namespace Superdev.Maui.Localization
{
    /// <summary>
    /// Implementations of this interface MUST convert iOS and Android
    /// platform-specific locales to a value supported in .NET because
    /// ONLY valid .NET cultures can have their RESX resources loaded and used.
    /// </summary>
    public interface ILocalizer
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="ILocalizer"/>.
        /// </summary>
        public static ILocalizer Current => Localizer.Current;

        /// <summary>
        /// Gets or sets the preferences key used to persist the selected language.
        /// </summary>
        string PreferencesKey { get; set; }

        /// <summary>
        /// Gets or sets the default language.
        /// </summary>
        CultureInfo DefaultLanguage { get; set; }

        /// <summary>
        /// Specifies the list of supported languages.
        /// If the list is empty, there are no restrictions regarding language selection.
        /// </summary>
        CultureInfo[] SupportedLanguages { get; set; }

        /// <summary>
        /// Gets or sets the current language.
        /// </summary>
        CultureInfo CurrentCulture { get; set; }


        /// <summary>
        /// Gets the platform-specific locale as string.
        /// </summary>
        string GetPlatformLocale();

        /// <summary>
        /// Gets the platform-specific locale as CultureInfo.
        /// </summary>
        CultureInfo GetPlatformCulture();

        /// <summary>
        /// Event is raised when the current language is going to be changed.
        /// </summary>
        event EventHandler<LanguageChangingEventArgs> LanguageChanging;

        /// <summary>
        /// Event is raised when the current language has changed.
        /// </summary>
        event EventHandler<LanguageChangedEventArgs> LanguageChanged;

        void Reset();
    }
}