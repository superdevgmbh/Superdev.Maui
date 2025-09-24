namespace Superdev.Maui.Resources.Styles
{
    public interface IThemeHelper
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IThemeHelper"/>.
        /// </summary>
        public static IThemeHelper Current => ThemeHelper.Current;

        public bool OverrideStyles { get; set; }

        /// <summary>
        /// Overrides existing styles in the application's resource dictionary with styles
        /// whose keys end with <c>_Override</c>.
        /// </summary>
        void OverrideStylesInternal();

        void ApplyTheme(string theme);

        void ApplyTheme(string lightTheme, string darkTheme);

        /// <summary>
        /// Event is fired when the theme is changed.
        /// </summary>
        event EventHandler<AppTheme> ThemeChanged;

        /// <summary>
        /// Gets the current app theme.
        /// </summary>
        AppTheme UserAppTheme { get; }

        /// <summary>
        /// Gets the current system theme.
        /// </summary>
        AppTheme PlatformAppTheme { get; }

        /// <summary>
        /// Gets or sets whether the app should follow the system theme.
        /// </summary>
        bool UseSystemTheme { get; set; }

        /// <summary>
        /// Gets or sets the user's preferred theme when not following system theme.
        /// </summary>
        AppTheme AppTheme { get; set; }

        /// <summary>
        /// Toggle between light and dark themes.
        /// </summary>
        void ToggleTheme();

        void Reset();

#if ANDROID
        void OnResume();
#endif
    }
}