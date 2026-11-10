using System.Globalization;
using System.Text.RegularExpressions;

namespace Superdev.Maui.Validation
{
    /// <summary>
    /// Fills <c>{Placeholder}</c>s in a message template from the error's arguments. A placeholder without an
    /// argument is left as it is, so a wrong template is visible rather than silently blank.
    /// </summary>
    internal static partial class ValidationMessageFormatter
    {
        [GeneratedRegex(@"\{(\w+)\}")]
        private static partial Regex PlaceholderRegex();

        public static string Format(string template, IReadOnlyDictionary<string, object?> arguments)
        {
            ArgumentNullException.ThrowIfNull(template);
            ArgumentNullException.ThrowIfNull(arguments);

            if (arguments.Count == 0)
            {
                return template;
            }

            return PlaceholderRegex().Replace(template, match =>
            {
                var name = match.Groups[1].Value;
                return arguments.TryGetValue(name, out var value)
                    ? Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty
                    : match.Value;
            });
        }
    }
}
