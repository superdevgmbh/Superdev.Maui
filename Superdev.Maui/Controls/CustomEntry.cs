namespace Superdev.Maui.Controls
{
    public class CustomEntry : Entry
    {
        public static new readonly BindableProperty BackgroundColorProperty =
            BindableProperty.Create(nameof(BackgroundColor),
                typeof(Color),
                typeof(CustomEntry),
                Colors.Transparent);

        public new Color BackgroundColor
        {
            get => (Color)this.GetValue(BackgroundColorProperty);
            set => this.SetValue(BackgroundColorProperty, value);
        }

        public static readonly BindableProperty BorderColorProperty =
            BindableProperty.Create(nameof(BorderColor),
                typeof(Color),
                typeof(CustomEntry),
                Colors.Transparent);

        public Color BorderColor
        {
            get => (Color)this.GetValue(BorderColorProperty);
            set => this.SetValue(BorderColorProperty, value);
        }

        public static readonly BindableProperty BorderWidthProperty =
            BindableProperty.Create(
                nameof(BorderWidth),
                typeof(int),
                typeof(CustomEntry),
                1);

        public int BorderWidth
        {
            get => (int)this.GetValue(BorderWidthProperty);
            set => this.SetValue(BorderWidthProperty, value);
        }

        public static readonly BindableProperty PaddingProperty =
            BindableProperty.Create(
                nameof(Padding),
                typeof(Thickness),
                typeof(CustomEntry),
                Thickness.Zero);

        public Thickness Padding
        {
            get => (Thickness)this.GetValue(PaddingProperty);
            set => this.SetValue(PaddingProperty, value);
        }

        public static BindableProperty CornerRadiusProperty =
             BindableProperty.Create(
                 nameof(CornerRadius),
                 typeof(int),
                 typeof(CustomEntry),
                 0);

        public int CornerRadius
        {
            get => (int)this.GetValue(CornerRadiusProperty);
            set => this.SetValue(CornerRadiusProperty, value);
        }

        public static readonly BindableProperty TextContentTypeProperty =
            BindableProperty.Create(
                nameof(TextContentType),
                typeof(TextContentType),
                typeof(CustomEntry),
                TextContentType.Default,
                BindingMode.OneWay);

        public TextContentType TextContentType
        {
            get => (TextContentType)this.GetValue(TextContentTypeProperty);
            set => this.SetValue(TextContentTypeProperty, value);
        }
    }
}