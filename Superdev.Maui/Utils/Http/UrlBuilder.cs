using System.Text.Encodings.Web;

namespace Superdev.Maui.Utils.Http
{
    /// <summary>
    ///     Allows constructing an absolute URL from a base URL, path segments, and query parameters.
    /// </summary>
    public class UrlBuilder
    {
        private readonly UriBuilder uriBuilder;

        /// <summary>
        ///     Initializes a new instance of <see cref="UrlBuilder" />.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        public UrlBuilder(string baseUrl)
            : this(new Uri(baseUrl))
        {
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="UrlBuilder" />.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        public UrlBuilder(Uri baseUrl)
        {
            ArgumentNullException.ThrowIfNull(baseUrl);

            this.uriBuilder = new UriBuilder(baseUrl);
        }

        /// <summary>
        ///     Gets the constructed URL.
        /// </summary>
        public Uri Url => this.uriBuilder.Uri;

        /// <summary>
        ///     Appends a path segment to the URL.
        /// </summary>
        /// <param name="path">The path segment to append.</param>
        /// <returns>The current <see cref="UrlBuilder" />.</returns>
        public UrlBuilder AddPath(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return this;
            }

            this.uriBuilder.Path = $"{this.uriBuilder.Path.TrimEnd('/', '\\')}/{path.TrimStart('/', '\\')}";
            return this;
        }

        /// <summary>
        ///     Appends a flag query parameter without a value.
        /// </summary>
        /// <param name="name">The query parameter name.</param>
        /// <returns>The current <see cref="UrlBuilder" />.</returns>
        public UrlBuilder AddQuery(string? name)
        {
            if (name == null)
            {
                return this;
            }

            return this.AppendQueryString(new QueryString($"?{UrlEncoder.Default.Encode(name)}"));
        }

        /// <summary>
        ///     Appends a query parameter.
        /// </summary>
        /// <param name="name">The query parameter name.</param>
        /// <param name="value">The query parameter value.</param>
        /// <returns>The current <see cref="UrlBuilder" />.</returns>
        public UrlBuilder AddQuery(string? name, string? value)
        {
            if (name == null || value == null)
            {
                return this;
            }

            return this.AppendQuery(name, value);
        }

        /// <summary>
        ///     Appends a query parameter.
        /// </summary>
        /// <param name="name">The query parameter name.</param>
        /// <param name="value">The query parameter value.</param>
        /// <returns>The current <see cref="UrlBuilder" />.</returns>
        public UrlBuilder AddQuery(string? name, object? value)
        {
            if (name == null || value == null)
            {
                return this;
            }

            var stringValue = value.ToString();
            if (stringValue == null)
            {
                return this;
            }

            return this.AppendQuery(name, stringValue);
        }

        /// <summary>
        ///     Appends a <see cref="DateTime" /> query parameter in UTC round-trip format.
        /// </summary>
        /// <param name="name">The query parameter name.</param>
        /// <param name="value">The query parameter value.</param>
        /// <returns>The current <see cref="UrlBuilder" />.</returns>
        public UrlBuilder AddQuery(string? name, DateTime value)
        {
            if (name == null)
            {
                return this;
            }

            return this.AppendQuery(name, value.ToUniversalTime().ToString("O"));
        }

        /// <summary>
        ///     Appends query parameters with repeated keys.
        /// </summary>
        /// <param name="name">The query parameter name.</param>
        /// <param name="values">The query parameter values.</param>
        /// <returns>The current <see cref="UrlBuilder" />.</returns>
        public UrlBuilder AddQuery(string? name, IEnumerable<string>? values)
        {
            if (name == null || values == null)
            {
                return this;
            }

            var queryBuilder = new QueryBuilder();
            foreach (var value in values)
            {
                if (value != null)
                {
                    queryBuilder.Add(name, value);
                }
            }

            return this.AppendQueryString(queryBuilder.ToQueryString());
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="UrlBuilder" /> from a string.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        public static implicit operator UrlBuilder(string baseUrl)
        {
            return new UrlBuilder(baseUrl);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Url.AbsoluteUri;
        }

        private UrlBuilder AppendQuery(string name, string value)
        {
            var queryBuilder = new QueryBuilder
            {
                { name, value },
            };

            return this.AppendQueryString(queryBuilder.ToQueryString());
        }

        private UrlBuilder AppendQueryString(QueryString queryString)
        {
            var existingQueryString = QueryString.FromUriComponent(this.uriBuilder.Uri);
            var combinedQueryString = existingQueryString.Add(queryString);
            this.uriBuilder.Query = combinedQueryString.ToUriComponent();

            return this;
        }
    }
}
