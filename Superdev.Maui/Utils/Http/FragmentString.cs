// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Source: https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http.Abstractions/src/FragmentString.cs

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Superdev.Maui.Utils.Http
{
    /// <summary>
    ///     Provides correct handling for FragmentString value when needed to generate a URI string.
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public readonly struct FragmentString : IEquatable<FragmentString>
    {
        /// <summary>
        ///     Represents the empty fragment string. This field is read-only.
        /// </summary>
        public static readonly FragmentString Empty = new(string.Empty);

        /// <summary>
        ///     Initialize the fragment string with a given value. This value must be in escaped and delimited format with
        ///     a leading '#' character.
        /// </summary>
        /// <param name="value">The fragment string to be assigned to the Value property.</param>
        public FragmentString(string? value)
        {
            if (!string.IsNullOrEmpty(value) && value[0] != '#')
            {
                throw new ArgumentException("The leading '#' must be included for a non-empty fragment.", nameof(value));
            }

            this.Value = value;
        }

        /// <summary>
        ///     The escaped fragment string with the leading '#' character.
        /// </summary>
        public string? Value { get; }

        /// <summary>
        ///     True if the fragment string is not empty.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue => !string.IsNullOrEmpty(this.Value);

        /// <summary>
        ///     Provides the fragment string escaped in a way which is correct for combining into the URI representation.
        ///     A leading '#' character will be included unless the Value is null or empty.
        /// </summary>
        /// <returns>The fragment string value.</returns>
        public override string ToString()
        {
            return this.ToUriComponent();
        }

        /// <summary>
        ///     Provides the fragment string escaped in a way which is correct for combining into the URI representation.
        ///     A leading '#' character will be included unless the Value is null or empty.
        /// </summary>
        /// <returns>The fragment string value.</returns>
        public string ToUriComponent()
        {
            return this.HasValue ? this.Value : string.Empty;
        }

        /// <summary>
        ///     Returns a <see cref="FragmentString" /> given the fragment as it is escaped in the URI format.
        /// </summary>
        /// <param name="uriComponent">The escaped fragment as it appears in the URI format.</param>
        /// <returns>The resulting <see cref="FragmentString" />.</returns>
        public static FragmentString FromUriComponent(string uriComponent)
        {
            if (string.IsNullOrEmpty(uriComponent))
            {
                return Empty;
            }

            return new FragmentString(uriComponent);
        }

        /// <summary>
        ///     Returns a <see cref="FragmentString" /> given the fragment as from a Uri object.
        /// </summary>
        /// <param name="uri">The Uri object.</param>
        /// <returns>The resulting <see cref="FragmentString" />.</returns>
        public static FragmentString FromUriComponent(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            var fragmentValue = uri.GetComponents(UriComponents.Fragment, UriFormat.UriEscaped);
            if (!string.IsNullOrEmpty(fragmentValue))
            {
                fragmentValue = "#" + fragmentValue;
            }

            return new FragmentString(fragmentValue);
        }

        /// <summary>
        ///     Evaluates if the current fragment is equal to <paramref name="other" />.
        /// </summary>
        /// <param name="other">A <see cref="FragmentString" /> to compare.</param>
        /// <returns><see langword="true" /> if the fragments are equal.</returns>
        public bool Equals(FragmentString other)
        {
            if (!this.HasValue && !other.HasValue)
            {
                return true;
            }

            return string.Equals(this.Value, other.Value, StringComparison.Ordinal);
        }

        /// <summary>
        ///     Evaluates if the current fragment is equal to an object <paramref name="obj" />.
        /// </summary>
        /// <param name="obj">An object to compare.</param>
        /// <returns><see langword="true" /> if the fragments are equal.</returns>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return !this.HasValue;
            }

            return obj is FragmentString fragmentString && this.Equals(fragmentString);
        }

        /// <summary>
        ///     Gets a hash code for the value.
        /// </summary>
        /// <returns>The hash code as an <see cref="int" />.</returns>
        public override int GetHashCode()
        {
            return this.HasValue ? this.Value.GetHashCode() : 0;
        }

        /// <summary>
        ///     Evaluates if one fragment is equal to another.
        /// </summary>
        /// <param name="left">A <see cref="FragmentString" /> instance.</param>
        /// <param name="right">A <see cref="FragmentString" /> instance.</param>
        /// <returns><see langword="true" /> if the fragments are equal.</returns>
        public static bool operator ==(FragmentString left, FragmentString right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Evalutes if one fragment is not equal to another.
        /// </summary>
        /// <param name="left">A <see cref="FragmentString" /> instance.</param>
        /// <param name="right">A <see cref="FragmentString" /> instance.</param>
        /// <returns><see langword="true" /> if the fragments are not equal.</returns>
        public static bool operator !=(FragmentString left, FragmentString right)
        {
            return !left.Equals(right);
        }
    }
}
