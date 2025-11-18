using System.Diagnostics;
using System.Globalization;
using Superdev.Maui.Resources.Styles;

namespace Superdev.Maui.Converters
{
    internal class PropertyErrorsToColorConverter : BindableObject, IValueConverter
    {
        private static Color NormalColor => (Color)Application.Current.Resources[ThemeConstants.Color.TextColorBright];

        private static Color ErrorColor => (Color)Application.Current.Resources[ThemeConstants.Color.Error];

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            Color color;
            var propertyErrors = value as IEnumerable<string>;
            if (propertyErrors != null && propertyErrors.Any())
            {
                color = ErrorColor;
            }
            else
            {
                color = NormalColor;
            }

            // Debug.WriteLine($"PropertyErrorsToColorConverter.Convert: color={color.ToHex()}");
            return color;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Convert back is not supported");
        }
    }
}