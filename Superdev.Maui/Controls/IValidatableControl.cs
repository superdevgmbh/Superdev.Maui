using Superdev.Maui.Validation;

namespace Superdev.Maui.Controls
{
    /// <summary>
    /// What the validation wiring needs from one of the library's own input controls: the element that actually
    /// receives focus, and a place to hand the field's state to, so that the control renders errors and pending
    /// state itself.
    /// </summary>
    internal interface IValidatableControl
    {
        VisualElement InputElement { get; }

        FieldState? Field { get; set; }

        /// <summary>Renders the field's messages and pending state.</summary>
        void ApplyField(IEnumerable<string> messages, bool isPending);
    }
}
