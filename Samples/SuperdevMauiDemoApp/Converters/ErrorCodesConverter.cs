using System.Globalization;
using Superdev.Maui.Validation;
using ValueConverters;

namespace SuperdevMauiDemoApp.Converters
{
    /// <summary>
    /// The error codes of a field, comma-separated, for the demo's state inspector.
    /// </summary>
    internal class ErrorCodesConverter : ConverterBase
    {
        protected override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is IEnumerable<ValidationError> errors ? string.Join(", ", errors.Select(e => e.ErrorCode)) : null;
        }
    }
}
