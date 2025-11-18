using Superdev.Maui.Services;
using Superdev.Maui.Utils;
using DeviceInfo = Superdev.Maui.Services.DeviceInfo;

namespace Superdev.Maui.Resources.Styles
{
    /// <summary>
    ///     Font configuration with named fonts, based on ideas behind https://material.io/design/typography.
    /// </summary>
    public sealed class FontConfiguration : BindableObject, IDisposable
    {
        private readonly IFontConverter fontConverter;

        private bool isInitialized;

        public FontResources Resources { get; }

        public FontConfiguration()
            : this(IFontConverter.Current)
        {
        }

        public FontConfiguration(IFontConverter fontConverter)
        {
            this.Resources = new FontResources();

            this.fontConverter = fontConverter;
            this.fontConverter.FontScalingChanged += this.FontConverterUIContentSizeChanged;
        }

        internal void Initialize()
        {
            if (this.isInitialized)
            {
                return;
            }

            var fontSizes = this.fontConverter.GetScaledFontSizes(this.FontSizes);
            this.SetFontSizes(fontSizes);
            this.SetFonts(fontSizes);
            this.isInitialized = true;
        }

        private void FontConverterUIContentSizeChanged(object? sender, EventArgs e)
        {
            var fontSizes = this.fontConverter.GetScaledFontSizes(this.FontSizes);
            this.SetFontSizes(fontSizes);
            this.SetFonts(fontSizes);
        }

        private void SetFontSizes(IFontSizeConfiguration fontSizes)
        {
            this.Resources.SetValue(ThemeConstants.FontSize.Micro, this.fontConverter.GetScaledFontSize(fontSizes.Micro));
            this.Resources.SetValue(ThemeConstants.FontSize.XSmall, this.fontConverter.GetScaledFontSize(fontSizes.XSmall));
            this.Resources.SetValue(ThemeConstants.FontSize.Small, this.fontConverter.GetScaledFontSize(fontSizes.Small));
            this.Resources.SetValue(ThemeConstants.FontSize.MidMedium, this.fontConverter.GetScaledFontSize(fontSizes.MidMedium));
            this.Resources.SetValue(ThemeConstants.FontSize.Medium, this.fontConverter.GetScaledFontSize(fontSizes.Medium));
            this.Resources.SetValue(ThemeConstants.FontSize.Large, this.fontConverter.GetScaledFontSize(fontSizes.Large));
            this.Resources.SetValue(ThemeConstants.FontSize.XLarge, this.fontConverter.GetScaledFontSize(fontSizes.XLarge));
            this.Resources.SetValue(ThemeConstants.FontSize.XXLarge, this.fontConverter.GetScaledFontSize(fontSizes.XXLarge));
            this.Resources.SetValue(ThemeConstants.FontSize.XXXLarge, this.fontConverter.GetScaledFontSize(fontSizes.XXXLarge));
        }

        private void SetFonts(IFontSizeConfiguration fontSizes)
        {
            var defaultFontFamily = TryGetFontFamily(this.Default, null);
            var defaultFontSize = TryGetFontSize(this.Default, 0d);
            var defaultFontAttributes = TryGetFontAttributes(this.Default);
            this.Resources.SetValue(ThemeConstants.FontFamily.Default, defaultFontFamily);
            this.Resources.SetValue(ThemeConstants.FontSize.Default, defaultFontSize);
            this.Resources.SetValue(ThemeConstants.FontAttributes.Default, defaultFontAttributes);

            this.Resources.SetValue(ThemeConstants.FontFamily.Body1, TryGetFontFamily(this.Body1, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.Body1, TryGetFontSize(this.Body1, fontSizes.MidMedium));
            this.Resources.SetValue(ThemeConstants.FontAttributes.Body1, TryGetFontAttributes(this.Body1, defaultFontAttributes));

            this.Resources.SetValue(ThemeConstants.FontFamily.Body2, TryGetFontFamily(this.Body2, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.Body2, TryGetFontSize(this.Body2, fontSizes.Small));
            this.Resources.SetValue(ThemeConstants.FontAttributes.Body2, TryGetFontAttributes(this.Body2, defaultFontAttributes));

            this.Resources.SetValue(ThemeConstants.FontFamily.Button, TryGetFontFamily(this.Button, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.Button, TryGetFontSize(this.Button, fontSizes.Medium));
            this.Resources.SetValue(ThemeConstants.FontAttributes.Button, TryGetFontAttributes(this.Button, defaultFontAttributes));

            this.Resources.SetValue(ThemeConstants.FontFamily.Input, TryGetFontFamily(this.Input, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.Input, TryGetFontSize(this.Input, fontSizes.Medium));
            this.Resources.SetValue(ThemeConstants.FontAttributes.Input, TryGetFontAttributes(this.Input, defaultFontAttributes));

            this.Resources.SetValue(ThemeConstants.FontFamily.Caption, TryGetFontFamily(this.Caption, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.Caption, TryGetFontSize(this.Caption, fontSizes.XSmall));
            this.Resources.SetValue(ThemeConstants.FontAttributes.Caption, TryGetFontAttributes(this.Caption, defaultFontAttributes));

            this.Resources.SetValue(ThemeConstants.FontFamily.H1, TryGetFontFamily(this.H1, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.H1, TryGetFontSize(this.H1, 96d));
            this.Resources.SetValue(ThemeConstants.FontAttributes.H1, TryGetFontAttributes(this.H1, defaultFontAttributes));

            this.Resources.SetValue(ThemeConstants.FontFamily.H2, TryGetFontFamily(this.H2, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.H2, TryGetFontSize(this.H2, 60d));
            this.Resources.SetValue(ThemeConstants.FontAttributes.H2, TryGetFontAttributes(this.H2, defaultFontAttributes));

            this.Resources.SetValue(ThemeConstants.FontFamily.H3, TryGetFontFamily(this.H3, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.H3, TryGetFontSize(this.H3, 48d));
            this.Resources.SetValue(ThemeConstants.FontAttributes.H3, TryGetFontAttributes(this.H3, defaultFontAttributes));

            this.Resources.SetValue(ThemeConstants.FontFamily.H4, TryGetFontFamily(this.H4, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.H4, TryGetFontSize(this.H4, 34d));
            this.Resources.SetValue(ThemeConstants.FontAttributes.H4, TryGetFontAttributes(this.H4, defaultFontAttributes));

            this.Resources.SetValue(ThemeConstants.FontFamily.H5, TryGetFontFamily(this.H5, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.H5, TryGetFontSize(this.H5, 24d));
            this.Resources.SetValue(ThemeConstants.FontAttributes.H5, TryGetFontAttributes(this.H5, defaultFontAttributes));

            this.Resources.SetValue(ThemeConstants.FontFamily.H6, TryGetFontFamily(this.H6, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.H6, TryGetFontSize(this.H6, 20d));
            this.Resources.SetValue(ThemeConstants.FontAttributes.H6, TryGetFontAttributes(this.H6, defaultFontAttributes));

            this.Resources.SetValue(ThemeConstants.FontFamily.Overline, TryGetFontFamily(this.Overline, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.Overline, TryGetFontSize(this.Overline, fontSizes.Micro));
            this.Resources.SetValue(ThemeConstants.FontAttributes.Overline, TryGetFontAttributes(this.Overline, defaultFontAttributes));

            this.Resources.SetValue(ThemeConstants.FontFamily.Title, TryGetFontFamily(this.Title, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.Title, TryGetFontSize(this.Title, fontSizes.Medium));
            this.Resources.SetValue(ThemeConstants.FontAttributes.Title, TryGetFontAttributes(this.Title, FontAttributes.Bold));

            this.Resources.SetValue(ThemeConstants.FontFamily.Subtitle1, TryGetFontFamily(this.Subtitle1, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.Subtitle1, TryGetFontSize(this.Subtitle1, fontSizes.MidMedium));
            this.Resources.SetValue(ThemeConstants.FontAttributes.Subtitle1, TryGetFontAttributes(this.Subtitle1, FontAttributes.Bold));

            this.Resources.SetValue(ThemeConstants.FontFamily.Subtitle2, TryGetFontFamily(this.Subtitle2, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.FontSize.Subtitle2, TryGetFontSize(this.Subtitle2, fontSizes.Small));
            this.Resources.SetValue(ThemeConstants.FontAttributes.Subtitle2, TryGetFontAttributes(this.Subtitle2, defaultFontAttributes));

            this.Resources.SetValue(ThemeConstants.CardViewStyle.HeaderFontFamily, TryGetFontFamily(this.SectionLabel, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.CardViewStyle.HeaderFontSize, TryGetFontSize(this.SectionLabel, PlatformHelper.OnPlatformValue((DevicePlatform.iOS, () => 13d), (DevicePlatform.Android, () => 18d))));
            this.Resources.SetValue(ThemeConstants.CardViewStyle.HeaderFontAttributes, TryGetFontAttributes(this.SectionLabel, defaultFontAttributes));

            this.Resources.SetValue(ThemeConstants.CardViewStyle.FooterFontFamily, TryGetFontFamily(this.FooterSection, defaultFontFamily));
            this.Resources.SetValue(ThemeConstants.CardViewStyle.FooterFontSize, TryGetFontSize(this.FooterSection, fontSizes.XSmall));
            this.Resources.SetValue(ThemeConstants.CardViewStyle.FooterFontAttributes, TryGetFontAttributes(this.FooterSection, defaultFontAttributes));
        }

        private static string? TryGetFontFamily(FontElement? fontElement, string? @default)
        {
            var fontFamily = fontElement?.FontFamily;
            return fontFamily ?? @default;
        }

        private static double? TryGetFontSize(FontElement? fontElement, double? @default)
        {
            var fontSize = fontElement?.FontSize;
            if (fontSize is > 0)
            {
                return fontSize;
            }

            return @default;
        }

        private static FontAttributes TryGetFontAttributes(FontElement fontElement, FontAttributes @default = FontAttributes.None)
        {
            return fontElement?.FontAttributes ?? @default;
        }

        public static readonly BindableProperty FontSizesProperty =
            BindableProperty.Create(
                nameof(FontSizes),
                typeof(IFontSizeConfiguration),
                typeof(FontConfiguration),
                new DefaultFontSizeConfiguration());

        public IFontSizeConfiguration FontSizes
        {
            get => (IFontSizeConfiguration)this.GetValue(FontSizesProperty);
            set => this.SetValue(FontSizesProperty, value);
        }

        public static readonly BindableProperty DefaultProperty =
            BindableProperty.Create(
                nameof(Default),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        public FontElement Default
        {
            get => (FontElement)this.GetValue(DefaultProperty);
            set => this.SetValue(DefaultProperty, value);
        }

        public static readonly BindableProperty SectionLabelProperty =
            BindableProperty.Create(
                nameof(SectionLabel),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        ///     Section label font, used as header font in card views.
        /// </summary>
        public FontElement SectionLabel
        {
            get => (FontElement)this.GetValue(SectionLabelProperty);
            set => this.SetValue(SectionLabelProperty, value);
        }

        public static readonly BindableProperty FooterSectionProperty =
            BindableProperty.Create(
                nameof(FooterSection),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        ///     Footer label, used as footer text in card views.
        /// </summary>
        public FontElement FooterSection
        {
            get => (FontElement)this.GetValue(FooterSectionProperty);
            set => this.SetValue(FooterSectionProperty, value);
        }

        public static readonly BindableProperty Body1Property =
            BindableProperty.Create(
                nameof(Body1),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        ///     Body 1 font, used for long-form writing and small text sizes.
        /// </summary>
        public FontElement Body1
        {
            get => (FontElement)this.GetValue(Body1Property);
            set => this.SetValue(Body1Property, value);
        }

        public static readonly BindableProperty Body2Property =
            BindableProperty.Create(
                nameof(Body2),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        ///     Body 2 font, used for long-form writing and small text sizes.
        /// </summary>
        public FontElement Body2
        {
            get => (FontElement)this.GetValue(Body2Property);
            set => this.SetValue(Body2Property, value);
        }

        public static readonly BindableProperty ButtonProperty =
            BindableProperty.Create(
                nameof(Button),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        public FontElement Button
        {
            get => (FontElement)this.GetValue(ButtonProperty);
            set => this.SetValue(ButtonProperty, value);
        }

        public static readonly BindableProperty InputProperty =
            BindableProperty.Create(
                nameof(Input),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        /// Input font, used for all kinds of user input fields (text entry, auto-complete entry, pickers, etc...).
        /// </summary>
        public FontElement Input
        {
            get => (FontElement)this.GetValue(InputProperty);
            set => this.SetValue(InputProperty, value);
        }

        /// <summary>
        ///     Caption font, used for annotations or to introduce a headline text.
        /// </summary>
        public static readonly BindableProperty CaptionProperty =
            BindableProperty.Create(
                nameof(Caption),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        public FontElement Caption
        {
            get => (FontElement)this.GetValue(CaptionProperty);
            set => this.SetValue(CaptionProperty, value);
        }

        public static readonly BindableProperty H1Property =
            BindableProperty.Create(
                nameof(H1),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        ///     Headline 1 font, used by large text on the screen.
        /// </summary>
        public FontElement H1
        {
            get => (FontElement)this.GetValue(H1Property);
            set => this.SetValue(H1Property, value);
        }

        public static readonly BindableProperty H2Property =
            BindableProperty.Create(
                nameof(H2),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        ///     Headline 2 font, used by large text on the screen.
        /// </summary>
        public FontElement H2
        {
            get => (FontElement)this.GetValue(H2Property);
            set => this.SetValue(H2Property, value);
        }

        public static readonly BindableProperty H3Property =
            BindableProperty.Create(
                nameof(H3),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        ///     Headline 3 font, used by large text on the screen.
        /// </summary>
        public FontElement H3
        {
            get => (FontElement)this.GetValue(H3Property);
            set => this.SetValue(H3Property, value);
        }

        public static readonly BindableProperty H4Property =
            BindableProperty.Create(
                nameof(H4),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        ///     Headline 4 font, used by large text on the screen.
        /// </summary>
        public FontElement H4
        {
            get => (FontElement)this.GetValue(H4Property);
            set => this.SetValue(H4Property, value);
        }

        public static readonly BindableProperty H5Property =
            BindableProperty.Create(
                nameof(H5),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        ///     Headline 5 font, used by large text on the screen.
        /// </summary>
        public FontElement H5
        {
            get => (FontElement)this.GetValue(H5Property);
            set => this.SetValue(H5Property, value);
        }

        public static readonly BindableProperty H6Property =
            BindableProperty.Create(
                nameof(H6),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        ///     Headline 6 font, used by large text on the screen.
        /// </summary>
        public FontElement H6
        {
            get => (FontElement)this.GetValue(H6Property);
            set => this.SetValue(H6Property, value);
        }

        public static readonly BindableProperty OverlineProperty =
            BindableProperty.Create(
                nameof(Overline),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        ///     Overline font, used for annotations or to introduce a headline text.
        /// </summary>
        public FontElement Overline
        {
            get => (FontElement)this.GetValue(OverlineProperty);
            set => this.SetValue(OverlineProperty, value);
        }

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(
                nameof(Title),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        ///     Title font, used by as page title, list group headers, loading indicators.
        /// </summary>
        public FontElement Title
        {
            get => (FontElement)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }

        public static readonly BindableProperty Subtitle1Property =
            BindableProperty.Create(
                nameof(Subtitle1),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        ///     Subtitle 1 font, used by medium-emphasis text.
        /// </summary>
        public FontElement Subtitle1
        {
            get => (FontElement)this.GetValue(Subtitle1Property);
            set => this.SetValue(Subtitle1Property, value);
        }

        public static readonly BindableProperty Subtitle2Property =
            BindableProperty.Create(
                nameof(Subtitle2),
                typeof(FontElement),
                typeof(FontConfiguration),
                null);

        /// <summary>
        ///     Subtitle 2 font, used by medium-emphasis text.
        /// </summary>
        public FontElement Subtitle2
        {
            get => (FontElement)this.GetValue(Subtitle2Property);
            set => this.SetValue(Subtitle2Property, value);
        }

        public void Dispose()
        {
            this.fontConverter.FontScalingChanged -= this.FontConverterUIContentSizeChanged;
            this.fontConverter?.Dispose();
        }
    }
}