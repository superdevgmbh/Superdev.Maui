using System.ComponentModel;
using System.Diagnostics;
using Superdev.Maui.Effects;
using Superdev.Maui.Internals;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Controls
{
    /// <summary>
    /// Wires any control to a field of the view model's validation with one attribute:
    /// <c>&lt;Entry s:Validation.PropertyName="UserName" /&gt;</c>. The control then marks the field touched when it
    /// loses focus, is scrolled into view — and, when it takes text, focused — when the field is the first invalid one,
    /// announces the error to a screen reader after its own hint, and — for the input types <see cref="LineColorEffect"/>
    /// supports — colours its underline while the field has errors. It works on a plain MAUI control, on a third-party
    /// one, and on the library's own <c>Validatable*</c> controls, which additionally render the error text.
    /// </summary>
    public static class Validation
    {
        public static readonly BindableProperty PropertyNameProperty = BindableProperty.CreateAttached(
            "PropertyName",
            typeof(string),
            typeof(Validation),
            null,
            propertyChanged: OnWiringChanged);

        /// <summary>
        /// The validation to wire to. Without it, the control's binding context is asked: a <see cref="BaseViewModel"/>
        /// provides its <see cref="BaseViewModel.Validation"/>, a <see cref="ViewModelValidation"/> is used as it is.
        /// </summary>
        public static readonly BindableProperty SourceProperty = BindableProperty.CreateAttached(
            "Source",
            typeof(ViewModelValidation),
            typeof(Validation),
            null,
            propertyChanged: OnWiringChanged);

        public static readonly BindableProperty ShowErrorLineProperty = BindableProperty.CreateAttached(
            "ShowErrorLine",
            typeof(bool),
            typeof(Validation),
            true,
            propertyChanged: OnWiringChanged);

        private static readonly BindableProperty AttachmentProperty = BindableProperty.CreateAttached(
            "Attachment",
            typeof(ValidationAttachment),
            typeof(Validation),
            null);

        public static string? GetPropertyName(BindableObject view)
        {
            return (string?)view.GetValue(PropertyNameProperty);
        }

        public static void SetPropertyName(BindableObject view, string? value)
        {
            view.SetValue(PropertyNameProperty, value);
        }

        public static ViewModelValidation? GetSource(BindableObject view)
        {
            return (ViewModelValidation?)view.GetValue(SourceProperty);
        }

        public static void SetSource(BindableObject view, ViewModelValidation? value)
        {
            view.SetValue(SourceProperty, value);
        }

        public static bool GetShowErrorLine(BindableObject view)
        {
            return (bool)view.GetValue(ShowErrorLineProperty);
        }

        public static void SetShowErrorLine(BindableObject view, bool value)
        {
            view.SetValue(ShowErrorLineProperty, value);
        }

        /// <summary>
        /// The validation a control is wired to: the explicit <see cref="SourceProperty"/> if set, else the one its
        /// binding context provides.
        /// </summary>
        public static ViewModelValidation? ResolveForm(object? bindingContext, ViewModelValidation? source)
        {
            if (source != null)
            {
                return source;
            }

            return bindingContext switch
            {
                BaseViewModel viewModel => viewModel.Validation,
                ViewModelValidation validation => validation,
                _ => null,
            };
        }

        /// <summary>The field state a control is wired to, or <c>null</c> when it is not wired (yet).</summary>
        public static FieldState? GetField(BindableObject view)
        {
            return ((ValidationAttachment?)view.GetValue(AttachmentProperty))?.Field;
        }

        private static void OnWiringChanged(BindableObject bindable, object? oldValue, object? newValue)
        {
            if (bindable is not VisualElement element)
            {
                return;
            }

            var attachment = (ValidationAttachment?)element.GetValue(AttachmentProperty);
            if (attachment == null)
            {
                attachment = new ValidationAttachment(element);
                element.SetValue(AttachmentProperty, attachment);
            }

            attachment.Rewire();
        }

        /// <summary>
        /// The live wiring between one element and one field. The form reaches it through weak subscriptions only, so
        /// that a view model which outlives the page never keeps the page alive; it is released when the element unloads
        /// and wired again when it loads.
        /// </summary>
        internal sealed class ValidationAttachment
        {
            private readonly VisualElement element;
            private readonly PropertyOverlay hint = new PropertyOverlay(SemanticProperties.HintProperty, followsOwnValue: true);
            private readonly PropertyOverlay lineColor = new PropertyOverlay(LineColorEffect.LineColorProperty, followsOwnValue: false);
            private ViewModelValidation? form;
            private FieldState? field;
            private VisualElement? input;
            private string? propertyName;
            private IDisposable? fieldSubscription;
            private IDisposable? focusSubscription;

            public ValidationAttachment(VisualElement element)
            {
                this.element = element;
                this.element.BindingContextChanged += (_, _) => this.Rewire();
                this.element.Loaded += (_, _) => this.Rewire();
                this.element.Unloaded += (_, _) => this.Release();
            }

            public FieldState? Field => this.field;

            public void Rewire()
            {
                this.Release();

                this.propertyName = GetPropertyName(this.element);
                if (string.IsNullOrWhiteSpace(this.propertyName))
                {
                    return;
                }

                this.form = ResolveForm(this.element.BindingContext, GetSource(this.element));
                if (this.form == null)
                {
                    return;
                }

                this.field = this.form.Fields[this.propertyName];
                this.input = this.element is IValidatableControl control ? control.InputElement : this.element;

                this.input.Unfocused += this.OnUnfocused;

                var wiredForm = this.form;
                this.focusSubscription = WeakSubscription.ToEvent<ValidationAttachment, FocusRequestedEventArgs>(
                    handler => wiredForm.FocusRequested += handler,
                    handler => wiredForm.FocusRequested -= handler,
                    this,
                    static (attachment, e) => attachment.OnFocusRequested(e));
                this.fieldSubscription = WeakSubscription.ToPropertyChanged(
                    this.field,
                    this,
                    static (attachment, e) => attachment.OnFieldChanged(e));

                if (this.element is IValidatableControl validatableControl)
                {
                    validatableControl.Field = this.field;
                }

                this.Apply();
            }

            private void Release()
            {
                this.fieldSubscription?.Dispose();
                this.focusSubscription?.Dispose();
                this.fieldSubscription = null;
                this.focusSubscription = null;

                if (this.input != null)
                {
                    this.input.Unfocused -= this.OnUnfocused;
                    this.RestoreInput(this.input);
                }

                if (this.element is IValidatableControl validatableControl && this.field != null && ReferenceEquals(validatableControl.Field, this.field))
                {
                    validatableControl.Field = null;
                }

                this.form = null;
                this.field = null;
                this.input = null;
            }

            private void OnUnfocused(object? sender, FocusEventArgs e)
            {
                if (this.form != null && this.propertyName != null)
                {
                    this.form.MarkTouched(this.propertyName);
                }
            }

            private void OnFocusRequested(FocusRequestedEventArgs e)
            {
                if (!string.Equals(e.PropertyName, this.propertyName, StringComparison.Ordinal) || this.input == null)
                {
                    return;
                }

                UiThread.Run(this.element, this.BringIntoView);
            }

            private void OnFieldChanged(PropertyChangedEventArgs e)
            {
                if (e.PropertyName is nameof(FieldState.FirstMessage) or nameof(FieldState.HasErrors) or nameof(FieldState.Messages))
                {
                    UiThread.Run(this.element, this.Apply);
                }
            }

            /// <summary>
            /// Shows the first invalid field: scrolls it into view and focuses it when it takes text. A picker is not
            /// focused, since focusing one opens it.
            /// </summary>
            private void BringIntoView()
            {
                if (this.input == null)
                {
                    return;
                }

                if (FindAncestor<ScrollView>(this.element) is { } scrollView)
                {
                    _ = ScrollIntoViewAsync(scrollView, this.element);
                }

                if (this.input is InputView)
                {
                    this.input.Focus();
                }
            }

            /// <summary>
            /// While the field has an error: announces it after the control's own hint and colours the underline. Neither
            /// is written while the field is valid, so that the control's own values — bound ones too — stay in charge.
            /// </summary>
            private void Apply()
            {
                if (this.input == null || this.field == null)
                {
                    return;
                }

                var message = this.field.FirstMessage;
                if (string.IsNullOrEmpty(message))
                {
                    this.hint.Remove(this.input);
                }
                else
                {
                    this.hint.Show(this.input, ownHint => ComposeHint((string?)ownHint, message));
                }

                // The library's own controls colour their underline themselves, from the field they were handed.
                if (this.element is IValidatableControl || !GetShowErrorLine(this.element) || !SupportsErrorLine(this.input))
                {
                    return;
                }

                if (this.field.HasErrors)
                {
                    var errorColor = ResourceColors.Error(Colors.Red);
                    this.lineColor.Show(this.input, _ => errorColor);
                }
                else
                {
                    this.lineColor.Remove(this.input);
                }
            }

            private void RestoreInput(VisualElement target)
            {
                this.hint.Remove(target);
                this.lineColor.Remove(target);
            }

            internal static string? ComposeHint(string? ownHint, string? message)
            {
                if (string.IsNullOrEmpty(message))
                {
                    return ownHint;
                }

                if (string.IsNullOrWhiteSpace(ownHint))
                {
                    return message;
                }

                var trimmed = ownHint.TrimEnd();
                var separator = trimmed.EndsWith('.') || trimmed.EndsWith('!') || trimmed.EndsWith('?') ? " " : ". ";
                return trimmed + separator + message;
            }

            private static bool SupportsErrorLine(VisualElement input)
            {
                return input is Entry or Editor or Picker or DatePicker or TimePicker;
            }

            private static TAncestor? FindAncestor<TAncestor>(Element element)
                where TAncestor : Element
            {
                for (var parent = element.Parent; parent != null; parent = parent.Parent)
                {
                    if (parent is TAncestor match)
                    {
                        return match;
                    }
                }

                return null;
            }

            private static async Task ScrollIntoViewAsync(ScrollView scrollView, Element element)
            {
                try
                {
                    await scrollView.ScrollToAsync(element, ScrollToPosition.MakeVisible, animated: true);
                }
                catch (Exception ex)
                {
                    // Scrolling is a courtesy: a page that cannot scroll right now must not fail the validation.
                    Trace.WriteLine($"Scrolling the first invalid field into view failed: {ex.Message}");
                }
            }

            /// <summary>
            /// A value shown on a control over the control's own one, and taken away without a trace. MAUI keeps a value
            /// set in code or XAML in the slot the overlay is written to, and a bound, styled or dynamic value beneath it;
            /// a visual state or trigger set on the element ranks above both. What a slot holds shows only by clearing
            /// it, so every decision is made by clearing: removing the overlay lets a bound or styled value show through
            /// again, with its latest value, and puts one set in code or XAML back. A value bound two-way is not
            /// supported, since the overlay would flow back to its source; neither is a value set in code which a visual
            /// state covers when the overlay first shows — clearing removes it unseen.
            /// </summary>
            private sealed class PropertyOverlay
            {
                private readonly BindableProperty property;
                private readonly bool followsOwnValue;
                private bool isShown;
                private object? ownValue;
                private bool ownValueSetInCode;
                private object? shownValue;

                /// <param name="property">The property the overlay is written to.</param>
                /// <param name="followsOwnValue">
                /// Whether the value shown is made from the own value, as the hint is: a bound own value is then read
                /// again whenever the overlay is written, so that it follows a new language.
                /// </param>
                public PropertyOverlay(BindableProperty property, bool followsOwnValue)
                {
                    this.property = property;
                    this.followsOwnValue = followsOwnValue;
                }

                public void Show(BindableObject target, Func<object?, object?> valueOver)
                {
                    var visible = target.GetValue(this.property);
                    if (!this.isShown)
                    {
                        // A value set in code or XAML goes with the clearing; one bound, styled or ranked above stays.
                        target.ClearValue(this.property);
                        this.ownValue = visible;
                        this.ownValueSetInCode = !Equals(target.GetValue(this.property), visible);
                    }
                    else if (!Equals(visible, this.shownValue))
                    {
                        // Something else shows: the app set a value in code, which the clearing takes away and which is
                        // the own value from now on, or a visual state or trigger covers the overlay for now.
                        target.ClearValue(this.property);
                        if (!Equals(target.GetValue(this.property), visible))
                        {
                            this.ownValue = visible;
                            this.ownValueSetInCode = true;
                        }
                    }
                    else if (this.followsOwnValue && !this.ownValueSetInCode)
                    {
                        // A bound own value may have moved on underneath, to a new language say: read it again.
                        target.ClearValue(this.property);
                        this.ownValue = target.GetValue(this.property);
                    }

                    this.shownValue = valueOver(this.ownValue);
                    this.isShown = true;
                    target.SetValue(this.property, this.shownValue);
                }

                public void Remove(BindableObject target)
                {
                    if (!this.isShown)
                    {
                        return;
                    }

                    this.isShown = false;
                    var visible = target.GetValue(this.property);
                    target.ClearValue(this.property);

                    if (!Equals(visible, this.shownValue) && !Equals(target.GetValue(this.property), visible))
                    {
                        // The app set a value in code while the overlay showed, and the clearing took it away: it goes back.
                        target.SetValue(this.property, visible);
                    }
                    else if (this.ownValueSetInCode)
                    {
                        // The own value was set in code or XAML: it goes back. A bound or styled one shows by itself.
                        target.SetValue(this.property, this.ownValue);
                    }
                }
            }
        }
    }
}
