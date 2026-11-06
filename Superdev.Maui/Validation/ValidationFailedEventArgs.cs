namespace Superdev.Maui.Validation
{
    /// <summary>
    /// Raised through <see cref="ViewModelValidation.ValidationFailed"/> when a rule threw while a property was
    /// validated after a change — the path no caller awaits.
    /// </summary>
    public sealed class ValidationFailedEventArgs : EventArgs
    {
        public ValidationFailedEventArgs(string? propertyName, Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            this.PropertyName = propertyName;
            this.Exception = exception;
        }

        /// <summary>The property whose change was being validated.</summary>
        public string? PropertyName { get; }

        public Exception Exception { get; }
    }
}
