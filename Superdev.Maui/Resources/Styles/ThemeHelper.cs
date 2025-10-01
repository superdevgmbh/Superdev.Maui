using Microsoft.Extensions.Logging;
using Superdev.Maui.Services;
using Preferences = Superdev.Maui.Services.Preferences;
using Superdev.Maui.Extensions;
using Superdev.Maui.Utils;

namespace Superdev.Maui.Resources.Styles
{
    /// <summary>
    ///     Helper class for managing application themes with platform-aware adaptations
    /// </summary>
    public partial class ThemeHelper : IThemeHelper
    {
        private const string OverrideStylesSuffix = "_Override";
        private const bool DefaultUseSystemTheme = true;
        private const string AppThemeSettingsKey = "AppThemeSettingsKey";
        private const string UseSystemThemeSettingsKey = "UseSystemThemeSettingsKey";

        private static readonly Lazy<IThemeHelper> Implementation = new Lazy<IThemeHelper>(CreateThemeHelper, LazyThreadSafetyMode.PublicationOnly);

        private static IThemeHelper CreateThemeHelper()
        {
            var logger = IPlatformApplication.Current.Services.GetService<ILogger<ThemeHelper>>();
            var preferences = Preferences.Current;
            var fontConverter = IFontConverter.Current;
            return new ThemeHelper(logger, preferences, fontConverter);
        }

        public static IThemeHelper Current => Implementation.Value;

        private readonly ILogger logger;
        private readonly IPreferences preferences;
        private readonly IFontConverter fontConverter;

        private readonly object eventLock = new object();

        private event EventHandler<AppTheme> ThemeChangedEventHandler;
        private EventHandler<AppThemeChangedEventArgs> systemThemeHandler;
        private string lightThemeName;
        private string darkThemeName;
        private bool? useSystemTheme;
        private AppTheme? appTheme;
        private AppTheme? lastUsedTheme;
        private bool overrideStyles;
        private bool isInitialized;

        private ThemeHelper(
            ILogger<ThemeHelper> logger,
            IPreferences preferences,
            IFontConverter fontConverter)
        {
            this.logger = logger;
            this.preferences = preferences;
            this.fontConverter = fontConverter;

            this.Initialize();
        }

        public bool OverrideStyles
        {
            get => this.overrideStyles;
            set
            {
                if (this.overrideStyles != value)
                {
                    this.overrideStyles = value;
                    if (value)
                    {
                        this.OverrideStylesInternal();
                    }
                }
            }
        }

        public void OverrideStylesInternal()
        {
            var app = Application.Current;
            var mergedResources = ReflectionHelper.GetPropertyValue<IEnumerable<KeyValuePair<string, object>>>(app.Resources, "MergedResources")
#if DEBUG
                    .ToArray()
#endif
                ;
#if DEBUG
            this.logger.LogDebug(
                $"OverrideStyles: Found resources: {mergedResources.Length}");

            var allStyles = mergedResources
                .Where(r => r.Value is Style)
                .OrderBy(r => r.Key)
                .ToArray();
            this.logger.LogDebug(
                $"OverrideStyles: Found styles: {allStyles.Length}{Environment.NewLine}" +
                $"{string.Join(Environment.NewLine, allStyles.Select(r => $"Key: {r.Key}, TargetType: {((Style)r.Value).TargetType}"))}");
#endif

            var styleOverrides = mergedResources.Where(r => (r.Key?.EndsWith(OverrideStylesSuffix, StringComparison.InvariantCultureIgnoreCase) ?? false) && r.Value is Style)
                .Select(r => (r.Key, BaseKey: r.Key.Replace(OverrideStylesSuffix, string.Empty), Style: r.Value as Style));

            foreach (var styleOverride in styleOverrides)
            {
                this.logger.LogDebug($"OverrideStyles: Overriding style x:Key={styleOverride.BaseKey} with x:Key={styleOverride.Key}");
                app.Resources[styleOverride.BaseKey] = styleOverride.Style;
            }
        }

        /// <summary>
        ///     Configures the current app's resources by merging pre-defined CrossPlatformLibrary resources and creating new
        ///     resources based on the <see cref="ITheme" />'s properties.
        /// </summary>
        /// <param name="themeName">
        ///     The key of the <see cref="ITheme" /> object in the current app's resource
        ///     dictionary.
        /// </param>
        /// <exception cref="ArgumentNullException" />
        public void ApplyTheme(string themeName)
        {
            ArgumentNullException.ThrowIfNull(themeName);

            this.ApplyTheme(themeName, themeName);
        }

        public void ApplyTheme(string lightTheme, string darkTheme)
        {
            ArgumentNullException.ThrowIfNull(lightTheme);
            ArgumentNullException.ThrowIfNull(darkTheme);

            this.lightThemeName = lightTheme;
            this.darkThemeName = darkTheme;
            this.ApplyTheme();
        }

        /// <inheritdoc />
        public event EventHandler<AppTheme> ThemeChanged
        {
            add
            {
                lock (this.eventLock)
                {
                    this.ThemeChangedEventHandler += value;
                }
            }
            remove
            {
                lock (this.eventLock)
                {
                    this.ThemeChangedEventHandler -= value;
                }
            }
        }

        /// <inheritdoc />
        public AppTheme PlatformAppTheme => Application.Current.PlatformAppTheme;

        /// <inheritdoc />
        public AppTheme UserAppTheme
        {
            get => Application.Current.UserAppTheme;
            private set => Application.Current.UserAppTheme = value;
        }

        /// <inheritdoc />
        public bool UseSystemTheme
        {
            get => this.useSystemTheme ?? (this.useSystemTheme = this.preferences.Get(UseSystemThemeSettingsKey, DefaultUseSystemTheme)) ?? DefaultUseSystemTheme;
            set
            {
                if (this.useSystemTheme != value)
                {
                    this.useSystemTheme = value;
                    this.preferences.Set(UseSystemThemeSettingsKey, value);
                    this.ApplyTheme();
                }
            }
        }

        public AppTheme AppTheme
        {
            get => this.appTheme ?? (this.appTheme = this.preferences.Get(AppThemeSettingsKey, this.UserAppTheme)) ?? this.UserAppTheme;
            set
            {
                if (this.appTheme != value)
                {
                    this.UseSystemTheme = false;

                    this.appTheme = value;
                    this.preferences.Set(AppThemeSettingsKey, value);

                    if (!this.UseSystemTheme) // TODO: This is always false (because we set it to false!)
                    {
                        this.ApplyTheme();
                    }
                }
            }
        }

        private void Initialize()
        {
            try
            {
                if (Application.Current == null)
                {
                    this.logger.LogWarning("Application.Current is null, cannot initialize theme helper");
                    return;
                }

                // Initialize theme resources
                // this.InitializeThemeResources();

                // Unsubscribe existing handler if any
                if (this.systemThemeHandler != null)
                {
                    Application.Current.RequestedThemeChanged -= this.systemThemeHandler;
                }

                // Register for system theme changes
                this.systemThemeHandler = (s, e) =>
                {
                    // Store the last detected system theme
                    this.lastUsedTheme = e.RequestedTheme;

                    if (this.UseSystemTheme)
                    {
                        // If following system theme, update when system changes
                        this.ApplyTheme();
                    }
                };

                // Register for system theme changes // TODO: Unregister in Dispose
                Application.Current.RequestedThemeChanged += this.systemThemeHandler;

                // Store initial system theme
                this.lastUsedTheme = this.GetCurrentTheme();

                // Apply the initial theme
                this.ApplyTheme();

                this.isInitialized = true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Initialize failed with exception");
            }
        }

        /// <summary>
        ///     Initialize theme resources for light and dark themes
        /// </summary>
        // private void InitializeThemeResources()
        // {
        //     try
        //     {
        //         // Clear existing resources
        //         this.lightThemeResources.Clear();
        //         this.darkThemeResources.Clear();
        //         this.platformLightThemeResources.Clear();
        //         this.platformDarkThemeResources.Clear();
        //
        //         // Load resources from the existing theme dictionaries
        //         this.LoadResourcesFromExistingThemes();
        //     }
        //     catch (Exception ex)
        //     {
        //         this.logger.LogError(ex, "InitializeThemeResources failed with exception");
        //     }
        // }

        /// <summary>
        ///     Load resources from the existing theme dictionaries in the app
        /// </summary>
        // private void LoadResourcesFromExistingThemes()
        // {
        //     try
        //     {
        //         if (Application.Current?.Resources?.MergedDictionaries is not { } mergedDictionaries)
        //         {
        //             return;
        //         }
        //
        //         // Find the theme dictionaries
        //         var colorResources = mergedDictionaries.FirstOrDefault(d => d.Source?.OriginalString?.Contains("ColorResources.xaml") == true);
        //         if (colorResources != null)
        //         {
        //             mergedDictionaries.Remove(colorResources);
        //         }
        //
        //         var lightDict = mergedDictionaries.FirstOrDefault(d => d.Source?.OriginalString?.Contains("Colors.Light.xaml") == true);
        //
        //         var darkDict = mergedDictionaries.FirstOrDefault(d => d.Source?.OriginalString?.Contains("Colors.Dark.xaml") == true);
        //         var platformDict = mergedDictionaries.FirstOrDefault(d => d.Source?.OriginalString?.Contains("PlatformColors.xaml") == true);
        //         var stylesDict = mergedDictionaries.FirstOrDefault(d => d.Source?.OriginalString?.Contains("Styles.xaml") == true);
        //
        //         // Load light theme resources
        //         if (lightDict != null)
        //         {
        //             foreach (var key in lightDict.Keys)
        //             {
        //                 if (key is string stringKey && lightDict[key] != null)
        //                 {
        //                     this.lightThemeResources[stringKey] = lightDict[key];
        //                 }
        //             }
        //         }
        //         // else
        //         // {
        //         //     // Fallback light theme resources if dictionary not found
        //         //     this.lightThemeResources["BackgroundColor"] = MaterialColors.White;
        //         //     this.lightThemeResources["ForegroundColor"] = MaterialColors.Black;
        //         //     this.lightThemeResources["PrimaryColor"] = Color.FromArgb("#FF0078D7");
        //         //     this.lightThemeResources["SecondaryColor"] = Color.FromArgb("#FF6C757D");
        //         //     this.lightThemeResources["AccentColor"] = Color.FromArgb("#FF0078D7");
        //         //     this.lightThemeResources["SurfaceColor"] = Color.FromArgb("#FFF8F9FA");
        //         //     this.lightThemeResources["CardColor"] = MaterialColors.White;
        //         //     this.lightThemeResources["BorderColor"] = Color.FromArgb("#FFCED4DA");
        //         //     this.lightThemeResources["TextColor"] = MaterialColors.Black;
        //         //     this.lightThemeResources["TextSecondaryColor"] = Color.FromArgb("#FF6C757D");
        //         //     this.lightThemeResources["TextTertiaryColor"] = Color.FromArgb("#FF8A8A8A");
        //         //     this.lightThemeResources["DividerColor"] = Color.FromArgb("#FFE0E0E0");
        //         // }
        //
        //         // Load dark theme resources
        //         if (darkDict != null)
        //         {
        //             foreach (var key in darkDict.Keys)
        //             {
        //                 if (key is string stringKey && darkDict[key] != null)
        //                 {
        //                     this.darkThemeResources[stringKey] = darkDict[key];
        //                 }
        //             }
        //         }
        //         // else
        //         // {
        //         //     // Fallback dark theme resources if dictionary not found
        //         //     this.darkThemeResources["BackgroundColor"] = Color.FromArgb("#FF121212");
        //         //     this.darkThemeResources["ForegroundColor"] = MaterialColors.White;
        //         //     this.darkThemeResources["PrimaryColor"] = Color.FromArgb("#FF0078D7");
        //         //     this.darkThemeResources["SecondaryColor"] = Color.FromArgb("#FF6C757D");
        //         //     this.darkThemeResources["AccentColor"] = Color.FromArgb("#FF0078D7");
        //         //     this.darkThemeResources["SurfaceColor"] = Color.FromArgb("#FF1E1E1E");
        //         //     this.darkThemeResources["CardColor"] = Color.FromArgb("#FF2D2D2D");
        //         //     this.darkThemeResources["BorderColor"] = Color.FromArgb("#FF444444");
        //         //     this.darkThemeResources["TextColor"] = MaterialColors.White;
        //         //     this.darkThemeResources["TextSecondaryColor"] = Color.FromArgb("#FFB0B0B0");
        //         //     this.darkThemeResources["TextTertiaryColor"] = Color.FromArgb("#FF8A8A8A");
        //         //     this.darkThemeResources["DividerColor"] = Color.FromArgb("#FF333333");
        //         // }
        //
        //         // Load platform-specific resources
        //         if (platformDict != null)
        //         {
        //             foreach (var key in platformDict.Keys)
        //             {
        //                 if (key is string stringKey && platformDict[key] != null)
        //                 {
        //                     // AppThemeBinding is not accessible in C#; just add all resources
        //                     this.lightThemeResources[stringKey] = platformDict[key];
        //                     this.darkThemeResources[stringKey] = platformDict[key];
        //                 }
        //             }
        //         }
        //
        //         // Load common styles
        //         if (stylesDict != null)
        //         {
        //             // Store style resources that should be applied to both themes
        //             var styleKeys = new List<string>();
        //
        //             foreach (var key in stylesDict.Keys)
        //             {
        //                 if (key is string stringKey && !stringKey.Contains("Color") && stylesDict[key] != null)
        //                 {
        //                     styleKeys.Add(stringKey);
        //                 }
        //             }
        //
        //             // Apply styles to both themes
        //             foreach (var key in styleKeys)
        //             {
        //                 this.lightThemeResources[key] = stylesDict[key];
        //                 this.darkThemeResources[key] = stylesDict[key];
        //             }
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         this.logger.LogError(ex, "LoadResourcesFromExistingThemes failed with exception");
        //     }
        // }

        /// <summary>
        ///     Check for system theme changes
        /// </summary>
        public void CheckForSystemThemeChanges()
        {
            try
            {
                if (Application.Current == null)
                {
                    return;
                }

                var currentSystemTheme = this.PlatformAppTheme;

                // If system theme has changed
                if (currentSystemTheme != this.lastUsedTheme)
                {
                    this.lastUsedTheme = currentSystemTheme;

                    if (this.UseSystemTheme)
                    {
                        // Apply the new theme
                        this.ApplyTheme();
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "CheckForSystemThemeChanges failed with exception");
            }
        }

        /// <summary>
        ///     Apply the appropriate theme based on settings
        /// </summary>
        private void ApplyTheme()
        {
            try
            {
                if (Application.Current is not Application application)
                {
                    return;
                }

                // Determine which theme to use
                var currentTheme = this.GetCurrentTheme();

                // Apply the theme
                if (application.UserAppTheme != currentTheme)
                {
                    application.UserAppTheme = currentTheme;
                }

                // Apply theme resources
                this.ApplyThemeResources(currentTheme);

                // TODO: Notify listeners of theme change
                // var hasThemeChanged = this.lastUsedTheme != currentTheme;
                // if (hasThemeChanged)
                // {
                //     this.RaiseThemeChangedEvent(currentTheme);
                // }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "ApplyTheme failed with exception");
            }
        }

        private AppTheme GetCurrentTheme()
        {
            return this.UseSystemTheme
                ? this.PlatformAppTheme
                : this.AppTheme;
        }

        private void RaiseThemeChangedEvent(AppTheme appTheme)
        {
            lock (this.eventLock)
            {
                this.ThemeChangedEventHandler?.Invoke(null, appTheme);
            }
        }

        /// <summary>
        ///     Apply theme resources to the application
        /// </summary>
        private void ApplyThemeResources(AppTheme appTheme)
        {
            try
            {
                if (Application.Current?.Resources == null)
                {
                    return;
                }

                // Get the merged dictionaries
                var globalMergedDictionary = Application.Current.Resources.MergedDictionaries;
                if (globalMergedDictionary == null)
                {
                    return;
                }

                // Find the theme dictionaries
                var themeName = appTheme == AppTheme.Light ? this.lightThemeName : this.darkThemeName;
                if (themeName == null)
                {
                    return;
                }

                var theme = Application.Current.Resources.GetValue<ITheme>(themeName);
                if (theme == null)
                {
                    this.logger.LogError($"Theme '{themeName}' could not be found");
                    return;
                }

                var superdevMauiStyles = globalMergedDictionary.FirstOrDefault<SuperdevMauiStyles>();
                var localMergedDictionaries = superdevMauiStyles.MergedDictionaries;

                // Update ColorResources
                var colorResources = localMergedDictionaries.FirstOrDefault<ColorResources>();
                if (colorResources != null)
                {
                    localMergedDictionaries.Remove(colorResources);
                }

                var colorConfiguration = theme.ColorConfiguration ?? new ColorConfiguration();
                colorConfiguration.Initialize();
                localMergedDictionaries.Add(colorConfiguration.Resources);

                // Update SpacingResources
                var spacingResources = localMergedDictionaries.FirstOrDefault<SpacingResources>();
                if (spacingResources != null)
                {
                    localMergedDictionaries.Remove(spacingResources);
                }

                var spacingConfiguration = theme.SpacingConfiguration ?? new SpacingConfiguration();
                spacingConfiguration.Initialize();
                localMergedDictionaries.Add(spacingConfiguration.Resources);

                // Update FontResources
                var fontResources = localMergedDictionaries.FirstOrDefault<FontResources>();
                if (fontResources != null)
                {
                    localMergedDictionaries.Remove(fontResources);
                }

                var fontConfiguration = theme.FontConfiguration ?? new FontConfiguration(this.fontConverter);
                fontConfiguration.Initialize();
                localMergedDictionaries.Add(fontConfiguration.Resources);

                if (this.OverrideStyles)
                {
                    this.OverrideStylesInternal();
                }

#if ANDROID || IOS
                this.ApplyThemePlatformResources(theme);
#endif

                // Apply theme resources from the appropriate dictionary first
                // var sourceDict = theme == AppTheme.Dark ? darkDict : lightDict;
                // if (colorResources != null)
                // {
                //     foreach (var key in colorResources.Keys)
                //     {
                //         if (key is string stringKey)
                //         {
                //             if (stringKey == ThemeConstants.Button.TextColor)
                //             {
                //
                //             }
                //
                //             Application.Current.Resources[stringKey] = colorResources[key];
                //         }
                //     }
                // }

                // Get the appropriate theme resources
                // var themeResources = appTheme == AppTheme.Dark ? this.darkThemeResources : this.lightThemeResources;

                // Apply our enhanced theme resources (these will override any conflicts)
                // foreach (var key in themeResources.Keys)
                // {
                //     Application.Current.Resources[key] = themeResources[key];
                // }

                // Apply platform-specific theme resources if enabled
                // if (this.UsePlatformSpecificThemes)
                // {
                //     var platformResources = appTheme == AppTheme.Dark
                //         ? this.platformDarkThemeResources
                //         : this.platformLightThemeResources;
                //
                //     foreach (var key in platformResources.Keys)
                //     {
                //         Application.Current.Resources[key] = platformResources[key];
                //     }
                // }

                // Update dynamic resources that use AppThemeBinding
                // this.UpdateDynamicResources();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "ApplyThemeResources failed with exception");
            }
        }

        /// <summary>
        ///     Update dynamic resources that use AppThemeBinding
        /// </summary>
        private void UpdateDynamicResources()
        {
            try
            {
                if (Application.Current?.Resources == null)
                {
                    return;
                }

                // Force update of any dynamic resources
                var temp = new ResourceDictionary();
                var keys = Application.Current.Resources.Keys.ToList();

                foreach (var key in keys)
                {
                    if (key is string stringKey && Application.Current.Resources[key] != null)
                    {
                        // Store the resource temporarily
                        temp[stringKey] = Application.Current.Resources[key];

                        // Remove and re-add to force update of AppThemeBinding
                        Application.Current.Resources.Remove(stringKey);
                        Application.Current.Resources[stringKey] = temp[stringKey];
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "UpdateDynamicResources failed with exception");
            }
        }

        /// <inheritdoc />
        public void ToggleTheme()
        {
            // If following system, switch to manual mode
            if (this.UseSystemTheme)
            {
                this.UseSystemTheme = false;
                this.AppTheme = Application.Current?.PlatformAppTheme == AppTheme.Dark
                    ? AppTheme.Light
                    : AppTheme.Dark;
            }
            else
            {
                // Toggle between light and dark
                this.AppTheme = this.AppTheme == AppTheme.Dark
                    ? AppTheme.Light
                    : AppTheme.Dark;
            }
        }

#if ANDROID
        public void OnResume()
        {
            var currentTheme = this.GetCurrentTheme();
            if (currentTheme != this.lastUsedTheme)
            {
                this.ApplyTheme();
            }

        }
#endif

        /// <inheritdoc />
        public void Reset()
        {
            this.useSystemTheme = null;
            this.appTheme = null;

            this.preferences.Remove(AppThemeSettingsKey);
            this.preferences.Remove(UseSystemThemeSettingsKey);

            this.Initialize();
        }

        /// <summary>
        ///     Get a theme-specific resource value
        /// </summary>
        public T GetThemeResource<T>(string resourceKey, AppTheme theme)
        {
            if (Application.Current?.Resources == null)
            {
                return default!;
            }

            // Try to get the theme-specific resource
            var themeKey = $"{resourceKey}{(theme == AppTheme.Dark ? "Dark" : "Light")}";

            if (Application.Current.Resources.TryGetValue(themeKey, out var themeValue) && themeValue is T themeTypedValue)
            {
                return themeTypedValue;
            }

            // Fall back to the base resource
            if (Application.Current.Resources.TryGetValue(resourceKey, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            return default!;
        }
    }
}