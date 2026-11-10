namespace Superdev.Maui.Validation
{
    /// <summary>
    /// The error codes of the shipped rules. They are the localization keys: a message provider resolves a
    /// code to a message, and <c>WithErrorCode</c> lets a rule report under a code of its own.
    /// </summary>
    public static class ValidationErrorCodes
    {
        public const string NotNull = "NotNull";
        public const string NotEmpty = "NotEmpty";
        public const string IsTrue = "IsTrue";
        public const string EqualTo = "EqualTo";
        public const string NotEqualTo = "NotEqualTo";
        public const string EqualToOtherProperty = "EqualToOtherProperty";
        public const string NotEqualToOtherProperty = "NotEqualToOtherProperty";
        public const string MinimumLength = "MinimumLength";
        public const string MaximumLength = "MaximumLength";
        public const string Length = "Length";
        public const string Matches = "Matches";
        public const string EmailAddress = "EmailAddress";
        public const string GreaterThan = "GreaterThan";
        public const string GreaterThanOrEqualTo = "GreaterThanOrEqualTo";
        public const string LessThan = "LessThan";
        public const string LessThanOrEqualTo = "LessThanOrEqualTo";
        public const string Between = "Between";
        public const string OneOf = "OneOf";
        public const string InThePast = "InThePast";
        public const string InTheFuture = "InTheFuture";
        public const string IsInteger = "IsInteger";
        public const string IsDecimal = "IsDecimal";
        public const string IsDate = "IsDate";
        public const string IsTime = "IsTime";
        public const string Must = "Must";
        public const string MustAsync = "MustAsync";

        /// <summary>
        /// An error that was set from outside the rules (<see cref="ViewModelValidation.SetErrors{TMessages}"/>).
        /// </summary>
        public const string Custom = "Custom";
    }
}
