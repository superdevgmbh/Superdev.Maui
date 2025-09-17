using System.Diagnostics;

namespace Superdev.Maui.Resources.Styles
{
    /// <summary>
    ///     Class that provides the theme configuration that will be applied in the current App.
    /// </summary>
    public class Theme : BindableObject, ITheme
    {
        public static readonly BindableProperty ColorConfigurationProperty =
            BindableProperty.Create(
                nameof(ColorConfiguration),
                typeof(ColorConfiguration),
                typeof(Theme),
                null,
                propertyChanged: OnColorConfigurationPropertyChanged);

        private static void OnColorConfigurationPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not Theme theme)
            {
                return;
            }

            Debug.WriteLine("OnColorConfigurationPropertyChanged");
        }

        /// <summary>
        ///     Gets or sets the color configuration of the theme.
        /// </summary>
        public ColorConfiguration ColorConfiguration
        {
            get => (ColorConfiguration)this.GetValue(ColorConfigurationProperty);
            set => this.SetValue(ColorConfigurationProperty, value);
        }

        public static readonly BindableProperty SpacingConfigurationProperty =
            BindableProperty.Create(
                nameof(SpacingConfiguration),
                typeof(SpacingConfiguration),
                typeof(Theme),
                null);

        /// <summary>
        ///     Gets or sets the spacing configuration of the theme.
        /// </summary>
        public SpacingConfiguration SpacingConfiguration
        {
            get => (SpacingConfiguration)this.GetValue(SpacingConfigurationProperty);
            set => this.SetValue(SpacingConfigurationProperty, value);
        }

        public static readonly BindableProperty FontConfigurationProperty =
            BindableProperty.Create(
                nameof(FontConfiguration),
                typeof(FontConfiguration),
                typeof(Theme),
                null);

        // TODO: Dispose oldValue (if is not null)

        /// <summary>
        ///     Gets or sets the font configuration of the theme.
        /// </summary>
        public FontConfiguration FontConfiguration
        {
            get => (FontConfiguration)this.GetValue(FontConfigurationProperty);
            set => this.SetValue(FontConfigurationProperty, value);
        }
    }
}