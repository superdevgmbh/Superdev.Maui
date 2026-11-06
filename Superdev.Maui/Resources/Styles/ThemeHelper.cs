using Microsoft.Extensions.Logging;
using Superdev.Maui.Extensions;
using Superdev.Maui.Services;
using Superdev.Maui.Utils;
using Preferences = Superdev.Maui.Services.Preferences;

namespace Superdev.Maui.Resources.Styles
{
    /// <summary>
    ///     Helper class for managing application themes with platform-aware adaptations
    /// </summary>
    public partial class ThemeHelper : IThemeHelper, IDisposable
    {
        private const string OverrideStylesSuffix = "_Override";
        private const string MergeStylesSuffix = "_Merge";
        private const bool DefaultUseSystemTheme = true;
        private const string AppThemeSettingsKey = "AppThemeSettingsKey";
        private const string UseSystemThemeSettingsKey = "UseSystemThemeSettingsKey";

        private static readonly Lazy<IThemeHelper> Implementation = new Lazy<IThemeHelper>(CreateThemeHelper, LazyThreadSafetyMode.PublicationOnly);

        private static IThemeHelper CreateThemeHelper()
        {
            var logger = IPlatformApplication.Current!.Services.GetRequiredService<ILogger<ThemeHelper>>();
            var preferences = Preferences.Current;
            var fontConverter = IFontConverter.Current;
            return new ThemeHelper(logger, preferences, fontConverter);
        }

        public static IThemeHelper Current => Implementation.Value;

        private readonly ILogger logger;
        private readonly IPreferences preferences;
        private readonly IFontConverter fontConverter;

        private readonly object eventLock = new object();

        private event EventHandler<AppTheme>? ThemeChangedEventHandler;
        private EventHandler<AppThemeChangedEventArgs>? systemThemeHandler;

        /// <summary>
        /// The application instance <see cref="systemThemeHandler"/> is subscribed to, so that the subscription
        /// is released from that instance even if <see cref="Application.Current"/> was replaced since.
        /// </summary>
        private Application? subscribedApplication;
        private string? lightThemeName;
        private string? darkThemeName;
        private bool? useSystemTheme;
        private AppTheme? appTheme;
        private AppTheme? lastUsedTheme;

        /// <summary>
        /// The theme the last application of resources used, which decides whether <see cref="ThemeChanged"/> is raised.
        /// Kept apart from <see cref="lastUsedTheme"/>, which the system theme handler updates as soon as
        /// <see cref="Application.UserAppTheme"/> changes, before the resources are applied.
        /// </summary>
        private AppTheme? appliedTheme;

        /// <summary>
        /// Set while this helper changes <see cref="Application.UserAppTheme"/>. MAUI raises
        /// <see cref="Application.RequestedThemeChanged"/> from that setter, and the theme it applies next is
        /// applied by the caller, not again by the system theme handler.
        /// </summary>
        private bool isUpdatingUserAppTheme;

        /// <summary>
        /// The font configuration this helper created itself, and therefore has to dispose when it is
        /// replaced. A configuration supplied by the theme is owned by that theme.
        /// </summary>
        private FontConfiguration? ownedFontConfiguration;
        private bool overrideStyles;
        private bool mergeStyles;

        internal ThemeHelper(
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
                        // this.OverrideStylesInternal(overrideStyles: true, mergeStyles: this.MergeStyles);
                    }
                }
            }
        }

        public bool MergeStyles
        {
            get => this.mergeStyles;
            set
            {
                if (this.mergeStyles != value)
                {
                    this.mergeStyles = value;
                    if (value)
                    {
                        // this.OverrideStylesInternal(overrideStyles: this.OverrideStyles, mergeStyles: true);
                    }
                }
            }
        }

        private void OverrideStylesInternal(bool overrideStyles, bool mergeStyles)
        {
            var app = Application.Current!;
            var mergedResources = ReflectionHelper.GetPropertyValue<IEnumerable<KeyValuePair<string, object>>>(app.Resources, "MergedResources")!
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

            if (overrideStyles)
            {
                var styleOverrides = mergedResources.Where(r => (r.Key?.EndsWith(OverrideStylesSuffix, StringComparison.InvariantCultureIgnoreCase) ?? false) && r.Value is Style)
                    .Select(r => (r.Key, BaseKey: r.Key.Replace(OverrideStylesSuffix, string.Empty), Style: r.Value as Style));

                foreach (var styleOverride in styleOverrides)
                {
                    this.logger.LogDebug($"OverrideStyles: Overriding style x:Key={styleOverride.BaseKey} with x:Key={styleOverride.Key}");
                    app.Resources[styleOverride.BaseKey] = styleOverride.Style;
                }
            }

            if (mergeStyles)
            {
                var styleMerges = mergedResources.Where(r => (r.Key?.EndsWith(MergeStylesSuffix, StringComparison.InvariantCultureIgnoreCase) ?? false) && r.Value is Style)
                    .Select(r => (r.Key, BaseKey: r.Key.Replace(MergeStylesSuffix, string.Empty), Style: r.Value as Style));

                foreach (var styleMerge in styleMerges)
                {
                    this.logger.LogDebug($"OverrideStyles: Merging style x:Key={styleMerge.BaseKey} with x:Key={styleMerge.Key}");

                    var baseStyle = (Style)app.Resources[styleMerge.BaseKey];

                    if (styleMerge.Style is Style style)
                    {
                        foreach (var setter in style.Setters)
                        {
                            var existingSetter = baseStyle.Setters.FirstOrDefault(s => s.Property.PropertyName == setter.Property.PropertyName);
                            if (existingSetter != null)
                            {
                                baseStyle.Setters.Remove(existingSetter);
                            }

                            baseStyle.Setters.Add(setter);
                        }
                    }
                }
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
        public event EventHandler<AppTheme>? ThemeChanged
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

        /// <summary>
        ///     Raises <see cref="ThemeChanged" />.
        /// </summary>
        private void RaiseThemeChangedEvent(AppTheme appTheme)
        {
            EventHandler<AppTheme>? themeChangedEventHandler;

            lock (this.eventLock)
            {
                themeChangedEventHandler = this.ThemeChangedEventHandler;
            }

            try
            {
                themeChangedEventHandler?.Invoke(this, appTheme);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "A {EventName} handler failed with exception", nameof(this.ThemeChanged));
            }
        }

        /// <inheritdoc />
        public AppTheme PlatformAppTheme => Application.Current!.PlatformAppTheme;

        /// <inheritdoc />
        public AppTheme UserAppTheme
        {
            get => Application.Current!.UserAppTheme;
            private set => Application.Current!.UserAppTheme = value;
        }

        /// <inheritdoc />
        public bool UseSystemTheme
        {
            get => this.useSystemTheme ?? (this.useSystemTheme = this.preferences.Get(UseSystemThemeSettingsKey, DefaultUseSystemTheme)) ?? DefaultUseSystemTheme;
            set
            {
                if (this.StoreUseSystemTheme(value))
                {
                    this.ApplyTheme();
                }
            }
        }

        public AppTheme AppTheme
        {
            get
            {
                if (this.appTheme is AppTheme appTheme)
                {
                    return appTheme;
                }

                if (this.preferences.ContainsKey(AppThemeSettingsKey))
                {
                    return (this.appTheme = this.preferences.Get(AppThemeSettingsKey, AppTheme.Unspecified)).Value;
                }

                // Until a theme is chosen, the preferred theme is the one shown, so that no longer following the
                // system theme keeps it. Not cached: while the system theme is followed, the shown theme changes.
                return Application.Current?.RequestedTheme ?? AppTheme.Unspecified;
            }
            set
            {
                if (this.StoreAppTheme(value))
                {
                    // Choosing a theme stops following the system theme. Both values are stored before
                    // the theme is applied, so it is applied once, with the new value.
                    this.StoreUseSystemTheme(false);
                    this.ApplyTheme();
                }
            }
        }

        /// <summary>
        /// Stores <paramref name="value"/> as <see cref="UseSystemTheme"/> without applying the theme.
        /// </summary>
        /// <returns><c>true</c> if the stored value changed.</returns>
        private bool StoreUseSystemTheme(bool value)
        {
            if (this.useSystemTheme == value)
            {
                return false;
            }

            this.useSystemTheme = value;
            this.preferences.Set(UseSystemThemeSettingsKey, value);
            return true;
        }

        /// <summary>
        /// Stores <paramref name="value"/> as <see cref="AppTheme"/> without applying the theme.
        /// </summary>
        /// <returns><c>true</c> if the stored value changed.</returns>
        private bool StoreAppTheme(AppTheme value)
        {
            if (this.appTheme == value)
            {
                return false;
            }

            this.appTheme = value;
            this.preferences.Set(AppThemeSettingsKey, value);
            return true;
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

                this.UnsubscribeFromSystemTheme();

                // Register for system theme changes
                this.systemThemeHandler = (s, e) =>
                {
                    // Store the last detected system theme
                    this.lastUsedTheme = e.RequestedTheme;

                    if (this.UseSystemTheme && !this.isUpdatingUserAppTheme)
                    {
                        // If following system theme, update when system changes
                        this.ApplyTheme();
                    }
                };

                this.subscribedApplication = Application.Current;
                this.subscribedApplication.RequestedThemeChanged += this.systemThemeHandler;

                // Store initial system theme
                this.lastUsedTheme = this.GetCurrentTheme();

                // Apply the initial theme
                this.ApplyTheme();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Initialize failed with exception");
            }
        }

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

                this.UpdateUserAppTheme(application);

                // Apply theme resources
                this.ApplyThemeResources(currentTheme);

                var hasThemeChanged = this.appliedTheme != currentTheme;
                this.appliedTheme = currentTheme;
                this.lastUsedTheme = currentTheme;

                if (hasThemeChanged)
                {
                    this.RaiseThemeChangedEvent(currentTheme);
                }
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

        /// <summary>
        /// Pins <see cref="Application.UserAppTheme"/> to the chosen theme, or leaves it
        /// <see cref="AppTheme.Unspecified"/> while the system theme is followed.
        /// </summary>
        /// <remarks>
        /// A pinned user theme overrides the platform's theme, so a change of the system theme would no longer reach
        /// the app. MAUI passes the user theme on to the platform only when the requested theme changes, which pinning
        /// the theme the system already shows does not do, so the handler is asked to pass it on here.
        /// </remarks>
        private void UpdateUserAppTheme(Application application)
        {
            var userAppTheme = this.UseSystemTheme ? AppTheme.Unspecified : this.AppTheme;
            if (application.UserAppTheme == userAppTheme)
            {
                return;
            }

            try
            {
                this.isUpdatingUserAppTheme = true;
                application.UserAppTheme = userAppTheme;
                application.Handler?.UpdateValue(nameof(IApplication.UserAppTheme));
            }
            finally
            {
                this.isUpdatingUserAppTheme = false;
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

                this.logger.LogDebug("ApplyThemeResources: {AppTheme} theme {ThemeName}", appTheme, themeName);

                var theme = Application.Current.Resources.GetValue<ITheme>(themeName);

                var superdevMauiStyles = globalMergedDictionary.FirstOrDefault<SuperdevMauiStyles>();
                if (superdevMauiStyles != null)
                {
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

                    // A font configuration subscribes to the font converter, which is a singleton, so an
                    // instance this helper created has to be released before it is replaced. Otherwise
                    // every theme change leaves one behind, permanently reachable from that singleton.
                    this.ownedFontConfiguration?.Dispose();
                    this.ownedFontConfiguration = null;

                    var fontConfiguration = theme.FontConfiguration;
                    if (fontConfiguration == null)
                    {
                        fontConfiguration = new FontConfiguration(this.fontConverter);
                        this.ownedFontConfiguration = fontConfiguration;
                    }

                    fontConfiguration.Initialize();
                    localMergedDictionaries.Add(fontConfiguration.Resources);
                }

                if (this.OverrideStyles || this.MergeStyles)
                {
                    this.OverrideStylesInternal(overrideStyles: this.OverrideStyles, mergeStyles: this.MergeStyles);
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

        /// <inheritdoc />
        public void ToggleTheme()
        {
            // Switches to the manual theme opposite to the one shown, and applies it once.
            var shownTheme = this.UseSystemTheme
                ? Application.Current?.PlatformAppTheme
                : this.AppTheme;
            var toggledTheme = shownTheme == AppTheme.Dark
                ? AppTheme.Light
                : AppTheme.Dark;

            this.StoreAppTheme(toggledTheme);
            this.StoreUseSystemTheme(false);
            this.ApplyTheme();
        }

#if ANDROID
        public void OnResume()
        {
            var currentTheme = this.GetCurrentTheme();
            if (currentTheme != this.appliedTheme)
            {
                this.ApplyTheme();
            }
        }
#endif

        private void UnsubscribeFromSystemTheme()
        {
            if (this.subscribedApplication != null && this.systemThemeHandler != null)
            {
                this.subscribedApplication.RequestedThemeChanged -= this.systemThemeHandler;
            }

            this.subscribedApplication = null;
            this.systemThemeHandler = null;
        }

        /// <summary>
        /// Releases the subscription to the system theme and the font configuration this helper created itself.
        /// <see cref="Reset"/> or <see cref="ApplyTheme(string)"/> subscribe again.
        /// </summary>
        public void Dispose()
        {
            this.UnsubscribeFromSystemTheme();

            this.ownedFontConfiguration?.Dispose();
            this.ownedFontConfiguration = null;
        }

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