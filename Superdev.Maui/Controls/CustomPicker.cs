namespace Superdev.Maui.Controls
{
    public class CustomPicker : Picker
    {
        public static readonly BindableProperty PlaceholderColorProperty =
            BindableProperty.Create(
                nameof(PlaceholderColor),
                typeof(Color),
                typeof(CustomPicker));

        public Color PlaceholderColor
        {
            get => (Color)this.GetValue(PlaceholderColorProperty);
            set => this.SetValue(PlaceholderColorProperty, value);
        }
    }
}