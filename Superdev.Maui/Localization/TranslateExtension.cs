using System.Globalization;
using Superdev.Maui.Internals;

namespace Superdev.Maui.Localization
{
    [Preserve(AllMembers = true)]
    [ContentProperty("Text")]
    public class TranslateExtension : BindableObject, IMarkupExtension<BindingBase>
    {
        private static ILocalizer Localizer = new NullLocalizer();
        private static ITranslationProvider TranslationProvider = new NullTranslationProvider();

        public static void Init(ILocalizer localizer, ITranslationProvider translationProvider)
        {
            Localizer = localizer;
            TranslationProvider = translationProvider;
        }

        public string Text { get; set; }

        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding(nameof(TranslationData.Value))
            {
                Source = new TranslationData(this.Text, Localizer, TranslationProvider)
            };
            return binding;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return this.ProvideValue(serviceProvider);
        }

        private class NullTranslationProvider : ITranslationProvider
        {
            public string Translate(string key, CultureInfo cultureInfo)
            {
                return $"Call {nameof(TranslateExtension)}.{nameof(Init)}!";
            }
        }

        private class NullLocalizer : ILocalizer
        {
            public CultureInfo GetCurrentCulture()
            {
                return CultureInfo.CurrentCulture;
            }

            public void SetCultureInfo(CultureInfo cultureInfo)
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }

            public event EventHandler<LanguageChangingEventArgs> LanguageChanging;

            public event EventHandler<LanguageChangedEventArgs> LanguageChanged;
        }
    }
}