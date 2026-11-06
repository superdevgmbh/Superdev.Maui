using Superdev.Maui.Localization;

namespace Superdev.Maui.Validation
{
    /// <summary>
    /// Resolves an error code through an <see cref="ITranslationProvider"/> — the resource key is the code
    /// prefixed with <see cref="TranslationKeyPrefix"/>, e.g. <c>Validation_NotEmpty</c> — and falls back to
    /// a built-in English template when there is no provider or the key is not translated. Display names are
    /// looked up the same way, under <see cref="DisplayNameKeyPrefix"/> plus the property name, e.g.
    /// <c>ValidationField_PostalCode</c>.
    /// </summary>
    /// <remarks>
    /// <c>UseSuperdevMaui</c> installs one backed by the application's translation provider as
    /// <see cref="ViewModelValidation.DefaultMessageProvider"/>.
    /// </remarks>
    public class DefaultValidationMessageProvider : IValidationMessageProvider
    {
        public const string TranslationKeyPrefix = "Validation_";
        public const string DisplayNameKeyPrefix = "ValidationField_";

        private static readonly IReadOnlyDictionary<string, string> Templates = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ValidationErrorCodes.NotNull] = "{PropertyName} must not be empty.",
            [ValidationErrorCodes.NotEmpty] = "{PropertyName} must not be empty.",
            [ValidationErrorCodes.IsTrue] = "{PropertyName} is required.",
            [ValidationErrorCodes.EqualTo] = "{PropertyName} must be equal to {ComparisonValue}.",
            [ValidationErrorCodes.NotEqualTo] = "{PropertyName} must not be equal to {ComparisonValue}.",
            [ValidationErrorCodes.EqualToOtherProperty] = "{PropertyName} must match {OtherPropertyName}.",
            [ValidationErrorCodes.NotEqualToOtherProperty] = "{PropertyName} must not match {OtherPropertyName}.",
            [ValidationErrorCodes.MinimumLength] = "{PropertyName} must be at least {MinimumLength} characters long.",
            [ValidationErrorCodes.MaximumLength] = "{PropertyName} must be at most {MaximumLength} characters long.",
            [ValidationErrorCodes.Length] = "{PropertyName} must be between {MinimumLength} and {MaximumLength} characters long.",
            [ValidationErrorCodes.Matches] = "{PropertyName} is not in the correct format.",
            [ValidationErrorCodes.EmailAddress] = "{PropertyName} is not a valid e-mail address.",
            [ValidationErrorCodes.GreaterThan] = "{PropertyName} must be greater than {ComparisonValue}.",
            [ValidationErrorCodes.GreaterThanOrEqualTo] = "{PropertyName} must be greater than or equal to {ComparisonValue}.",
            [ValidationErrorCodes.LessThan] = "{PropertyName} must be less than {ComparisonValue}.",
            [ValidationErrorCodes.LessThanOrEqualTo] = "{PropertyName} must be less than or equal to {ComparisonValue}.",
            [ValidationErrorCodes.Between] = "{PropertyName} must be between {From} and {To}.",
            [ValidationErrorCodes.OneOf] = "{PropertyName} must be one of: {Values}.",
            [ValidationErrorCodes.InThePast] = "{PropertyName} must be in the past.",
            [ValidationErrorCodes.InTheFuture] = "{PropertyName} must be in the future.",
            [ValidationErrorCodes.IsInteger] = "{PropertyName} must be a whole number.",
            [ValidationErrorCodes.IsDecimal] = "{PropertyName} must be a number.",
            [ValidationErrorCodes.IsDate] = "{PropertyName} must be a valid date.",
            [ValidationErrorCodes.IsTime] = "{PropertyName} must be a valid time.",
            [ValidationErrorCodes.Must] = "{PropertyName} is not valid.",
            [ValidationErrorCodes.MustAsync] = "{PropertyName} is not valid.",
            [ValidationErrorCodes.Custom] = "{PropertyName} is not valid.",
        };

        private readonly ITranslationProvider? translationProvider;
        private readonly string translationKeyPrefix;
        private readonly string displayNameKeyPrefix;

        public DefaultValidationMessageProvider()
            : this(null)
        {
        }

        public DefaultValidationMessageProvider(ITranslationProvider? translationProvider, string? translationKeyPrefix = null, string? displayNameKeyPrefix = null)
        {
            this.translationProvider = translationProvider;
            this.translationKeyPrefix = translationKeyPrefix ?? TranslationKeyPrefix;
            this.displayNameKeyPrefix = displayNameKeyPrefix ?? DisplayNameKeyPrefix;
        }

        public string GetMessage(string errorCode, IReadOnlyDictionary<string, object?> arguments)
        {
            ArgumentNullException.ThrowIfNull(errorCode);
            ArgumentNullException.ThrowIfNull(arguments);

            var template = this.TryTranslate(this.translationKeyPrefix + errorCode)
                ?? (Templates.TryGetValue(errorCode, out var builtIn) ? builtIn : Templates[ValidationErrorCodes.Custom]);

            return ValidationMessageFormatter.Format(template, arguments);
        }

        public string? GetDisplayName(string propertyName)
        {
            ArgumentNullException.ThrowIfNull(propertyName);
            return this.TryTranslate(this.displayNameKeyPrefix + propertyName);
        }

        private string? TryTranslate(string key)
        {
            if (this.translationProvider == null)
            {
                return null;
            }

            var translated = this.translationProvider.Translate(key);

            // The resx and default providers answer a missing key with "#key#"; treat that, the key itself
            // and an empty string as "not translated" so that the built-in template is used instead.
            if (string.IsNullOrWhiteSpace(translated) || translated == key || translated == $"#{key}#")
            {
                return null;
            }

            return translated;
        }
    }
}
