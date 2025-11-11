using System.Globalization;

namespace Superdev.Maui.Converters
{
    public class ColorAlphaConverter : BindableObject, IValueConverter
    {
        public static readonly BindableProperty AlphaProperty =
            BindableProperty.Create(
                nameof(Alpha),
                typeof(float),
                typeof(ColorAlphaConverter),
                1F);

        public float Alpha
        {
            get => (float)this.GetValue(AlphaProperty);
            set => this.SetValue(AlphaProperty, value);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return new Color(color.Red, color.Green, color.Blue, this.Alpha);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}