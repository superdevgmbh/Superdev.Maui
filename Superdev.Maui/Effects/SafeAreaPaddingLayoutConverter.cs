using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Layouts;
using static Superdev.Maui.Effects.SafeAreaPaddingLayout;

namespace Superdev.Maui.Effects
{
    /// <inheritdoc/>
	public class SafeAreaPaddingLayoutConverter : TypeConverter
    {
        private static readonly char[] CommaSeparator = new char[1] { ',' };

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => sourceType == typeof(string);

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => destinationType == typeof(string);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var strValue = value?.ToString();

            if (value == null)
            {
                throw new InvalidOperationException(string.Format("Cannot convert null into {0}", typeof(SafeAreaPaddingLayout)));
            }

            var strArray = strValue.Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length < 1 || strArray.Length > 4)
            {
                throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(SafeAreaPaddingLayout)));
            }

            var paddingPositions = strArray.Select(s => (PaddingPosition)Enum.Parse(typeof(PaddingPosition), s, true)).ToArray();
            return new SafeAreaPaddingLayout(paddingPositions);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is not SafeAreaPaddingLayout t)
            {
                throw new NotSupportedException();
            }

            return string.Join(",", t.Positions.Select(p => p.ToString()));
        }
    }
}
