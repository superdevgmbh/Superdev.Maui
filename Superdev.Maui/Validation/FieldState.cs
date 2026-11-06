using Superdev.Maui.Mvvm;

namespace Superdev.Maui.Validation
{
    /// <summary>
    /// The state of one field: its errors, and whether the user has touched it, changed it, or is waiting for
    /// an asynchronous rule. Bind to it through <c>Validation.Fields[Name]</c>; every member notifies.
    /// </summary>
    public sealed class FieldState : BindableBase
    {
        private static readonly IReadOnlyList<ValidationError> NoErrors = Array.Empty<ValidationError>();
        private static readonly IReadOnlyList<string> NoMessages = Array.Empty<string>();

        private IReadOnlyList<ValidationError> errors = NoErrors;
        private IReadOnlyList<string> messages = NoMessages;
        private bool isTouched;
        private bool isDirty;
        private bool isPending;

        internal FieldState(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        public string PropertyName { get; }

        /// <summary>The errors of the field, rule and set errors alike, as a snapshot.</summary>
        public IReadOnlyList<ValidationError> Errors => this.errors;

        /// <summary>The messages of <see cref="Errors"/>, what a control shows.</summary>
        public IReadOnlyList<string> Messages => this.messages;

        public string? FirstMessage => this.messages.Count > 0 ? this.messages[0] : null;

        public bool HasErrors => this.errors.Count > 0;

        /// <summary>The user has been here: the field lost focus, or the form was validated explicitly.</summary>
        public bool IsTouched => this.isTouched;

        /// <summary>The value differs from the one the field had when the rules were set up, or at the last <see cref="ViewModelValidation.Reset"/>.</summary>
        public bool IsDirty => this.isDirty;

        /// <summary>An asynchronous rule is in flight.</summary>
        public bool IsPending => this.isPending;

        internal void SetErrors(IReadOnlyList<ValidationError> value)
        {
            var hadErrors = this.HasErrors;
            var firstMessage = this.FirstMessage;

            this.errors = value.Count == 0 ? NoErrors : value;
            this.messages = value.Count == 0 ? NoMessages : value.Select(e => e.Message).ToArray();

            this.RaisePropertyChanged(nameof(this.Errors));
            this.RaisePropertyChanged(nameof(this.Messages));

            if (firstMessage != this.FirstMessage)
            {
                this.RaisePropertyChanged(nameof(this.FirstMessage));
            }

            if (hadErrors != this.HasErrors)
            {
                this.RaisePropertyChanged(nameof(this.HasErrors));
            }
        }

        internal void SetTouched(bool value)
        {
            this.SetProperty(ref this.isTouched, value, nameof(this.IsTouched));
        }

        internal void SetDirty(bool value)
        {
            this.SetProperty(ref this.isDirty, value, nameof(this.IsDirty));
        }

        internal void SetPending(bool value)
        {
            this.SetProperty(ref this.isPending, value, nameof(this.IsPending));
        }
    }
}
