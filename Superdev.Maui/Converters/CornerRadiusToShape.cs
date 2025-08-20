using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace Superdev.Maui.Converters
{
    class CornerRadiusToShape : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new RoundRectangle
            {
                CornerRadius = (int)value,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}