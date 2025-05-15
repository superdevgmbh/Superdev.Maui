namespace Superdev.Maui.Controls
{
    public class CustomButton : Button
    {
        public static readonly BindableProperty BackgroundColorPressedProperty =
            BindableProperty.Create(
                nameof(BackgroundColorPressed),
                typeof(Color),
                typeof(CustomButton),
                Colors.White);

        public Color BackgroundColorPressed
        {
            get => (Color)this.GetValue(BackgroundColorPressedProperty);
            set => this.SetValue(BackgroundColorPressedProperty, value);
        }

        public static readonly BindableProperty BorderColorPressedProperty = BindableProperty.Create(
                nameof(BorderColorPressed),
                typeof(Color),
                typeof(CustomButton),
                Colors.White);

        public Color BorderColorPressed
        {
            get => (Color)this.GetValue(BorderColorPressedProperty);
            set => this.SetValue(BorderColorPressedProperty, value);
        }

        public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.Create(
            nameof(VerticalTextAlignment),
            typeof(TextAlignment),
            typeof(CustomButton),
            TextAlignment.Center);

        public TextAlignment VerticalTextAlignment
        {
            get => (TextAlignment)this.GetValue(VerticalTextAlignmentProperty);
            set => this.SetValue(VerticalTextAlignmentProperty, value);
        }

        public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create(
            nameof(HorizontalTextAlignment),
            typeof(TextAlignment),
            typeof(CustomButton),
            TextAlignment.Center);

        public TextAlignment HorizontalTextAlignment
        {
            get => (TextAlignment)this.GetValue(HorizontalTextAlignmentProperty);
            set => this.SetValue(HorizontalTextAlignmentProperty, value);
        }

        public static readonly BindableProperty AllCapsProperty = BindableProperty.Create(
                nameof(AllCaps),
                typeof(bool),
                typeof(CustomButton),
                false);

        public bool AllCaps
        {
            get => (bool)this.GetValue(AllCapsProperty);
            set => this.SetValue(AllCapsProperty, value);
        }

        public static readonly BindableProperty ElevationProperty =
            BindableProperty.Create(
                nameof(Elevation),
                typeof(float),
                typeof(CustomButton),
                4.0f);

        public float Elevation
        {
            get => (float)this.GetValue(ElevationProperty);
            set => this.SetValue(ElevationProperty, value);
        }
    }
}