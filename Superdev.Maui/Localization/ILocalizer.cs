using System.Globalization;

namespace Superdev.Maui.Localization
{
    /// <summary>
    ///     Implementations of this interface MUST convert iOS and Android
    ///     platform-specific locales to a value supported in .NET because
    ///     ONLY valid .NET cultures can have their RESX resources loaded and used.
    /// </summary>
    public interface ILocalizer
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="ILocalizer"/>.
        /// </summary>
        public static ILocalizer Current => Localizer.Current;

        /// <summary>
        ///     Returns platform-specific locale settings.
        /// </summary>
        CultureInfo GetCurrentCulture();

        /// <summary>
        ///     Sets all relevant culture settings to <paramref name="cultureInfo" />.
        /// </summary>
        /// <remarks>
        ///     This method must be run from the UI thread.
        /// </remarks>
        void SetCultureInfo(CultureInfo cultureInfo);

        /// <summary>
        ///     Event is raised when the current culture info has changed.
        /// </summary>
        event EventHandler<LanguageChangedEventArgs> LanguageChanged;
    }
}