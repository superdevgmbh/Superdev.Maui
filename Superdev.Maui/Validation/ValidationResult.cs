namespace Superdev.Maui.Validation
{
    /// <summary>
    /// The outcome of a validation, or of an action run through <see cref="ViewModelValidation.ValidateAsync(Func{CancellationToken, Task{ValidationResult}}, IEnumerable{string}?, CancellationToken)"/>.
    /// It is immutable and never null: a successful result carries an empty error list.
    /// </summary>
    public sealed class ValidationResult
    {
        public static ValidationResult Success { get; } = new ValidationResult(Array.Empty<ValidationError>());

        private ValidationResult(IReadOnlyList<ValidationError> errors)
        {
            this.Errors = errors;
        }

        public IReadOnlyList<ValidationError> Errors { get; }

        public bool Succeeded => this.Errors.Count == 0;

        public static ValidationResult FromErrors(IEnumerable<ValidationError> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);

            var array = errors.ToArray();
            return array.Length == 0 ? Success : new ValidationResult(array);
        }

        /// <summary>
        /// Creates a result from a property → messages map, the shape every backend answers with
        /// (problem details <c>errors</c>, a validation tool's "to dictionary", a DTO of your own).
        /// </summary>
        public static ValidationResult FromErrors<TMessages>(IEnumerable<KeyValuePair<string, TMessages>> errors)
            where TMessages : IEnumerable<string>
        {
            ArgumentNullException.ThrowIfNull(errors);

            var list = new List<ValidationError>();
            foreach (var (propertyName, messages) in errors)
            {
                if (messages == null)
                {
                    continue;
                }

                foreach (var message in messages)
                {
                    list.Add(new ValidationError(propertyName ?? string.Empty, ValidationErrorCodes.Custom, message));
                }
            }

            return list.Count == 0 ? Success : new ValidationResult(list);
        }

        public static ValidationResult FromError(string propertyName, string message)
        {
            return new ValidationResult(new[] { new ValidationError(propertyName, ValidationErrorCodes.Custom, message) });
        }

        /// <summary>
        /// The errors as a property → messages map.
        /// </summary>
        public IReadOnlyDictionary<string, string[]> ToErrorsDictionary()
        {
            return this.Errors
                .GroupBy(e => e.PropertyName, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray(), StringComparer.Ordinal);
        }
    }
}
