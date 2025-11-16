using System.Globalization;
using System.Runtime.CompilerServices;
using Superdev.Maui.Internals;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;

#if ANDROID
using Android.OS;
using Locale = Java.Util.Locale;
using Superdev.Maui.Platforms.Extensions;
#endif

namespace Superdev.Maui.Localization
{
    [Preserve(AllMembers = true)]
    public class Localizer : BindableBase, ILocalizer
    {
        private readonly IMainThread mainThread;
        private readonly IPreferences preferences;
        private static readonly Lazy<ILocalizer> Implementation = new(CreateLocalizer, LazyThreadSafetyMode.PublicationOnly);

        public static ILocalizer Current => Implementation.Value;

        private static ILocalizer CreateLocalizer()
        {
            return new Localizer(IPreferences.Current, IMainThread.Current, initialize: true);
        }

        private static readonly Dictionary<string, string> LocaleMappings = new(StringComparer.OrdinalIgnoreCase)
        {
            { "ms-BN", "ms" },
            { "ms-MY", "ms" },
            { "ms-SG", "ms" },
            { "in-ID", "id-ID" },
            { "gsw", "de-CH" },
            { "gsw-CH", "de-CH" },
            { "es-419", "es-ES" }, // Latin America Spanish
            { "zh-Hans", "zh-CN" }, // Simplified Chinese
            { "zh-Hant", "zh-TW" }, // Traditional Chinese
            { "no", "nb-NO" }, // Norwegian
            { "iw", "he" }, // Hebrew
            { "ji", "yi" }, // Yiddish
            { "fil", "tl-PH" } // Filipino
        };

        internal const string DefaultPreferencesKey = "Localizer_AppLanguage";
        private CultureInfo platformCulture;
        private CultureInfo overrideCulture;
        private CultureInfo[] supportedLanguages = [];
        private string preferencesKey = DefaultPreferencesKey;
        private CultureInfo defaultLanguage;

        internal Localizer(IPreferences preferences, IMainThread mainThread, bool initialize)
        {
            this.mainThread = mainThread;
            this.preferences = preferences;

            if (initialize)
            {
                this.InitializeFromPreferencesOrSystem();
            }
        }

        public string PreferencesKey
        {
            get => this.preferencesKey;
            set
            {
                if (this.SetProperty(ref this.preferencesKey, value))
                {
                    this.InitializeFromPreferencesOrSystem();
                }
            }
        }

        public CultureInfo[] SupportedLanguages
        {
            get => this.supportedLanguages;
            set
            {
                value = value?
                    .Where(c => c != null && !Equals(c, CultureInfo.InvariantCulture))
                    .ToArray() ?? [];

                if (this.SetProperty(ref this.supportedLanguages, value))
                {
                    this.InitializeFromPreferencesOrSystem();
                }
            }
        }

        public CultureInfo DefaultLanguage
        {
            get => this.defaultLanguage;
            set
            {
                if (this.SetProperty(ref this.defaultLanguage, value))
                {
                    this.InitializeFromPreferencesOrSystem();
                }
            }
        }

        internal void InitializeFromPreferencesOrSystem()
        {
            if (this.GetLanguageFromPreferences() is string language &&
                TryConvertToCultureInfo(language, out var cultureInfo))
            {
                cultureInfo = this.ResolveSupportedCulture(cultureInfo);
                this.overrideCulture = cultureInfo;
            }
            else
            {
                var platformCulture = this.GetPlatformCulture();
                cultureInfo = this.ResolveSupportedCulture(platformCulture);
                this.platformCulture = cultureInfo;
                this.overrideCulture = null;
            }

            this.SetCurrentCultureInternal(cultureInfo);
        }

        private CultureInfo ResolveSupportedCulture(CultureInfo cultureInfo)
        {
            if (this.SupportedLanguages is { Length: > 0 } supportedLanguages)
            {
                // Direct match
                var exactMatch = supportedLanguages.FirstOrDefault(c => string.Equals(c.Name, cultureInfo.Name, StringComparison.InvariantCultureIgnoreCase));
                if (exactMatch != null)
                {
                    return exactMatch;
                }

                // Match by parent/base culture ("fr-CH" → "fr")
                if (cultureInfo.Parent is CultureInfo parent && !Equals(parent, CultureInfo.InvariantCulture))
                {
                    var parentMatch = supportedLanguages.FirstOrDefault(c => c.Name.Equals(parent.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (parentMatch != null)
                    {
                        return parentMatch;
                    }
                }

                // Match by ISO-2 language ("fr-FR" → "fr")
                var twoLetterMatch =
                    supportedLanguages.FirstOrDefault(c => string.Equals(c.TwoLetterISOLanguageName, cultureInfo.TwoLetterISOLanguageName, StringComparison.InvariantCultureIgnoreCase));
                if (twoLetterMatch != null)
                {
                    return twoLetterMatch;
                }

                // Fallback to default language
                if (this.DefaultLanguage is CultureInfo defaultLanguage)
                {
                    return defaultLanguage;
                }

                // Fallback to first supported language
                return supportedLanguages.First();
            }

            if (!Equals(cultureInfo, CultureInfo.InvariantCulture))
            {
                return cultureInfo;
            }

            if (this.DefaultLanguage is CultureInfo defaultLang)
            {
                return defaultLang;
            }

            throw new InvalidOperationException($"Unable to resolve culture. Please specify {nameof(this.SupportedLanguages)} and/or a {nameof(this.DefaultLanguage)}.");
        }

        public CultureInfo CurrentCulture
        {
            get => this.overrideCulture ?? this.platformCulture;
            set
            {
                if (value != null)
                {
                    var currentCulture = this.ResolveSupportedCulture(value);
                    this.overrideCulture = currentCulture;
                    this.SetLanguageToPreferences(currentCulture.Name);
                    this.SetCurrentCultureInternal(currentCulture);
                }
            }
        }

        protected virtual void SetLanguageToPreferences(string language)
        {
            this.preferences.Set(this.PreferencesKey, language);
        }

        protected virtual string GetLanguageFromPreferences()
        {
            return this.preferences.Get<string>(this.PreferencesKey, null);
        }

        private void SetCurrentCultureInternal(CultureInfo cultureInfo)
        {
            ArgumentNullException.ThrowIfNull(cultureInfo);

            if (this.mainThread.IsMainThread)
            {
                SetCultureInfoInternal();
            }
            else
            {
                this.mainThread.BeginInvokeOnMainThread(SetCultureInfoInternal);
            }

            void SetCultureInfoInternal()
            {
                Trace.WriteLine($"SetCultureInfo: cultureInfo={cultureInfo.Name}");

                this.OnLanguageChanging(cultureInfo);

                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                CultureInfo.CurrentCulture = cultureInfo;
                CultureInfo.CurrentUICulture = cultureInfo;

                CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

                this.OnLanguageChanged(cultureInfo);

                this.RaisePropertyChanged(nameof(this.CurrentCulture));
            }
        }

        public event EventHandler<LanguageChangingEventArgs> LanguageChanging;

        private void OnLanguageChanging(CultureInfo ci)
        {
            this.LanguageChanging?.Invoke(this, new LanguageChangingEventArgs(ci));
        }

        public event EventHandler<LanguageChangedEventArgs> LanguageChanged;

        protected virtual void OnLanguageChanged(CultureInfo ci)
        {
            this.LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(ci));
        }

        public virtual string GetPlatformLocale()
        {
#if ANDROID
            Locale locale;

            if (Build.VERSION.SdkInt < BuildVersionCodes.N)
            {
                locale = Locale.Default;
            }
            else
            {
                locale = LocaleList.Default.ToEnumerable().FirstOrDefault();
            }

            var language = locale?.ToString();
            return language;

#elif IOS
            var preferred = Foundation.NSLocale.PreferredLanguages;
            if (preferred is { Length: > 0 })
            {
                return preferred[0];
            }
#endif
            return null;
        }

        public CultureInfo GetPlatformCulture()
        {
            var platformLocale = this.GetPlatformLocale();
            if (platformLocale == null)
            {
                throw new InvalidOperationException($"{nameof(this.GetPlatformLocale)} must not return null.");
            }

            TryConvertToCultureInfo(platformLocale, out var cultureInfo);
            return cultureInfo;
        }

        private static bool TryConvertToCultureInfo(string locale, out CultureInfo cultureInfo)
        {
            locale = NormalizeLocaleString(locale);

            if (LocaleMappings.TryGetValue(locale, out var mapped))
            {
                locale = mapped;
            }

            try
            {
                cultureInfo = new CultureInfo(locale);
                return true;
            }
            catch
            {
                // Try base culture ("fr-CH" → "fr")
                var dash = locale.IndexOf('-');
                if (dash > 0)
                {
                    var baseLang = locale[..dash];
                    try
                    {
                        cultureInfo = new CultureInfo(baseLang);
                        return true;
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }


            cultureInfo = CultureInfo.InvariantCulture;
            return false;
        }

        private static string NormalizeLocaleString(string locale)
        {
            return locale?.Replace('_', '-');
        }

        public void Reset()
        {
            this.preferences.Remove(this.PreferencesKey);
            this.InitializeFromPreferencesOrSystem();
        }
    }
}