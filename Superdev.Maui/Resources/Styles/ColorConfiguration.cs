namespace Superdev.Maui.Resources.Styles
{
    public sealed class ColorConfiguration : BindableObject
    {
        private bool isInitialized;

        public ColorResources Resources { get; }

        public ColorConfiguration()
        {
            this.Resources = new ColorResources();
        }

        internal void Initialize()
        {
            if (this.isInitialized)
            {
                return;
            }

            this.SetThemeColors(this);
            this.SetPageColors(this);
            this.SetButtonColors(this);
            this.SetDrilldownButtonColors(this);
            this.SetCardViewColors(this);
            this.isInitialized = true;
        }

        private void SetPageColors(ColorConfiguration colorConfiguration)
        {
            this.Resources.SetValue(ThemeConstants.Page.BackgroundColor, colorConfiguration.PageBackgroundColor);
        }

        private void SetButtonColors(ColorConfiguration colorConfiguration)
        {
            // Default button style
            this.Resources.SetValue(ThemeConstants.Button.TextColor, colorConfiguration.TextColor);
            this.Resources.SetValue(ThemeConstants.Button.BorderColor, colorConfiguration.TextColor);
            this.Resources.SetValue(ThemeConstants.Button.BackgroundColor, MaterialColors.White);

            this.Resources.SetValue(ThemeConstants.Button.TextColorPressed, MaterialColors.White);
            this.Resources.SetValue(ThemeConstants.Button.BorderColorPressed, colorConfiguration.TextColor);
            this.Resources.SetValue(ThemeConstants.Button.BackgroundColorPressed, colorConfiguration.TextColor);

            this.Resources.SetValue(ThemeConstants.Button.TextColorDisabled, colorConfiguration.PrimaryDisabled);
            this.Resources.SetValue(ThemeConstants.Button.BorderColorDisabled, colorConfiguration.PrimaryDisabled);
            this.Resources.SetValue(ThemeConstants.Button.BackgroundColorDisabled, MaterialColors.Gray200);

            // Primary button style
            this.Resources[ThemeConstants.Button.Primary.TextColor] = colorConfiguration.OnPrimary;
            this.Resources[ThemeConstants.Button.Primary.BorderColor] = colorConfiguration.Primary;
            this.Resources[ThemeConstants.Button.Primary.BackgroundColor] = colorConfiguration.Primary;

            this.Resources[ThemeConstants.Button.Primary.TextColorPressed] = colorConfiguration.OnPrimary;
            this.Resources[ThemeConstants.Button.Primary.BorderColorPressed] = colorConfiguration.Primary;
            this.Resources[ThemeConstants.Button.Primary.BackgroundColorPressed] = colorConfiguration.PrimaryVariant;

            this.Resources[ThemeConstants.Button.Primary.TextColorDisabled] = colorConfiguration.OnPrimary;
            this.Resources[ThemeConstants.Button.Primary.BorderColorDisabled] = colorConfiguration.PrimaryDisabled;
            this.Resources[ThemeConstants.Button.Primary.BackgroundColorDisabled] = colorConfiguration.PrimaryDisabled;

            // Secondary button style
            this.Resources[ThemeConstants.Button.Secondary.TextColor] = colorConfiguration.Secondary;
            this.Resources[ThemeConstants.Button.Secondary.BorderColor] = colorConfiguration.Secondary;
            this.Resources[ThemeConstants.Button.Secondary.BackgroundColor] = colorConfiguration.OnSecondary;

            this.Resources[ThemeConstants.Button.Secondary.TextColorPressed] = colorConfiguration.Secondary;
            this.Resources[ThemeConstants.Button.Secondary.BorderColorPressed] = colorConfiguration.Secondary;
            this.Resources[ThemeConstants.Button.Secondary.BackgroundColorPressed] = colorConfiguration.SecondaryVariant;

            this.Resources[ThemeConstants.Button.Secondary.TextColorDisabled] = colorConfiguration.PrimaryDisabled;
            this.Resources[ThemeConstants.Button.Secondary.BorderColorDisabled] = colorConfiguration.PrimaryDisabled;
            this.Resources[ThemeConstants.Button.Secondary.BackgroundColorDisabled] = MaterialColors.Gray200;
        }

        private void SetDrilldownButtonColors(ColorConfiguration colorConfiguration)
        {
            this.Resources[ThemeConstants.DrilldownButtonStyle.TextColor] = colorConfiguration.TextColor;
            this.Resources[ThemeConstants.DrilldownButtonStyle.BorderColorEnabled] = Colors.Transparent;
            this.Resources[ThemeConstants.DrilldownButtonStyle.BorderColorDisabled] = Colors.Transparent;
            this.Resources[ThemeConstants.DrilldownButtonStyle.BorderColorPressed] = Colors.Transparent;
            this.Resources[ThemeConstants.DrilldownButtonStyle.BackgroundColorEnabled] = Colors.Transparent;
            this.Resources[ThemeConstants.DrilldownButtonStyle.BackgroundColorDisabled] = MaterialColors.SemiTransparentBright;
            this.Resources[ThemeConstants.DrilldownButtonStyle.BackgroundColorPressed] = MaterialColors.SemiTransparentBright;
        }

        private void SetCardViewColors(ColorConfiguration colorConfiguration)
        {
            this.Resources.SetValue(ThemeConstants.CardViewStyle.HeaderTextColor, colorConfiguration.CardViewHeaderTextColor);
            this.Resources.SetValue(ThemeConstants.CardViewStyle.HeaderBackgroundColor, colorConfiguration.CardViewHeaderBackgroundColor);
            this.Resources.SetValue(ThemeConstants.CardViewStyle.HeaderDividerColor, colorConfiguration.CardViewDividerColor);
            this.Resources.SetValue(ThemeConstants.CardViewStyle.BackgroundColor, colorConfiguration.CardViewBackgroundColor);
            this.Resources.SetValue(ThemeConstants.CardViewStyle.FooterTextColor, colorConfiguration.CardViewFooterTextColor);
            this.Resources.SetValue(ThemeConstants.CardViewStyle.FooterDividerColor, colorConfiguration.CardViewDividerColor);
        }

        private void SetThemeColors(ColorConfiguration colorConfiguration)
        {
            this.Resources.SetValue(ThemeConstants.Color.TextColor, colorConfiguration.TextColor);
            this.Resources.SetValue(ThemeConstants.Color.TextColorBright, colorConfiguration.TextColorBright);
            this.Resources.SetValue(ThemeConstants.Color.Primary, colorConfiguration.Primary);
            this.Resources.SetValue(ThemeConstants.Color.PrimaryVariant, colorConfiguration.PrimaryVariant);
            this.Resources.SetValue(ThemeConstants.Color.PrimaryDisabled, colorConfiguration.PrimaryDisabled);
            this.Resources.SetValue(ThemeConstants.Color.OnPrimary, colorConfiguration.OnPrimary);
            this.Resources.SetValue(ThemeConstants.Color.Secondary, colorConfiguration.Secondary);
            this.Resources.SetValue(ThemeConstants.Color.SecondaryVariant, colorConfiguration.SecondaryVariant);
            this.Resources.SetValue(ThemeConstants.Color.SecondaryDisabled, colorConfiguration.SecondaryDisabled);
            this.Resources.SetValue(ThemeConstants.Color.OnSecondary, colorConfiguration.OnSecondary);
            this.Resources.SetValue(ThemeConstants.Color.Tertiary, colorConfiguration.Tertiary);
            this.Resources.SetValue(ThemeConstants.Color.TertiaryVariant, colorConfiguration.TertiaryVariant);
            this.Resources.SetValue(ThemeConstants.Color.TertiaryDisabled, colorConfiguration.TertiaryDisabled);
            this.Resources.SetValue(ThemeConstants.Color.OnTertiary, colorConfiguration.OnTertiary);
            this.Resources.SetValue(ThemeConstants.Color.Error, colorConfiguration.Error);
            this.Resources.SetValue(ThemeConstants.Color.ErrorBackground, colorConfiguration.ErrorBackground);

            this.Resources.SetValue(ThemeConstants.Color.SemiTransparentBright, MaterialColors.SemiTransparentBright);
            this.Resources.SetValue(ThemeConstants.Color.SemiTransparentDark, MaterialColors.SemiTransparentDark);
        }

        public static readonly BindableProperty ErrorProperty = BindableProperty.Create(
            nameof(Error),
            typeof(Color),
            typeof(Color),
            Color.FromArgb("#B00020"),
            propertyChanged: OnErrorPropertyChanged);

        private static void OnErrorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Color.Error, newValue);
        }

        /// <summary>
        ///     The color used to indicate error status.
        /// </summary>
        public Color Error
        {
            get => (Color)this.GetValue(ErrorProperty);
            set => this.SetValue(ErrorProperty, value);
        }

        public static readonly BindableProperty ErrorBackgroundProperty = BindableProperty.Create(
            nameof(ErrorBackground),
            typeof(Color),
            typeof(Color),
            Color.FromArgb("#FFCCD5"));

        /// <summary>
        ///     The color used as background for error callouts, message boxes, error signs.
        /// </summary>
        public Color ErrorBackground
        {
            get => (Color)this.GetValue(ErrorBackgroundProperty);
            set => this.SetValue(ErrorBackgroundProperty, value);
        }

        public static readonly BindableProperty PrimaryProperty = BindableProperty.Create(
            nameof(Primary),
            typeof(Color),
            typeof(Color),
            Color.FromArgb("#6200EE"),
            propertyChanged: OnPrimaryPropertyChanged);

        private static void OnPrimaryPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Color.Primary, newValue);
        }

        private static void UpdateColorProperty(BindableObject bindable, string key, object newValue)
        {
            if (bindable is not ColorConfiguration colorConfiguration)
            {
                return;
            }

            if (colorConfiguration.isInitialized == false)
            {
                return;
            }

            if (newValue is Color color)
            {
                colorConfiguration.Resources.SetValue(key, color);
            }
            else
            {
                colorConfiguration.Resources.Remove(key);
            }
        }

        /// <summary>
        ///     Displayed most frequently across your app.
        /// </summary>
        public Color Primary
        {
            get => (Color)this.GetValue(PrimaryProperty);
            set => this.SetValue(PrimaryProperty, value);
        }

        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(Color),
            MaterialColors.Gray800,
            propertyChanged: OnTextColorProperty);

        private static void OnTextColorProperty(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Color.TextColor, newValue);
        }

        /// <summary>
        ///     Displayed most frequently across your app.
        /// </summary>
        public Color TextColor
        {
            get => (Color)this.GetValue(TextColorProperty);
            set => this.SetValue(TextColorProperty, value);
        }

        public static readonly BindableProperty TextColorBrightProperty = BindableProperty.Create(
            nameof(TextColorBright),
            typeof(Color),
            typeof(Color),
            MaterialColors.Gray500,
            propertyChanged: OnTextColorBrightProperty);

        private static void OnTextColorBrightProperty(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Color.TextColorBright, newValue);
        }

        /// <summary>
        ///     Displayed most frequently across your app.
        /// </summary>
        public Color TextColorBright
        {
            get => (Color)this.GetValue(TextColorBrightProperty);
            set => this.SetValue(TextColorBrightProperty, value);
        }

        public static readonly BindableProperty SecondaryProperty =
            BindableProperty.Create(
                nameof(Secondary),
                typeof(Color),
                typeof(Color),
                default(Color),
                propertyChanged: OnSecondaryPropertyChanged);

        private static void OnSecondaryPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Color.Secondary, newValue);
        }

        public static readonly BindableProperty OnPrimaryProperty = BindableProperty.Create(
            nameof(OnPrimary),
            typeof(Color),
            typeof(Color),
            Color.FromArgb("#FFFFFF"),
            propertyChanged: OnOnPrimaryPropertyChanged);

        private static void OnOnPrimaryPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Color.OnPrimary, newValue);
        }

        /// <summary>
        ///     A color that passes accessibility guidelines for text/iconography when drawn on top of <see cref="Primary" />.
        /// </summary>
        public Color OnPrimary
        {
            get => (Color)this.GetValue(OnPrimaryProperty);
            set => this.SetValue(OnPrimaryProperty, value);
        }

        public static readonly BindableProperty OnSecondaryProperty = BindableProperty.Create(
            nameof(OnSecondary),
            typeof(Color),
            typeof(Color),
            Color.FromArgb("#FFFFFF"),
            propertyChanged: OnOnSecondaryPropertyChanged);

        private static void OnOnSecondaryPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Color.OnSecondary, newValue);
        }

        /// <summary>
        ///     A color that passes accessibility guidelines for text/iconography when drawn on top of <see cref="Secondary" />.
        /// </summary>
        public Color OnSecondary
        {
            get
            {
                var color = (Color)this.GetValue(OnSecondaryProperty);

                return color.IsDefault() ? this.OnPrimary : color;
            }

            set => this.SetValue(OnSecondaryProperty, value);
        }

        public static readonly BindableProperty OnTertiaryProperty = BindableProperty.Create(
            nameof(OnTertiary),
            typeof(Color),
            typeof(Color),
            Color.FromArgb("#FFFFFF"),
            propertyChanged: OnOnTertiaryPropertyChanged);

        private static void OnOnTertiaryPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Color.OnTertiary, newValue);
        }

        public Color OnTertiary
        {
            get => (Color)this.GetValue(OnTertiaryProperty);
            set => this.SetValue(OnTertiaryProperty, value);
        }

        public static readonly BindableProperty PrimaryVariantProperty = BindableProperty.Create(
            nameof(PrimaryVariant),
            typeof(Color),
            typeof(Color),
            Color.FromArgb("#6200EE"),
            propertyChanged: OnPrimaryVariantPropertyChanged);

        private static void OnPrimaryVariantPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Color.PrimaryVariant, newValue);
        }

        /// <summary>
        ///     A tonal variation of <see cref="Primary" />.
        /// </summary>
        public Color PrimaryVariant
        {
            get => (Color)this.GetValue(PrimaryVariantProperty);
            set => this.SetValue(PrimaryVariantProperty, value);
        }

        public static readonly BindableProperty PrimaryDisabledProperty =
            BindableProperty.Create(
                nameof(PrimaryDisabled),
                typeof(Color),
                typeof(Color),
                MaterialColors.Gray500,
                propertyChanged: OnPrimaryDisabledPropertyChanged);

        private static void OnPrimaryDisabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Color.PrimaryDisabled, newValue);
        }

        public Color PrimaryDisabled
        {
            get => (Color)this.GetValue(PrimaryDisabledProperty);
            set => this.SetValue(PrimaryDisabledProperty, value);
        }

        /// <summary>
        ///     Accents select parts of your UI.
        ///     If not provided, use <see cref="Primary" />.
        /// </summary>
        public Color Secondary
        {
            get
            {
                var color = (Color)this.GetValue(SecondaryProperty);

                if (color.IsDefault() && this.Primary.IsDefault())
                {
                    return KnownColor.Accent;
                }

                return color.IsDefault() ? this.Primary : color;
            }
            set => this.SetValue(SecondaryProperty, value);
        }

        public static readonly BindableProperty SecondaryVariantProperty = BindableProperty.Create(
            nameof(SecondaryVariant),
            typeof(Color),
            typeof(Color),
            Color.FromArgb("#0400BA"),
            propertyChanged: OnSecondaryVariantPropertyChanged);

        private static void OnSecondaryVariantPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Color.SecondaryVariant, newValue);
        }

        /// <summary>
        ///     A tonal variation of <see cref="Secondary" />.
        /// </summary>
        public Color SecondaryVariant
        {
            get => (Color)this.GetValue(SecondaryVariantProperty);
            set => this.SetValue(SecondaryVariantProperty, value);
        }

        public static readonly BindableProperty SecondaryDisabledProperty = BindableProperty.Create(
            nameof(SecondaryDisabled),
            typeof(Color),
            typeof(Color),
            MaterialColors.Gray500,
            propertyChanged: OnSecondaryDisabledProperty);

        private static void OnSecondaryDisabledProperty(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Color.SecondaryDisabled, newValue);
        }

        public Color SecondaryDisabled
        {
            get => (Color)this.GetValue(SecondaryDisabledProperty);
            set => this.SetValue(SecondaryDisabledProperty, value);
        }

        public static readonly BindableProperty TertiaryProperty =
            BindableProperty.Create(
                nameof(Tertiary),
                typeof(Color),
                typeof(Color),
                MaterialColors.White,
                propertyChanged: OnTertiaryPropertyChanged);

        private static void OnTertiaryPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Color.Tertiary, newValue);
        }

        public Color Tertiary
        {
            get => (Color)this.GetValue(TertiaryProperty);
            set => this.SetValue(TertiaryProperty, value);
        }

        public static readonly BindableProperty TertiaryVariantProperty = BindableProperty.Create(
            nameof(TertiaryVariant),
            typeof(Color),
            typeof(Color),
            Color.FromArgb("#0400BA"),
            propertyChanged: OnTertiaryVariantPropertyChanged);

        private static void OnTertiaryVariantPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Color.TertiaryVariant, newValue);
        }

        /// <summary>
        ///     A tonal variation of <see cref="Tertiary" />.
        /// </summary>
        public Color TertiaryVariant
        {
            get => (Color)this.GetValue(TertiaryVariantProperty);
            set => this.SetValue(TertiaryVariantProperty, value);
        }

        public static readonly BindableProperty TertiaryDisabledProperty = BindableProperty.Create(
                nameof(TertiaryDisabled),
                typeof(Color),
                typeof(Color),
                MaterialColors.Gray500);

        public Color TertiaryDisabled
        {
            get => (Color)this.GetValue(TertiaryDisabledProperty);
            set => this.SetValue(TertiaryDisabledProperty, value);
        }

        #region Page

        public Color PageBackgroundColor
        {
            get => (Color)this.GetValue(PageBackgroundColorProperty);
            set => this.SetValue(PageBackgroundColorProperty, value);
        }

        public static readonly BindableProperty PageBackgroundColorProperty = BindableProperty.Create(
            nameof(PageBackgroundColor),
            typeof(Color),
            typeof(Color),
            MaterialColors.Gray100,
            propertyChanged: OnPageBackgroundColorPropertyChanged);

        private static void OnPageBackgroundColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateColorProperty(bindable, ThemeConstants.Page.BackgroundColor, newValue);
        }

        #endregion

        #region CardView

        public static readonly BindableProperty CardViewDividerColorProperty =
            BindableProperty.Create(
                nameof(CardViewDividerColor),
                typeof(Color),
                typeof(Color),
                GetDefaultCardViewDividerColor());

        private static Color GetDefaultCardViewDividerColor()
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                return Color.FromArgb("#ECECEC");
            }

            return Color.FromArgb("#C8C7CC");
        }

        public Color CardViewDividerColor
        {
            get => (Color)this.GetValue(CardViewDividerColorProperty);
            set => this.SetValue(CardViewDividerColorProperty, value);
        }

        public static readonly BindableProperty CardViewHeaderTextColorProperty =
            BindableProperty.Create(
                nameof(CardViewHeaderTextColor),
                typeof(Color),
                typeof(Color),
                GetDefaultCardViewHeaderTextColor());

        private static Color GetDefaultCardViewHeaderTextColor()
        {
            return Color.FromArgb("#6D6D72");
        }

        public Color CardViewHeaderTextColor
        {
            get => (Color)this.GetValue(CardViewHeaderTextColorProperty);
            set => this.SetValue(CardViewHeaderTextColorProperty, value);
        }

        public static readonly BindableProperty CardViewFooterTextColorProperty =
            BindableProperty.Create(
                nameof(CardViewFooterTextColor),
                typeof(Color),
                typeof(Color),
                GetDefaultCardViewFooterTextColor());

        private static Color GetDefaultCardViewFooterTextColor()
        {
            return Color.FromArgb("#6D6D72");
        }

        public Color CardViewFooterTextColor
        {
            get => (Color)this.GetValue(CardViewFooterTextColorProperty);
            set => this.SetValue(CardViewFooterTextColorProperty, value);
        }

        public static readonly BindableProperty CardViewHeaderBackgroundColorProperty =
            BindableProperty.Create(
                nameof(CardViewHeaderBackgroundColor),
                typeof(Color),
                typeof(Color),
                GetDefaultCardViewHeaderBackgroundColor());

        private static Color GetDefaultCardViewHeaderBackgroundColor()
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                return Colors.White;
            }

            if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                // TODO: Check if Color.Transparent is the better choice
                return Color.FromArgb("#EFEFF4");
            }

            return Color.FromArgb("#EFEFF4");
        }

        public Color CardViewHeaderBackgroundColor
        {
            get => (Color)this.GetValue(CardViewHeaderBackgroundColorProperty);
            set => this.SetValue(CardViewHeaderBackgroundColorProperty, value);
        }

        public static readonly BindableProperty CardViewBackgroundColorProperty = BindableProperty.Create(
            nameof(CardViewBackgroundColor),
            typeof(Color),
            typeof(Color),
            GetDefaultCardViewBackgroundColor());

        private static Color GetDefaultCardViewBackgroundColor()
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                return Color.FromArgb("#F5F5F5");
            }

            if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                return Colors.White;
            }

            return Colors.White;
        }

        /// <summary>
        ///     The color used as background inside the content area of a
        ///     <see cref="Superdev.Maui.Controls.CardView" />.
        /// </summary>
        public Color CardViewBackgroundColor
        {
            get => (Color)this.GetValue(CardViewBackgroundColorProperty);
            set => this.SetValue(CardViewBackgroundColorProperty, value);
        }

        #endregion
    }
}