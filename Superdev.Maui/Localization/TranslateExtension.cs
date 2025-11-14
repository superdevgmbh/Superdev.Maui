using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Maui.Controls.Internals;

namespace Superdev.Maui.Localization
{
    [Preserve(AllMembers = true)]
    [ContentProperty("Key")]
    public class TranslateExtension : IMarkupExtension<BindingBase>
    {
        private static ILocalizer Localizer = new NullLocalizer();
        private static TranslateExtensionMultiConverter MultiConverter;

        public static void Init(ILocalizer localizer, ITranslationProvider translationProvider)
        {
            Localizer = localizer;
            MultiConverter = new TranslateExtensionMultiConverter(translationProvider);
        }

        public string Key { get; set; }

        public IValueConverter Converter { get; set; }

        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.Key == null)
            {
                return null;
            }

            BindingBase binding;

            if (this.Converter != null)
            {
                // If Converter is not null, Key is used as a binding path.
                binding = new MultiBinding
                {
                    Bindings = new BindingBase[]
                    {
                        new Binding(this.Key, converter: this.Converter),
                        new Binding(nameof(Localizer.CurrentCultureInfo))
                        {
                            Source = Localizer
                        }
                    },
                    Converter = MultiConverter
                };
            }
            else
            {
                // If Converter is null, Key is used as translation key
                binding = new MultiBinding
                {
                    Bindings = new BindingBase[]
                    {
                        new Binding(Binding.SelfPath)
                        {
                            Source = this.Key
                        },
                        new Binding(nameof(Localizer.CurrentCultureInfo))
                        {
                            Source = Localizer
                        }
                    },
                    Converter = MultiConverter
                };
            }

            return binding;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return this.ProvideValue(serviceProvider);
        }

        private class TranslateExtensionMultiConverter : IMultiValueConverter
        {
            private readonly ITranslationProvider translationProvider;

            public TranslateExtensionMultiConverter(ITranslationProvider translationProvider)
            {
                this.translationProvider = translationProvider;
            }

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                if (values != null && values.Any())
                {
                    if (values.ElementAtOrDefault(0) is string key &&
                        values.ElementAtOrDefault(1) is CultureInfo cultureInfo)
                    {
                        var translate = this.translationProvider.Translate(key, cultureInfo);
                        return translate;
                    }
                }

                return null;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException("ConvertBack is not supported");
            }
        }
    }

    /// <summary>
    /// Null implementation of <seealso cref="ITranslationProvider"/>
    /// in order to prevent NullReferenceExceptions in case TranslateExtension is not yet initialized.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class NullTranslationProvider : ITranslationProvider
    {
        public string Translate(string key, CultureInfo cultureInfo)
        {
            return $"No key found for '{key}'";
        }
    }

    /// <summary>
    /// Null implementation of <seealso cref="ILocalizer"/>
    /// in order to prevent NullReferenceExceptions in case TranslateExtension is not yet initialized.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class NullLocalizer : ILocalizer
    {
        public CultureInfo CurrentCultureInfo
        {
            get => CultureInfo.CurrentCulture;
            set => CultureInfo.CurrentCulture = value;
        }

        public event EventHandler<LanguageChangingEventArgs> LanguageChanging;

        public event EventHandler<LanguageChangedEventArgs> LanguageChanged;
    }
}