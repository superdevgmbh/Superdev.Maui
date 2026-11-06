using Superdev.Maui.Validation;

namespace Superdev.Maui.Controls
{
    /// <summary>
    /// A bordered summary of every message of a form — its fields, its form-level messages and its children's — which
    /// hides itself while there is none.
    /// </summary>
    public partial class ValidationErrorBox : Border
    {
        public ValidationErrorBox()
        {
            this.InitializeComponent();
        }

        public static readonly BindableProperty ValidationProperty =
            BindableProperty.Create(
                nameof(Validation),
                typeof(ViewModelValidation),
                typeof(ValidationErrorBox));

        public ViewModelValidation Validation
        {
            get => (ViewModelValidation)this.GetValue(ValidationProperty);
            set => this.SetValue(ValidationProperty, value);
        }
    }
}
