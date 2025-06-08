using System.Diagnostics;
using Superdev.Maui.Resources.Styles;
using Superdev.Maui.Services;

namespace Superdev.Maui
{
    /// <summary>
    ///     Class that provides static methods and properties for configuring CrossPlatformLibrary resources.
    /// </summary>
    public class SuperdevMaui
    {
        private static IFontConverter fontConverter = new NullFontConverter();

        private readonly ITheme config;
        private readonly ResourceDictionary applicationResources;

        private SuperdevMaui(Application app, ITheme config)
        {
            this.applicationResources = app.Resources;
            this.config = config ?? GetDefaultConfiguration();
        }

        private static Theme GetDefaultConfiguration()
        {
            return new Theme
            {
                ColorConfiguration = new ColorConfiguration(),
                SpacingConfiguration = new SpacingConfiguration(),
                FontConfiguration = new FontConfiguration()
            };
        }

        /// <summary>
        /// Sets the <seealso cref="IFontConverter"/> which is used to scale font sizes.
        /// </summary>
        /// <param name="converter"></param>
        public static void SetFontConverter(IFontConverter converter)
        {
            fontConverter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        /// <summary>
        ///     Configures the current app's resources by merging pre-defined CrossPlatformLibrary resources.
        /// </summary>
        /// <param name="app">The cross-platform mobile application that is running.</param>
        /// <exception cref="ArgumentNullException" />
        public static void Init(Application app)
        {
            ArgumentNullException.ThrowIfNull(app);

            Init(app, GetDefaultConfiguration());
        }

        /// <summary>
        ///     Configures the current app's resources by merging pre-defined CrossPlatformLibrary resources and creating new
        ///     resources based on the <see cref="ITheme" />'s properties.
        /// </summary>
        /// <param name="app">The cross-platform mobile application that is running.</param>
        /// <param name="themeKey">
        ///     The key of the <see cref="ITheme" /> object in the current app's resource
        ///     dictionary.
        /// </param>
        /// <exception cref="ArgumentNullException" />
        public static void Init(Application app, string themeKey)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(themeKey);

            var stopwatch = Stopwatch.StartNew();
            var theme = app.Resources.ResolveTheme<ITheme>(themeKey);
            Init(app, theme);
            Debug.WriteLine($"Init with themeKey='{themeKey}' finished in {stopwatch.Elapsed.TotalMilliseconds:F0}ms");
        }

        /// <summary>
        /// Configures the current app's resources by merging pre-defined resources and creating new
        /// resources based on the <see cref="Theme" />'s properties.
        /// </summary>
        /// <param name="app">The current app.</param>
        /// <param name="theme">The theme configuration.</param>
        public static void Init(Application app, ITheme theme)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(theme);

            var stopwatch = Stopwatch.StartNew();
            var superdevMaui = new SuperdevMaui(app, theme);
            superdevMaui.MergeResources();
            Debug.WriteLine($"Init finished in {stopwatch.Elapsed.TotalMilliseconds:F0}ms");
        }

        private void MergeResources()
        {
            this.applicationResources.MergedDictionaries.Add(new ThemeColorResources(this.config.ColorConfiguration ?? new ColorConfiguration()));
            this.applicationResources.MergedDictionaries.Add(new ThemeSpacingResources(this.config.SpacingConfiguration ?? new SpacingConfiguration()));
            this.applicationResources.MergedDictionaries.Add(new ThemeFontResources(this.config.FontConfiguration ?? new FontConfiguration(), fontConverter));
        }
    }
}