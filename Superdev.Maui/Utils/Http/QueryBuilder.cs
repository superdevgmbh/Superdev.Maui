// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Source: https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http.Extensions/src/QueryBuilder.cs

using System.Collections;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Primitives;
using Superdev.Maui.Internals;

namespace Superdev.Maui.Utils.Http
{
    // The IEnumerable interface is required for the collection initialization syntax: new QueryBuilder() { { "key", "value" } };
    /// <summary>
    ///     Allows constructing a query string.
    /// </summary>
    public class QueryBuilder : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly IList<KeyValuePair<string, string>> parameters;

        /// <summary>
        ///     Initializes a new instance of <see cref="QueryBuilder" />.
        /// </summary>
        public QueryBuilder()
        {
            this.parameters = new List<KeyValuePair<string, string>>();
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="QueryBuilder" />.
        /// </summary>
        /// <param name="parameters">The parameters to initialize the instance with.</param>
        public QueryBuilder(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            this.parameters = new List<KeyValuePair<string, string>>(parameters);
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="QueryBuilder" />.
        /// </summary>
        /// <param name="parameters">The parameters to initialize the instance with.</param>
        public QueryBuilder(IEnumerable<KeyValuePair<string, StringValues>> parameters)
            : this(parameters.SelectMany(kvp => kvp.Value, (kvp, v) => KeyValuePair.Create(kvp.Key, v ?? string.Empty)))
        {
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.parameters.GetEnumerator();
        }

        /// <summary>
        ///     Adds a query string token to the instance.
        /// </summary>
        /// <param name="key">The query key.</param>
        /// <param name="values">The sequence of query values.</param>
        public void Add(string key, IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                this.parameters.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        /// <summary>
        ///     Adds a query string token to the instance.
        /// </summary>
        /// <param name="key">The query key.</param>
        /// <param name="value">The query value.</param>
        public void Add(string key, string value)
        {
            this.parameters.Add(new KeyValuePair<string, string>(key, value));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new ValueStringBuilder();
            var first = true;
            for (var i = 0; i < this.parameters.Count; i++)
            {
                var pair = this.parameters[i];
                builder.Append(first ? '?' : '&');
                first = false;
                builder.Append(UrlEncoder.Default.Encode(pair.Key));
                builder.Append('=');
                builder.Append(UrlEncoder.Default.Encode(pair.Value));
            }

            return builder.ToString();
        }

        /// <summary>
        ///     Constructs a <see cref="QueryString" /> from this <see cref="QueryBuilder" />.
        /// </summary>
        /// <returns>The <see cref="QueryString" />.</returns>
        public QueryString ToQueryString()
        {
            return new QueryString(this.ToString());
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.ToQueryString().GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return this.ToQueryString().Equals(obj);
        }
    }
}