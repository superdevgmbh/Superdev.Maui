using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Superdev.Maui.Validation
{
    /// <summary>
    /// Derives the property name from the text of an accessor lambda, as <see cref="System.Runtime.CompilerServices.CallerArgumentExpressionAttribute"/>
    /// hands it over: <c>() => this.UserName</c>, <c>() => vm.UserName</c> and <c>() => UserName</c> all name
    /// <c>UserName</c>. Anything else — a nested path, a call, a cast — is rejected, because the field name has to be
    /// a plain property of the view model.
    /// </summary>
    internal static partial class PropertyNameParser
    {
        [GeneratedRegex(@"^\(\s*\)\s*=>\s*(?:[A-Za-z_][A-Za-z0-9_]*\s*\.\s*)?([A-Za-z_][A-Za-z0-9_]*)\s*$")]
        private static partial Regex AccessorRegex();

        public static bool TryParse(string? accessorExpression, [NotNullWhen(true)] out string? propertyName)
        {
            propertyName = null;

            if (string.IsNullOrWhiteSpace(accessorExpression))
            {
                return false;
            }

            var match = AccessorRegex().Match(accessorExpression.Trim());
            if (!match.Success)
            {
                return false;
            }

            propertyName = match.Groups[1].Value;
            return true;
        }

        public static string Parse(string? accessorExpression)
        {
            if (TryParse(accessorExpression, out var propertyName))
            {
                return propertyName;
            }

            throw new ArgumentException(
                $"The property name cannot be derived from the accessor '{accessorExpression}'. " +
                "Use a plain property access such as '() => this.UserName', or name the property explicitly: " +
                "Property(nameof(this.UserName), () => this.UserName).",
                nameof(accessorExpression));
        }
    }
}
