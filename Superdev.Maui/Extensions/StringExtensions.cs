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
        ///     To the URI.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Uri.</returns>
        public static Uri ToUri(this string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            if (url.ToLower().StartsWith(HttpPrefix))
            {
                return new Uri(url);
            }

            return new Uri($"{HttpPrefix}{url}");
        }

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

        public static bool Like(this string toSearch, string toFind)
        {
            return
                new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(toFind, ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline).IsMatch(
                    toSearch);
        }

        /// <summary>Returns a value indicating whether a specified substring <paramref name="value"/> occurs within the source string <paramref name="source"/>.</summary>
        /// <param name="source">The source string.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>true if the <paramref name="value">value</paramref> parameter occurs within this string, or if <paramref name="value">value</paramref> is the empty string (""); otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="value">value</paramref> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="comparisonType">comparisonType</paramref> is not a valid <see cref="T:System.StringComparison"></see> value.</exception>
        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source?.IndexOf(value, comparisonType) >= 0;
        }

        /// <summary>
        /// Checks if any of the given strings <paramref name="strings"/> is contained in source string <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="strings">The enumeration of strings to be compared against the source string.</param>
        public static bool ContainsAny(this string source, IEnumerable<string> strings)
        {
            return strings.Any(source.Contains);
        }

        /// <summary>
        /// Checks if any of the given strings <paramref name="strings"/> is contained in source string <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="strings">The enumeration of strings to be compared against the source string.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        public static bool ContainsAny(this string source, IEnumerable<string> strings, StringComparison comparisonType)
        {
            return strings.Any(s => source.Contains(s, comparisonType));
        }

        public static bool StartsWithAny(this string source, IEnumerable<string> strings)
        {
            return strings.Any(source.StartsWith);
        }

        /// <summary>
        /// Converts the first character of <paramref name="s"/> to upper case.
        /// </summary>
        public static string ToUpperFirst(this string s)
        {
            if (s == null)
            {
                return null;
            }

            if (s == "")
            {
                return "";
            }

            var a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        /// <summary>
        ///     Removes all leading and trailing occurrences of new line (\n\r) as well as white-space characters in an array from
        ///     the current <see cref="T:System.String"></see> object.
        /// </summary>
        /// <param name="str">Input string.</param>
        /// <returns>
        ///     The string that remains after all occurrences of trim characters are removed from the start and end of the current
        ///     string.
        /// </returns>
        public static string TrimStartAndEnd(this string str)
        {
            return str.TrimStartAndEnd(TrimChars);
        }

        /// <summary>
        ///     Removes all leading and trailing occurrences of a set of characters specified in an array from the current
        ///     <see cref="T:System.String"></see> object.
        /// </summary>
        /// <param name="str">Input string.</param>
        /// <param name="trimChars">An array of Unicode characters to remove, or null.</param>
        /// <returns>
        ///     The string that remains after all occurrences of characters in the <paramref name="trimChars">trimChars</paramref>
        ///     parameter are removed from the start and end of the current string.
        ///     If <paramref name="trimChars">trimChars</paramref> is null or an empty array, white-space characters are removed
        ///     instead.
        /// </returns>
        public static string TrimStartAndEnd(this string str, params char[] trimChars)
        {
            if (str == null)
            {
                return null;
            }

            return str
                .TrimStart(trimChars)
                .TrimEnd(trimChars);
        }

        public static string RemoveEmptyLines(this string str)
        {
            if (str == null)
            {
                return null;
            }

            var lines = str.Split(TrimNewLineChars, StringSplitOptions.RemoveEmptyEntries);

            var stringBuilder = new StringBuilder(str.Length);

            foreach (var line in lines)
            {
                stringBuilder.AppendLine(line);
            }

            return stringBuilder.ToString().TrimEnd(TrimNewLineChars);
        }

        /// <summary>
        ///     Catch runs of any kind of whitespace (e.g. tabs, newlines, etc.) and replace them with a single space.
        /// </summary>
        public static string TrimWhitespaces(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return Regex.Replace(input, @"\s+", " ").Trim();
        }
    }
}