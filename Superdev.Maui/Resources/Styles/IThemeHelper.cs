namespace Superdev.Maui.Resources.Styles
{
    public interface IThemeHelper
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IThemeHelper"/>.
        /// </summary>
        public static IThemeHelper Current => ThemeHelper.Current;

        /// <summary>
        /// Determines whether to override existing styles with those whose <c>x:Key</c> is prefixed with <c>_Override</c>.
        /// </summary>
        public bool OverrideStyles { get; set; }

        /// <summary>
        /// Determines whether to merge existing styles with those whose <c>x:Key</c> is prefixed with <c>_Merge</c>.
        /// </summary>
        public bool MergeStyles { get; set; }

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