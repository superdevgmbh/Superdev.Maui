using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Superdev.Maui.Extensions
{
    public static class StringExtensions
    {
        private const string HttpPrefix = "http://";

        public static readonly char[] TrimNewLineChars = "\r\n".ToCharArray();
        public static readonly char[] TrimChars = "\r\n ".ToCharArray();

        /// <summary>
        ///     To the unique identifier.
        /// </summary>
        /// <returns>Guid.</returns>
        ////public static Guid ToGuid(this string src)
        ////{
        ////    byte[] stringbytes = Encoding.UTF8.GetBytes(src);
        ////    byte[] hashedBytes = new System.Security.Cryptography.SHA1Managed().ComputeHash(stringbytes);
        ////    Array.Resize(ref hashedBytes, 16);
        ////    return new Guid(hashedBytes);
        ////}
        public static bool Like(this string? source, string toFind)
        {
            ArgumentNullException.ThrowIfNull(source);

            return new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\")
                    .Replace(toFind, ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline)
                .IsMatch(source);
        }

        /// <summary>Returns a value indicating whether a specified substring <paramref name="value"/> occurs within the source string <paramref name="source"/>.</summary>
        /// <param name="source">The source string.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>true if the <paramref name="value">value</paramref> parameter occurs within this string, or if <paramref name="value">value</paramref> is the empty string (""); otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="value">value</paramref> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="comparisonType">comparisonType</paramref> is not a valid <see cref="T:System.StringComparison"></see> value.</exception>
        public static bool Contains(this string? source, string value, StringComparison comparisonType)
        {
            return source?.IndexOf(value, comparisonType) >= 0;
        }

        /// <summary>
        /// Checks if any of the given strings <paramref name="strings"/> is contained in source string <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="strings">The enumeration of strings to be compared against the source string.</param>
        public static bool ContainsAny(this string? source, IEnumerable<string> strings)
        {
            if (source == null)
            {
                return false;
            }

            return strings.Any(source.Contains);
        }

        /// <summary>
        /// Checks if any of the given strings <paramref name="strings"/> is contained in source string <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="strings">The enumeration of strings to be compared against the source string.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        public static bool ContainsAny(this string? source, IEnumerable<string> strings, StringComparison comparisonType)
        {
            if (source == null)
            {
                return false;
            }

            return strings.Any(s => source.Contains(s, comparisonType));
        }

        /// <summary>
        /// Determines whether any of the given <paramref name="values"/> is a prefix of <paramref name="source"/>
        /// </summary>
        public static bool StartsWithAny(this string? source, IEnumerable<string> values)
        {
            ArgumentNullException.ThrowIfNull(values);

            if (source == null)
            {
                return false;
            }

            return values.Any(source.StartsWith);
        }

        /// <summary>
        /// Determines whether any of the given <paramref name="values"/> is a prefix of <paramref name="source"/>
        /// </summary>
        public static bool StartsWithAny(this string? source, IEnumerable<string> values, StringComparison comparisonType)
        {
            ArgumentNullException.ThrowIfNull(values);

            if (source == null)
            {
                return false;
            }

            return values.Any(s => source.StartsWith(s, comparisonType));
        }

        /// <summary>
        /// Converts the first character of <paramref name="source"/> to upper case.
        /// </summary>
        [return: NotNullIfNotNull(nameof(source))]
        public static string? ToUpperFirst(this string? source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            var a = source.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        /// <summary>
        ///     Removes all leading and trailing occurrences of new line (\n\r) as well as white-space characters in an array from
        ///     the current <see cref="T:System.String"></see> object.
        /// </summary>
        /// <param name="source">Input string.</param>
        /// <returns>
        ///     The string that remains after all occurrences of trim characters are removed from the start and end of the current
        ///     string.
        /// </returns>
        [return: NotNullIfNotNull(nameof(source))]
        public static string? TrimStartAndEnd(this string? source)
        {
            return source?.TrimStartAndEnd(TrimChars);
        }

        /// <summary>
        ///     Removes all leading and trailing occurrences of a set of characters specified in an array from the current
        ///     <see cref="T:System.String"></see> object.
        /// </summary>
        /// <param name="source">Input string.</param>
        /// <param name="trimChars">An array of Unicode characters to remove, or null.</param>
        /// <returns>
        ///     The string that remains after all occurrences of characters in the <paramref name="trimChars">trimChars</paramref>
        ///     parameter are removed from the start and end of the current string.
        ///     If <paramref name="trimChars">trimChars</paramref> is null or an empty array, white-space characters are removed
        ///     instead.
        /// </returns>
        [return: NotNullIfNotNull(nameof(source))]
        public static string? TrimStartAndEnd(this string? source, params char[] trimChars)
        {
            return source?.TrimStart(trimChars)
                .TrimEnd(trimChars);
        }

        [return: NotNullIfNotNull(nameof(source))]
        public static string? RemoveEmptyLines(this string? source)
        {
            if (source == null)
            {
                return null;
            }

            var lines = source.Split(TrimNewLineChars, StringSplitOptions.RemoveEmptyEntries);

            var stringBuilder = new StringBuilder(source.Length);

            foreach (var line in lines)
            {
                stringBuilder.AppendLine(line);
            }

            return stringBuilder.ToString().TrimEnd(TrimNewLineChars);
        }

        /// <summary>
        ///     Catch runs of any kind of whitespace (e.g. tabs, newlines, etc.) and replace them with a single space.
        /// </summary>
        [return: NotNullIfNotNull(nameof(source))]
        public static string? TrimWhitespaces(this string? source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            return Regex.Replace(source, @"\s+", " ").Trim();
        }


        /// <summary>
        /// Truncates a string to the specified maximum length.
        /// </summary>
        /// <param name="source">The string to truncate, or <see langword="null" /> to return <see langword="null" />.</param>
        /// <param name="maxLength">The maximum length of the returned string, including the truncation indicator.</param>
        /// <param name="truncationIndicator">The optional string to append when the input is truncated.</param>
        /// <returns>The original string when it fits within the maximum length; otherwise, a truncated string.</returns>
        [return: NotNullIfNotNull(nameof(source))]
        internal static string? Truncate(this string? source, int maxLength, string? truncationIndicator = null)
        {
            if (source is null)
            {
                return null;
            }

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength);

            if (source.Length <= maxLength)
            {
                return source;
            }

            if (truncationIndicator != null && maxLength <= truncationIndicator.Length)
            {
                return truncationIndicator[..maxLength];
            }

            var truncationIndicatorLength = truncationIndicator?.Length ?? 0;
            return $"{source[..(maxLength - truncationIndicatorLength)]}{truncationIndicator}";
        }
    }
}