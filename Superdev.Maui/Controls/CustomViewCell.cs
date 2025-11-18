namespace Superdev.Maui.Controls
{
    public class CustomViewCell : ViewCell
    {
        public static readonly BindableProperty SelectedBackgroundColorProperty = BindableProperty.Create(
            nameof(SelectedBackgroundColor),
            typeof(Color),
            typeof(CustomViewCell),
            null);

        public Color SelectedBackgroundColor
        {
            get => (Color)this.GetValue(SelectedBackgroundColorProperty);
            set => this.SetValue(SelectedBackgroundColorProperty, value);
        }
    }
}