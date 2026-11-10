namespace Superdev.Maui.Validation
{
    /// <summary>
    /// When a field is validated for the first time — before it has an error and before the form was validated
    /// explicitly. <see cref="ReValidationMode"/> takes over once a field has an error or the form was validated.
    /// </summary>
    public enum ValidationMode
    {
        /// <summary>Only an explicit <see cref="ViewModelValidation.ValidateAsync(IEnumerable{string}?, CancellationToken)"/> validates. The default.</summary>
        Manual,

        /// <summary>The field validates when it loses focus for the first time, and on every change after that.</summary>
        OnTouched,

        /// <summary>The field validates when it loses focus.</summary>
        OnBlur,

        /// <summary>The field validates on every change.</summary>
        OnChange,

        /// <summary>The field validates on every change and whenever it loses focus.</summary>
        All,
    }

    /// <summary>
    /// When a field which already has an error — or any field once the form was validated explicitly — is
    /// validated again, so that the error clears the moment the user corrects it.
    /// </summary>
    public enum ReValidationMode
    {
        /// <summary>On every change. The default.</summary>
        OnChange,

        /// <summary>When the field loses focus.</summary>
        OnBlur,

        /// <summary>Only on the next explicit validation.</summary>
        Manual,
    }

    /// <summary>
    /// How many of a property's rules report when several fail.
    /// </summary>
    public enum ValidationBehavior
    {
        /// <summary>The first failing rule reports and the rest of the chain is skipped. The default.</summary>
        StopAtFirstError,

        /// <summary>Every failing rule reports.</summary>
        All,
    }
}
