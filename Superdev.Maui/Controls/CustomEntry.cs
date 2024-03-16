namespace Superdev.Maui.Controls
{
    public class CustomEntry : Entry
    {
        public static new readonly BindableProperty BackgroundColorProperty =
        BindableProperty.Create(nameof(BackgroundColor),
            typeof(Color), typeof(CustomEntry), Colors.Transparent);
        // Gets or sets BorderColor value  
        public new Color BackgroundColor
        {
            get => (Color)this.GetValue(BackgroundColorProperty);
            set => this.SetValue(BackgroundColorProperty, value);
        }

        public static readonly BindableProperty BorderColorProperty =
        BindableProperty.Create(nameof(BorderColor),
            typeof(Color), typeof(CustomEntry), Colors.Transparent);
        // Gets or sets BorderColor value  
        public Color BorderColor
        {
            get => (Color)this.GetValue(BorderColorProperty);
            set => this.SetValue(BorderColorProperty, value);
        }


        public static readonly BindableProperty BorderWidthProperty =
        BindableProperty.Create(nameof(BorderWidth), typeof(int),
            typeof(CustomEntry), 1);

        // Gets or sets BorderWidth value  
        public int BorderWidth
        {
            get => (int)this.GetValue(BorderWidthProperty);
            set => this.SetValue(BorderWidthProperty, value);
        }

        public static readonly BindableProperty PaddingProperty =
        BindableProperty.Create(nameof(Padding),
            typeof(Thickness), typeof(CustomEntry), new Thickness(8, 2));

        // Gets or sets Padding value  
        public Thickness Padding
        {
            get => (Thickness)this.GetValue(PaddingProperty);
            set => this.SetValue(PaddingProperty, value);
        }

        public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(nameof(CornerRadius),
            typeof(double), typeof(CustomEntry), 6.0);


        // Gets or sets CornerRadius value  
        public double CornerRadius
        {
            get => (double)this.GetValue(CornerRadiusProperty);
            set => this.SetValue(CornerRadiusProperty, value);
        }
    }
}