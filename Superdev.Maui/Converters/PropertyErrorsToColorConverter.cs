using System.Globalization;
using Superdev.Maui.Internals;
using Superdev.Maui.Resources.Styles;

namespace Superdev.Maui.Converters
{
    /// <summary>
    /// The underline colour of a validatable control: the theme's error colour while it has errors, its bright text
    /// colour otherwise. A missing theme key, or no application at all, yields the fallback instead of an exception
    /// inside the binding.
    /// </summary>
    internal class PropertyErrorsToColorConverter : BindableObject, IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is IEnumerable<string> propertyErrors && propertyErrors.Any()
                ? ResourceColors.Error(Colors.Red)
                : ResourceColors.Get(ThemeConstants.Color.TextColorBright);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Convert back is not supported");
        }
    }
}
