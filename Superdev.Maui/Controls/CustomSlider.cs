namespace Superdev.Maui.Controls
{
    public class CustomSlider : Slider
    {
        public static readonly BindableProperty ThumbSizeProperty =
            BindableProperty.Create(
                nameof(ThumbSize),
                typeof(int?),
                typeof(CustomSlider),
                null);

        public int? ThumbSize
        {
            get => (int?)this.GetValue(ThumbSizeProperty);
            set => this.SetValue(ThumbSizeProperty, value);
        }
    }
}