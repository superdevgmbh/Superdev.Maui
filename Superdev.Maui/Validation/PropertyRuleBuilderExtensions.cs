using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Superdev.Maui.Validation
{
    /// <summary>
    /// The shipped rules. A missing value — <c>null</c>, or empty text — passes every rule except
    /// <see cref="NotNull{TProperty}"/>, <see cref="NotEmpty(PropertyRuleBuilder{string})"/> and <see cref="IsTrue(PropertyRuleBuilder{bool})"/>:
    /// presence is their job alone, so <c>MinimumLength(3)</c> on an optional field does not also make it required.
    /// A field the user typed into and cleared again counts as missing too.
    /// </summary>
    public static class PropertyRuleBuilderExtensions
    {
        private static readonly TimeSpan RegexMatchTimeout = TimeSpan.FromSeconds(1);

        #region Presence

#nullable disable

        public static PropertyRuleBuilder<TProperty> NotNull<TProperty>(this PropertyRuleBuilder<TProperty> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(ValidationErrorCodes.NotNull, value => value is not null);
        }

        /// <summary>Requires text: not <c>null</c>, not empty, not only white space.</summary>
        public static PropertyRuleBuilder<string> NotEmpty(this PropertyRuleBuilder<string> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(ValidationErrorCodes.NotEmpty, value => !string.IsNullOrWhiteSpace(value));
        }

#nullable restore

        /// <summary>
        /// Requires a collection with at least one entry. A missing collection counts as empty. A collection which
        /// announces its changes, such as an <c>ObservableCollection</c>, is watched: adding or removing an item
        /// validates the field like a property change. After changing any other collection in place, raise
        /// <c>PropertyChanged</c> for the property.
        /// </summary>
        public static PropertyRuleBuilder<TCollection> NotEmpty<TCollection>(this PropertyRuleBuilder<TCollection> builder)
            where TCollection : IEnumerable?
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(ValidationErrorCodes.NotEmpty, value => value is not null && HasAny(value));
        }

        /// <summary>
        /// Requires the value to be <c>true</c> — the terms check box, a confirmation switch. Unlike <c>EqualTo(true)</c>
        /// it reports <see cref="ValidationErrorCodes.IsTrue"/>, which has a message of its own.
        /// </summary>
        public static PropertyRuleBuilder<bool> IsTrue(this PropertyRuleBuilder<bool> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(ValidationErrorCodes.IsTrue, value => value);
        }

        /// <summary>Requires the value to be <c>true</c>; a missing value fails, as the question was not answered.</summary>
        public static PropertyRuleBuilder<bool?> IsTrue(this PropertyRuleBuilder<bool?> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(ValidationErrorCodes.IsTrue, value => value == true);
        }

        #endregion

        #region Equality

        /// <summary>Requires the value to equal <paramref name="expected"/>. A missing value passes.</summary>
        public static PropertyRuleBuilder<TProperty> EqualTo<TProperty>(this PropertyRuleBuilder<TProperty> builder, TProperty expected)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(
                ValidationErrorCodes.EqualTo,
                value => IsMissing(value) || EqualityComparer<TProperty>.Default.Equals(value, expected),
                _ => Arguments(("ComparisonValue", expected)));
        }

        /// <summary>
        /// Requires the value to equal another property's — confirm password, confirm e-mail. This property is
        /// validated again when the other one changes. The other property is named in the message
        /// (<c>{OtherPropertyName}</c>), not its value, which may be a secret. A missing value on either side passes:
        /// add <c>NotEmpty()</c> when the confirmation is required.
        /// </summary>
        public static PropertyRuleBuilder<TProperty> EqualTo<TProperty>(
            this PropertyRuleBuilder<TProperty> builder,
            Func<TProperty> other,
            [CallerArgumentExpression(nameof(other))] string? otherExpression = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(other);

            return builder.AddCrossField(
                ValidationErrorCodes.EqualToOtherProperty,
                value => EqualityComparer<TProperty>.Default.Equals(value, other()),
                other,
                otherExpression);
        }

        /// <summary>Requires the value to differ from <paramref name="unexpected"/>. A missing value passes.</summary>
        public static PropertyRuleBuilder<TProperty> NotEqualTo<TProperty>(this PropertyRuleBuilder<TProperty> builder, TProperty unexpected)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(
                ValidationErrorCodes.NotEqualTo,
                value => IsMissing(value) || !EqualityComparer<TProperty>.Default.Equals(value, unexpected),
                _ => Arguments(("ComparisonValue", unexpected)));
        }

        /// <summary>
        /// Requires the value to differ from another property's — a new password that is not the old one. This
        /// property is validated again when the other one changes. A missing value on either side passes.
        /// </summary>
        public static PropertyRuleBuilder<TProperty> NotEqualTo<TProperty>(
            this PropertyRuleBuilder<TProperty> builder,
            Func<TProperty> other,
            [CallerArgumentExpression(nameof(other))] string? otherExpression = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(other);

            return builder.AddCrossField(
                ValidationErrorCodes.NotEqualToOtherProperty,
                value => !EqualityComparer<TProperty>.Default.Equals(value, other()),
                other,
                otherExpression);
        }

        #endregion

        #region Text

#nullable disable

        /// <summary>Requires at least <paramref name="minimumLength"/> characters. A missing or empty value passes.</summary>
        public static PropertyRuleBuilder<string> MinimumLength(this PropertyRuleBuilder<string> builder, int minimumLength)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentOutOfRangeException.ThrowIfNegative(minimumLength);
            return builder.Add(
                ValidationErrorCodes.MinimumLength,
                value => string.IsNullOrEmpty(value) || value.Length >= minimumLength,
                _ => Arguments(("MinimumLength", minimumLength)));
        }

        /// <summary>Allows at most <paramref name="maximumLength"/> characters.</summary>
        public static PropertyRuleBuilder<string> MaximumLength(this PropertyRuleBuilder<string> builder, int maximumLength)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentOutOfRangeException.ThrowIfNegative(maximumLength);
            return builder.Add(
                ValidationErrorCodes.MaximumLength,
                value => value is null || value.Length <= maximumLength,
                _ => Arguments(("MaximumLength", maximumLength)));
        }

        /// <summary>Requires exactly <paramref name="length"/> characters — a PIN, a country code. A missing or empty value passes.</summary>
        public static PropertyRuleBuilder<string> Length(this PropertyRuleBuilder<string> builder, int length)
        {
            return builder.Length(length, length);
        }

        /// <summary>Requires between <paramref name="minimumLength"/> and <paramref name="maximumLength"/> characters. A missing or empty value passes.</summary>
        public static PropertyRuleBuilder<string> Length(this PropertyRuleBuilder<string> builder, int minimumLength, int maximumLength)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentOutOfRangeException.ThrowIfNegative(minimumLength);
            ArgumentOutOfRangeException.ThrowIfLessThan(maximumLength, minimumLength);
            return builder.Add(
                ValidationErrorCodes.Length,
                value => string.IsNullOrEmpty(value) || (value.Length >= minimumLength && value.Length <= maximumLength),
                _ => Arguments(("MinimumLength", minimumLength), ("MaximumLength", maximumLength)));
        }

        /// <summary>Requires the text to match <paramref name="pattern"/>, compiled once here and matched with a timeout. A missing or blank value passes.</summary>
        public static PropertyRuleBuilder<string> Matches(this PropertyRuleBuilder<string> builder, string pattern, RegexOptions options = RegexOptions.None)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrEmpty(pattern);
            return builder.Matches(new Regex(pattern, options, RegexMatchTimeout));
        }

        public static PropertyRuleBuilder<string> Matches(this PropertyRuleBuilder<string> builder, Regex regex)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(regex);
            return builder.Add(
                ValidationErrorCodes.Matches,
                value => string.IsNullOrWhiteSpace(value) || regex.IsMatch(value),
                _ => Arguments(("Pattern", regex.ToString())));
        }

        /// <summary>
        /// Requires the shape of an e-mail address — something, one <c>@</c>, something, no white space. Whether the
        /// address exists is the server's question. A missing or blank value passes.
        /// </summary>
        public static PropertyRuleBuilder<string> EmailAddress(this PropertyRuleBuilder<string> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(ValidationErrorCodes.EmailAddress, value => string.IsNullOrWhiteSpace(value) || IsEmailAddress(value));
        }

#nullable restore

        #endregion

        #region Comparison

        public static PropertyRuleBuilder<TProperty> GreaterThan<TProperty>(this PropertyRuleBuilder<TProperty> builder, TProperty value)
        {
            return builder.Compare(ValidationErrorCodes.GreaterThan, value, comparison => comparison > 0);
        }

        public static PropertyRuleBuilder<TProperty> GreaterThanOrEqualTo<TProperty>(this PropertyRuleBuilder<TProperty> builder, TProperty value)
        {
            return builder.Compare(ValidationErrorCodes.GreaterThanOrEqualTo, value, comparison => comparison >= 0);
        }

        public static PropertyRuleBuilder<TProperty> LessThan<TProperty>(this PropertyRuleBuilder<TProperty> builder, TProperty value)
        {
            return builder.Compare(ValidationErrorCodes.LessThan, value, comparison => comparison < 0);
        }

        public static PropertyRuleBuilder<TProperty> LessThanOrEqualTo<TProperty>(this PropertyRuleBuilder<TProperty> builder, TProperty value)
        {
            return builder.Compare(ValidationErrorCodes.LessThanOrEqualTo, value, comparison => comparison <= 0);
        }

        /// <summary>Requires <paramref name="from"/> ≤ value ≤ <paramref name="to"/>. A missing value passes.</summary>
        public static PropertyRuleBuilder<TProperty> Between<TProperty>(this PropertyRuleBuilder<TProperty> builder, TProperty from, TProperty to)
        {
            ArgumentNullException.ThrowIfNull(builder);
            var comparer = OrderableComparer<TProperty>();

            if (!IsMissing(from) && !IsMissing(to) && comparer.Compare(from, to) > 0)
            {
                throw new ArgumentException($"The range is empty: {from} is greater than {to}.", nameof(from));
            }

            return builder.Add(
                ValidationErrorCodes.Between,
                value => IsMissing(value) || (comparer.Compare(value, from) >= 0 && comparer.Compare(value, to) <= 0),
                _ => Arguments(("From", from), ("To", to)));
        }

        /// <summary>Requires the value to be one of <paramref name="values"/>. A missing value passes.</summary>
        public static PropertyRuleBuilder<TProperty> OneOf<TProperty>(this PropertyRuleBuilder<TProperty> builder, params TProperty[] values)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(values);
            var allowed = new HashSet<TProperty>(values);
            return builder.Add(
                ValidationErrorCodes.OneOf,
                value => IsMissing(value) || allowed.Contains(value),
                _ => Arguments(("Values", string.Join(", ", values))));
        }

        private static PropertyRuleBuilder<TProperty> Compare<TProperty>(
            this PropertyRuleBuilder<TProperty> builder,
            string errorCode,
            TProperty comparisonValue,
            Func<int, bool> accept)
        {
            ArgumentNullException.ThrowIfNull(builder);
            var comparer = OrderableComparer<TProperty>();
            return builder.Add(
                errorCode,
                value => IsMissing(value) || accept(comparer.Compare(value, comparisonValue)),
                _ => Arguments(("ComparisonValue", comparisonValue)));
        }

        /// <summary>
        /// The comparer of an orderable type, asked once when the rule is declared, because <see cref="Comparer{T}.Default"/>
        /// only fails on the first comparison. Nullable types are ordered by their underlying type.
        /// </summary>
        private static IComparer<TProperty> OrderableComparer<TProperty>()
        {
            var type = Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty);
            if (!typeof(IComparable).IsAssignableFrom(type) && !typeof(IComparable<TProperty>).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"A comparison rule needs an orderable value; {typeof(TProperty).Name} does not implement IComparable.");
            }

            return Comparer<TProperty>.Default;
        }

        #endregion

        #region Dates

        /// <summary>Requires a moment before now — a birthdate. A missing value passes.</summary>
        public static PropertyRuleBuilder<DateTime?> InThePast(this PropertyRuleBuilder<DateTime?> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(ValidationErrorCodes.InThePast, value => value is null || value.Value < Now(builder.TimeProvider(), value.Value.Kind));
        }

        public static PropertyRuleBuilder<DateTime> InThePast(this PropertyRuleBuilder<DateTime> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(ValidationErrorCodes.InThePast, value => value < Now(builder.TimeProvider(), value.Kind));
        }

        public static PropertyRuleBuilder<DateOnly?> InThePast(this PropertyRuleBuilder<DateOnly?> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(ValidationErrorCodes.InThePast, value => value is null || value.Value < Today(builder.TimeProvider()));
        }

        public static PropertyRuleBuilder<DateOnly> InThePast(this PropertyRuleBuilder<DateOnly> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(ValidationErrorCodes.InThePast, value => value < Today(builder.TimeProvider()));
        }

        /// <summary>Requires a moment after now — an appointment. A missing value passes.</summary>
        public static PropertyRuleBuilder<DateTime?> InTheFuture(this PropertyRuleBuilder<DateTime?> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(ValidationErrorCodes.InTheFuture, value => value is null || value.Value > Now(builder.TimeProvider(), value.Value.Kind));
        }

        public static PropertyRuleBuilder<DateTime> InTheFuture(this PropertyRuleBuilder<DateTime> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(ValidationErrorCodes.InTheFuture, value => value > Now(builder.TimeProvider(), value.Kind));
        }

        public static PropertyRuleBuilder<DateOnly?> InTheFuture(this PropertyRuleBuilder<DateOnly?> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(ValidationErrorCodes.InTheFuture, value => value is null || value.Value > Today(builder.TimeProvider()));
        }

        public static PropertyRuleBuilder<DateOnly> InTheFuture(this PropertyRuleBuilder<DateOnly> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(ValidationErrorCodes.InTheFuture, value => value > Today(builder.TimeProvider()));
        }

        private static DateTime Now(TimeProvider timeProvider, DateTimeKind kind)
        {
            return kind == DateTimeKind.Utc ? timeProvider.GetUtcNow().UtcDateTime : timeProvider.GetLocalNow().DateTime;
        }

        private static DateOnly Today(TimeProvider timeProvider)
        {
            return DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        }

        #endregion

        #region Text that has to parse — client-only, the server validates the parsed model

#nullable disable

        /// <summary>Requires text that parses as a whole number in the current culture (or <paramref name="formatProvider"/>). A missing or blank value passes.</summary>
        public static PropertyRuleBuilder<string> IsInteger(this PropertyRuleBuilder<string> builder, IFormatProvider formatProvider = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(
                ValidationErrorCodes.IsInteger,
                value => string.IsNullOrWhiteSpace(value) || long.TryParse(value, NumberStyles.Integer, formatProvider ?? CultureInfo.CurrentCulture, out _));
        }

        /// <summary>
        /// Requires text that parses as a number in the current culture (or <paramref name="formatProvider"/>) —
        /// <c>1,5</c> under de-DE, <c>1.5</c> under en-US and de-CH. Group separators are not accepted, so
        /// <c>1,5</c> fails under en-US rather than becoming fifteen. A missing or blank value passes.
        /// </summary>
        public static PropertyRuleBuilder<string> IsDecimal(this PropertyRuleBuilder<string> builder, IFormatProvider formatProvider = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(
                ValidationErrorCodes.IsDecimal,
                value => string.IsNullOrWhiteSpace(value) || decimal.TryParse(value, NumberStyles.Float, formatProvider ?? CultureInfo.CurrentCulture, out _));
        }

        /// <summary>Requires text that parses as a date in the current culture (or <paramref name="formatProvider"/>). A missing or blank value passes.</summary>
        public static PropertyRuleBuilder<string> IsDate(this PropertyRuleBuilder<string> builder, IFormatProvider formatProvider = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(
                ValidationErrorCodes.IsDate,
                value => string.IsNullOrWhiteSpace(value) || DateTime.TryParse(value, formatProvider ?? CultureInfo.CurrentCulture, DateTimeStyles.None, out _));
        }

        /// <summary>Requires text that parses as a time of day in the current culture (or <paramref name="formatProvider"/>). A missing or blank value passes.</summary>
        public static PropertyRuleBuilder<string> IsTime(this PropertyRuleBuilder<string> builder, IFormatProvider formatProvider = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.Add(
                ValidationErrorCodes.IsTime,
                value => string.IsNullOrWhiteSpace(value) || TimeOnly.TryParse(value, formatProvider ?? CultureInfo.CurrentCulture, DateTimeStyles.None, out _));
        }

#nullable restore

        #endregion

        #region Custom

        /// <summary>A one-off rule. The predicate returns <c>true</c> when the value is valid.</summary>
        public static PropertyRuleBuilder<TProperty> Must<TProperty>(this PropertyRuleBuilder<TProperty> builder, Func<TProperty, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(predicate);
            return builder.Add(ValidationErrorCodes.Must, predicate);
        }

        /// <summary>
        /// A one-off rule which has to await something — "is this user name free?". It runs once every synchronous
        /// rule of the property passed, and the field is pending meanwhile. With a <paramref name="debounce"/>, a
        /// validation triggered by typing waits that long and is discarded when the value changes again meanwhile; an
        /// explicit validation runs it at once. Use <see cref="PropertyRuleBuilder{TProperty}.AddAsync"/> for a
        /// <see cref="ValueTask{TResult}"/>-returning rule.
        /// </summary>
        public static PropertyRuleBuilder<TProperty> MustAsync<TProperty>(
            this PropertyRuleBuilder<TProperty> builder,
            Func<TProperty, CancellationToken, Task<bool>> predicate,
            TimeSpan? debounce = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(predicate);
            return builder.AddAsync(ValidationErrorCodes.MustAsync, (value, ct) => new ValueTask<bool>(predicate(value, ct)), debounce);
        }

        #endregion

        /// <summary>
        /// A rule comparing with another property: it passes while either side is missing, re-validates when the
        /// other side changes, and names the other property in its message.
        /// </summary>
        private static PropertyRuleBuilder<TProperty> AddCrossField<TProperty>(
            this PropertyRuleBuilder<TProperty> builder,
            string errorCode,
            Func<TProperty, bool> isValid,
            Func<TProperty> other,
            string? otherExpression)
        {
            string? otherName = null;
            if (PropertyNameParser.TryParse(otherExpression, out var parsed))
            {
                otherName = parsed;
                builder.DependsOn(parsed);
            }

            return builder.Append(new PropertyRule<TProperty>(errorCode)
            {
                Predicate = value => IsMissing(value) || IsMissing(other()) || isValid(value),
                OtherPropertyName = otherName,
                Arguments = otherName == null ? _ => Arguments(("OtherPropertyName", "the other value")) : null,
            });
        }

        /// <summary>A missing value: <c>null</c>, or text that is empty.</summary>
        private static bool IsMissing<TProperty>(TProperty value)
        {
            return value is null || value is string { Length: 0 };
        }

        private static bool HasAny(IEnumerable collection)
        {
            if (collection is ICollection sized)
            {
                return sized.Count > 0;
            }

            var enumerator = collection.GetEnumerator();
            try
            {
                return enumerator.MoveNext();
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }

        private static bool IsEmailAddress(string value)
        {
            var at = value.IndexOf('@');
            return at > 0
                && at == value.LastIndexOf('@')
                && at < value.Length - 1
                && !value.Any(char.IsWhiteSpace);
        }

        private static IReadOnlyDictionary<string, object?> Arguments(params (string Key, object? Value)[] arguments)
        {
            var dictionary = new Dictionary<string, object?>(arguments.Length, StringComparer.Ordinal);
            foreach (var (key, value) in arguments)
            {
                dictionary[key] = value;
            }

            return dictionary;
        }
    }
}
