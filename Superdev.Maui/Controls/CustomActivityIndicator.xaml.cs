using Superdev.Maui.Styles;

namespace Superdev.Maui.Controls
{
    public partial class CustomActivityIndicator : ContentView
    {
        public CustomActivityIndicator()
        {
            this.InitializeComponent();
            ((VisualElement)this.Control).BackgroundColor = Colors.Transparent;

            // Hack: OnPlatform lacks of support for DynamicResource bindings!
            if (Device.RuntimePlatform == Device.Android)
            {
                this.ActivityIndicator.SetDynamicResource(ActivityIndicator.ColorProperty, ThemeConstants.Color.Secondary);
            }
        }

        public static readonly BindableProperty CaptionProperty =
            BindableProperty.Create(
                nameof(Caption),
                typeof(string),
                typeof(CustomActivityIndicator),
                null);

        public string Caption
        {
            get => (string)this.GetValue(CaptionProperty);
            set => this.SetValue(CaptionProperty, value);
        }

        public static readonly BindableProperty LabelStyleProperty =
            BindableProperty.Create(
                nameof(LabelStyle),
                typeof(Style),
                typeof(CustomActivityIndicator),
                default(Style),
                BindingMode.OneWay);

        public Style LabelStyle
        {
            get => (Style)this.GetValue(LabelStyleProperty);
            set => this.SetValue(LabelStyleProperty, value);
        }

        public new static readonly BindableProperty BackgroundColorProperty =
            BindableProperty.Create(
                nameof(BackgroundColor),
                typeof(Color),
                typeof(CustomActivityIndicator),
                Colors.Black,
                BindingMode.OneWay);

        public new Color BackgroundColor
        {
            get => (Color)this.GetValue(BackgroundColorProperty);
            set => this.SetValue(BackgroundColorProperty, value);
        }

        public static readonly BindableProperty CornerRadiusProperty =
            BindableProperty.Create(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(CustomActivityIndicator),
                new CornerRadius(),
                BindingMode.OneWay);

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)this.GetValue(CornerRadiusProperty);
            set => this.SetValue(CornerRadiusProperty, value);
        }
    }
}