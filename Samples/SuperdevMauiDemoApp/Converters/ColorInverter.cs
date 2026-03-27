using System.Globalization;
using Superdev.Maui.Extensions;
using ValueConverters;

namespace SuperdevMauiDemoApp.Converters
{
    internal class ColorInverter : ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                var invertedColor = color.Invert();
                return invertedColor;
            }

            return null;
        }
    }
}