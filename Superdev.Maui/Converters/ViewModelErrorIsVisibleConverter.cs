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
            return value is ViewModelError viewModelError && (viewModelError != ViewModelError.None) ^ this.IsInverted;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Convert back is not supported");
        }
    }
}