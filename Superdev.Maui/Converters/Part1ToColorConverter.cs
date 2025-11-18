using System;
using System.Globalization;
namespace Superdev.Maui.Converters
{
    internal class Part1ToColorConverter : BoolToColorConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusSegment statusSegment)
            {
                if (statusSegment.IsFirstElement)
                {
                    return Colors.Transparent;
                }
                else if (statusSegment.IsMiddleElement || (statusSegment.IsEndElement && !statusSegment.IsStartElement))
                {
                    return this.TrueValue;
                }

                return this.FalseValue;
            }

            return Colors.Transparent;
        }
    }

    internal class Part2ToColorConverter : BoolToColorConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusSegment statusSegment)
            {
                if (statusSegment.IsStartElement || statusSegment.IsMiddleElement || statusSegment.IsEndElement)
                {
                    return this.TrueValue;
                }

                return this.FalseValue;
            }

            return Colors.Transparent;
        }
    }

    internal class Part3ToColorConverter : BoolToColorConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusSegment statusSegment)
            {
                if (statusSegment.IsLastElement)
                {
                    return Colors.Transparent;
                }
                else if ((statusSegment.IsStartElement && !statusSegment.IsEndElement) || statusSegment.IsMiddleElement)
                {
                    return this.TrueValue;
                }

                return this.FalseValue;
            }

            return Colors.Transparent;
        }
    }
}