using System.Collections;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// The parameters of a navigation URI query string, e.g. of a deep link such as
    /// <c>"/PatentDetailPage?id=42"</c>.
    /// </summary>
    /// <remarks>
    /// Query parameters are delivered through the regular navigation parameter pipeline:
    /// implement <see cref="IInitialize{TParameter}"/> or <see cref="INavigatedTo{TParameter}"/>
    /// with <see cref="NavigationQueryParameters"/> as the type parameter in order to receive them.
    /// </remarks>
    public sealed class NavigationQueryParameters : IReadOnlyDictionary<string, string>
    {
        private readonly IReadOnlyDictionary<string, string> parameters;

        /// <inheritdoc cref="NavigationQueryParameters"/>
        public NavigationQueryParameters(IReadOnlyDictionary<string, string> parameters)
        {
            this.parameters = parameters;
        }

        /// <inheritdoc />
        public IEnumerable<string> Keys => this.parameters.Keys;

        /// <inheritdoc />
        public IEnumerable<string> Values => this.parameters.Values;

        /// <inheritdoc />
        public int Count => this.parameters.Count;

        /// <inheritdoc />
        public string this[string key] => this.parameters[key];

        /// <summary>
        /// Gets the value of the given <paramref name="key"/>, converted to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type the value is parsed into, e.g. <see cref="int"/> or <see cref="Guid"/>.</typeparam>
        /// <param name="key">The name of the query parameter.</param>
        /// <param name="value">The parsed value, if the parameter exists and could be parsed.</param>
        /// <returns><c>true</c> if the value was parsed; otherwise, <c>false</c>.</returns>
        public bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value) where T : IParsable<T>
        {
            if (this.parameters.TryGetValue(key, out var rawValue) &&
                T.TryParse(rawValue, CultureInfo.InvariantCulture, out var parsedValue))
            {
                value = parsedValue;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Gets the value of the given <paramref name="key"/> as a member of the enumeration
        /// <typeparamref name="TEnum"/>, by its name - case-insensitively - or by its numeric value.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <param name="key">The name of the query parameter.</param>
        /// <param name="value">The member, if the parameter exists and names a defined member.</param>
        /// <returns><c>true</c> if the value names a defined member; otherwise, <c>false</c>.</returns>
        public bool TryGetEnumValue<TEnum>(string key, out TEnum value) where TEnum : struct, Enum
        {
            // Enum.TryParse accepts any number, whether the enumeration defines it or not.
            if (this.parameters.TryGetValue(key, out var rawValue) &&
                Enum.TryParse(rawValue, ignoreCase: true, out value) &&
                Enum.IsDefined(value))
            {
                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return this.parameters.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        {
            return this.parameters.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join("&", this.parameters.Select(p => $"{p.Key}={p.Value}"));
        }

        /// <summary>
        /// Parses the query string of a navigation URI segment, e.g. <c>"id=42&amp;mode=edit"</c>.
        /// </summary>
        internal static NavigationQueryParameters Parse(string queryString)
        {
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in queryString.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var separatorIndex = pair.IndexOf('=');
                if (separatorIndex < 0)
                {
                    parameters[Uri.UnescapeDataString(pair)] = string.Empty;
                }
                else
                {
                    var key = Uri.UnescapeDataString(pair[..separatorIndex]);
                    var value = Uri.UnescapeDataString(pair[(separatorIndex + 1)..]);
                    parameters[key] = value;
                }
            }

            return new NavigationQueryParameters(parameters);
        }
    }
}
