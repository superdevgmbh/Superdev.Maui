using System.ComponentModel;
using Superdev.Maui.Internals;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Controls
{
    /// <summary>
    /// The supporting text under a plain control: shows the first message of the field named by
    /// <see cref="PropertyName"/> and hides itself while there is none. Wired like <see cref="Validation"/>,
    /// through the binding context or an explicit <see cref="Source"/>, and like it released on unload: the field
    /// reaches the label through a weak subscription only.
    /// </summary>
    public class ValidationErrorLabel : CustomLabel
    {
        public static readonly BindableProperty PropertyNameProperty =
            BindableProperty.Create(
                nameof(PropertyName),
                typeof(string),
                typeof(ValidationErrorLabel),
                null,
                propertyChanged: (bindable, _, _) => ((ValidationErrorLabel)bindable).Rewire());

        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(
                nameof(Source),
                typeof(ViewModelValidation),
                typeof(ValidationErrorLabel),
                null,
                propertyChanged: (bindable, _, _) => ((ValidationErrorLabel)bindable).Rewire());

        private FieldState? field;
        private IDisposable? fieldSubscription;

        public ValidationErrorLabel()
        {
            this.SetDynamicResource(StyleProperty, "ValidationErrorLabelStyle");
            this.IsVisible = false;
            this.Loaded += (_, _) => this.Rewire();
            this.Unloaded += (_, _) => this.Release();
        }

        public string? PropertyName
        {
            get => (string?)this.GetValue(PropertyNameProperty);
            set => this.SetValue(PropertyNameProperty, value);
        }

        public ViewModelValidation? Source
        {
            get => (ViewModelValidation?)this.GetValue(SourceProperty);
            set => this.SetValue(SourceProperty, value);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            this.Rewire();
        }

        private void Rewire()
        {
            this.Release();

            var propertyName = this.PropertyName;
            var form = Validation.ResolveForm(this.BindingContext, this.Source);
            if (form != null && !string.IsNullOrWhiteSpace(propertyName))
            {
                this.field = form.Fields[propertyName];
                this.fieldSubscription = WeakSubscription.ToPropertyChanged(
                    this.field,
                    this,
                    static (label, e) => label.OnFieldChanged(e));
            }

            this.Apply();
        }

        private void Release()
        {
            this.fieldSubscription?.Dispose();
            this.fieldSubscription = null;
            this.field = null;
        }

        private void OnFieldChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(FieldState.FirstMessage) or nameof(FieldState.Messages))
            {
                UiThread.Run(this, this.Apply);
            }
        }

        private void Apply()
        {
            var message = this.field?.FirstMessage;
            this.Text = message;
            this.IsVisible = !string.IsNullOrEmpty(message);
        }
    }
}
