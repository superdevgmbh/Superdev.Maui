using System.Collections;
using System.Globalization;

namespace Superdev.Maui.Converters
{
    public class EnumerableToFirstOrDefaultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable enumerable)
            {
                value = enumerable.GetEnumerator();
            }

            if (value is IEnumerator enumerator)
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Convert back is not supported");
        }
    }
}