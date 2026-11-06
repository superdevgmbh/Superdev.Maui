using System.Globalization;
using Superdev.Maui.Mvvm;

namespace Superdev.Maui.Converters
{
    public class ViewModelErrorIsVisibleConverter : BindableObject, IValueConverter
    {
        public static readonly BindableProperty IsInvertedProperty = BindableProperty.Create(
            nameof(IsInverted),
            typeof(bool),
            typeof(ViewModelErrorIsVisibleConverter),
            false);

        public bool IsInverted
        {
            get => (bool)this.GetValue(IsInvertedProperty);
            set => this.SetValue(IsInvertedProperty, value);
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // The inversion is applied to the whole result, not just to the error comparison: exclusive-or
            // binds tighter than the logical and, so inlining it here would leave a non-error value visible.
            var hasError = value is ViewModelError viewModelError && viewModelError != ViewModelError.None;
            return hasError ^ this.IsInverted;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Convert back is not supported");
        }
    }
}