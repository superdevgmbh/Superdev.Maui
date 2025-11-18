using System.Globalization;
using Microsoft.Maui.Controls.Internals;
using ValueConverters;

namespace SuperdevMauiDemoApp.Converters
{
    internal class FontElementToStringConverter : ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IFontElement fontElement)
            {
                var fontElementString = $"FontFamily: {fontElement.FontFamily ?? "null"}{Environment.NewLine}" +
                                        $"FontSize: {string.Format(CultureInfo.InvariantCulture, $"{fontElement.FontSize}")}{Environment.NewLine}" +
                                        $"FontAttributes: {fontElement.FontAttributes}";
                return fontElementString;
            }

            return null;
        }
    }
}