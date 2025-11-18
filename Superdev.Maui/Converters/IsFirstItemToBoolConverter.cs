using System.Collections;
using System.Globalization;

namespace Superdev.Maui.Converters
{
    public class IsFirstItemToBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length == 2 &&
                values[0] is object items &&
                values[1] is object item)
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Convert back is not supported");
        }
    }
}