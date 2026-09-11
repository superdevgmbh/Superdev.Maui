using System.ComponentModel;
using System.Globalization;
using static Superdev.Maui.Effects.SafeAreaPaddingLayout;

namespace Superdev.Maui.Effects
{
    /// <inheritdoc/>
    public class SafeAreaPaddingLayoutConverter : TypeConverter
    {
        private static readonly char[] CommaSeparator = new[] { ',' };

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            var strValue = value.ToString();

            var strArray = strValue?.Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (strArray == null || strArray.Length < 1 || strArray.Length >4)
            {
                throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(SafeAreaPaddingLayout)}");
            }

            var paddingPositions = strArray.Select(s => (PaddingPosition)Enum.Parse(typeof(PaddingPosition), s, true)).ToArray();
            return new SafeAreaPaddingLayout(paddingPositions);
        }

        public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is not SafeAreaPaddingLayout t)
            {
                throw new NotSupportedException();
            }

            return string.Join(",", t.Positions.Select(p => p.ToString()));
        }
    }
}