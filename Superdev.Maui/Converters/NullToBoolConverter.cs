using System.Globalization;

namespace Superdev.Maui.Converters
{
    internal class NullToBoolConverter : IValueConverter
    {
        public bool Inverted { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (value == null) ^ this.Inverted;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Convert back is not supported");
        }
    }
}