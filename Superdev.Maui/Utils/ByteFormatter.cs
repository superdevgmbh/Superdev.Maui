using System.Globalization;

namespace Superdev.Maui.Utils
{
    /// <summary>
    /// Source:
    /// https://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc/22733709
    /// </summary>
    public static class ByteFormatter
    {
        private static readonly string[] SizeSuffixes =
        {
            "bytes",
            "KB",
            "MB",
            "GB",
            "TB",
            "PB",
            "EB",
            "ZB",
            "YB"
        };

        public static string GetNamedSize(long byteLength, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces));
            }

            if (byteLength < 0)
            {
                // The magnitude is taken as an unsigned value because long.MinValue has no positive
                // counterpart: negating it as a long yields long.MinValue again, which used to recurse
                // here until the stack ran out.
                var magnitude = (ulong)(-(byteLength + 1)) + 1;
                return "-" + FormatMagnitude(magnitude, decimalPlaces);
            }

            return FormatMagnitude((ulong)byteLength, decimalPlaces);
        }

        private static string FormatMagnitude(ulong byteLength, int decimalPlaces)
        {
            if (byteLength == 0)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:n" + decimalPlaces + "} bytes", 0);
            }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            var mag = (int)Math.Log(byteLength, 1024);

            // 1UL << (mag * 10) == 2 ^ (10 * mag)
            // [i.e. the number of bytes in the unit corresponding to mag]
            var adjustedSize = (decimal)byteLength / (1UL << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format(CultureInfo.InvariantCulture, "{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
        }
    }
}