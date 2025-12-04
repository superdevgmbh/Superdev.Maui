using System.Collections;
using System.Globalization;

namespace Superdev.Maui.Converters
{
    public class IsFirstItemToBoolConverter : IMultiValueConverter
    {
        public object Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values is [object items, object item])
            {
                if (items is IEnumerable enumerable)
                {
                    items = enumerable.GetEnumerator();
                }

                if (items is IEnumerator enumerator)
                {
                    if (enumerator.MoveNext())
                    {
                        var first = enumerator.Current;

                        if (Equals(first, item))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Convert back is not supported");
        }
    }
}