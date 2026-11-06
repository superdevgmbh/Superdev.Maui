using System.ComponentModel;
using Superdev.Maui.Internals;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Controls
{
    /// <summary>
    /// The field wiring shared by the library's input controls: a <c>Field</c> to bind — or to receive from
    /// <see cref="Validation"/> — from which the control takes its error messages and its pending state. The field
    /// reaches the control through a weak subscription only, so a view model which outlives the page does not keep
    /// the control alive, and updates arrive on the UI thread whichever thread raised them.
    /// </summary>
    internal static class ValidatableControlField
    {
        private static readonly BindableProperty SubscriptionProperty = BindableProperty.CreateAttached(
            "FieldSubscription",
            typeof(IDisposable),
            typeof(ValidatableControlField),
            null);

        public static BindableProperty CreateFieldProperty<TControl>()
            where TControl : View, IValidatableControl
        {
            return BindableProperty.Create(
                "Field",
                typeof(FieldState),
                typeof(TControl),
                null,
                propertyChanged: (bindable, _, newValue) => OnFieldChanged((TControl)bindable, (FieldState?)newValue));
        }

        public static BindableProperty CreateIsPendingProperty<TControl>()
            where TControl : BindableObject
        {
            return BindableProperty.Create("IsPending", typeof(bool), typeof(TControl), false);
        }

        private static void OnFieldChanged<TControl>(TControl control, FieldState? newField)
            where TControl : View, IValidatableControl
        {
            ((IDisposable?)control.GetValue(SubscriptionProperty))?.Dispose();
            control.SetValue(SubscriptionProperty, null);

            if (newField != null)
            {
                var subscription = WeakSubscription.ToPropertyChanged(
                    newField,
                    control,
                    static (subscriber, e) => OnFieldPropertyChanged(subscriber, e));
                control.SetValue(SubscriptionProperty, subscription);
            }

            Apply(control, newField);
        }

        private static void OnFieldPropertyChanged<TControl>(TControl control, PropertyChangedEventArgs e)
            where TControl : View, IValidatableControl
        {
            if (e.PropertyName is nameof(FieldState.Messages) or nameof(FieldState.IsPending))
            {
                UiThread.Run(control, () => Apply(control, control.Field));
            }
        }

        private static void Apply(IValidatableControl control, FieldState? field)
        {
            control.ApplyField(field?.Messages ?? Array.Empty<string>(), field?.IsPending ?? false);
        }
    }

    public partial class ValidatableEntry : IValidatableControl
    {
        public static readonly BindableProperty FieldProperty = ValidatableControlField.CreateFieldProperty<ValidatableEntry>();

        public static readonly BindableProperty IsPendingProperty = ValidatableControlField.CreateIsPendingProperty<ValidatableEntry>();

        /// <summary>The field this control shows; set by <see cref="Validation"/>, or bound to <c>Validation.Fields[Name]</c>.</summary>
        public FieldState? Field
        {
            get => (FieldState?)this.GetValue(FieldProperty);
            set => this.SetValue(FieldProperty, value);
        }

        /// <summary>An asynchronous rule of the field is in flight.</summary>
        public bool IsPending
        {
            get => (bool)this.GetValue(IsPendingProperty);
            set => this.SetValue(IsPendingProperty, value);
        }

        VisualElement IValidatableControl.InputElement => this.Entry;

        void IValidatableControl.ApplyField(IEnumerable<string> messages, bool isPending)
        {
            this.ValidationErrors = messages;
            this.IsPending = isPending;
        }
    }

    public partial class ValidatableEditor : IValidatableControl
    {
        public static readonly BindableProperty FieldProperty = ValidatableControlField.CreateFieldProperty<ValidatableEditor>();

        public static readonly BindableProperty IsPendingProperty = ValidatableControlField.CreateIsPendingProperty<ValidatableEditor>();

        public FieldState? Field
        {
            get => (FieldState?)this.GetValue(FieldProperty);
            set => this.SetValue(FieldProperty, value);
        }

        public bool IsPending
        {
            get => (bool)this.GetValue(IsPendingProperty);
            set => this.SetValue(IsPendingProperty, value);
        }

        VisualElement IValidatableControl.InputElement => this.Editor;

        void IValidatableControl.ApplyField(IEnumerable<string> messages, bool isPending)
        {
            this.ValidationErrors = messages;
            this.IsPending = isPending;
        }
    }

    public partial class ValidatablePicker : IValidatableControl
    {
        public static readonly BindableProperty FieldProperty = ValidatableControlField.CreateFieldProperty<ValidatablePicker>();

        public static readonly BindableProperty IsPendingProperty = ValidatableControlField.CreateIsPendingProperty<ValidatablePicker>();

        public FieldState? Field
        {
            get => (FieldState?)this.GetValue(FieldProperty);
            set => this.SetValue(FieldProperty, value);
        }

        public bool IsPending
        {
            get => (bool)this.GetValue(IsPendingProperty);
            set => this.SetValue(IsPendingProperty, value);
        }

        VisualElement IValidatableControl.InputElement => this.Picker;

        void IValidatableControl.ApplyField(IEnumerable<string> messages, bool isPending)
        {
            this.ValidationErrors = messages;
            this.IsPending = isPending;
        }
    }

    public partial class ValidatableDatePicker : IValidatableControl
    {
        public static readonly BindableProperty FieldProperty = ValidatableControlField.CreateFieldProperty<ValidatableDatePicker>();

        public static readonly BindableProperty IsPendingProperty = ValidatableControlField.CreateIsPendingProperty<ValidatableDatePicker>();

        public FieldState? Field
        {
            get => (FieldState?)this.GetValue(FieldProperty);
            set => this.SetValue(FieldProperty, value);
        }

        public bool IsPending
        {
            get => (bool)this.GetValue(IsPendingProperty);
            set => this.SetValue(IsPendingProperty, value);
        }

        VisualElement IValidatableControl.InputElement => this.DatePicker;

        void IValidatableControl.ApplyField(IEnumerable<string> messages, bool isPending)
        {
            this.ValidationErrors = messages;
            this.IsPending = isPending;
        }
    }

    public partial class ValidatableDateTimePicker : IValidatableControl
    {
        public static readonly BindableProperty FieldProperty = ValidatableControlField.CreateFieldProperty<ValidatableDateTimePicker>();

        public static readonly BindableProperty IsPendingProperty = ValidatableControlField.CreateIsPendingProperty<ValidatableDateTimePicker>();

        public FieldState? Field
        {
            get => (FieldState?)this.GetValue(FieldProperty);
            set => this.SetValue(FieldProperty, value);
        }

        public bool IsPending
        {
            get => (bool)this.GetValue(IsPendingProperty);
            set => this.SetValue(IsPendingProperty, value);
        }

        VisualElement IValidatableControl.InputElement => this.DatePicker;

        void IValidatableControl.ApplyField(IEnumerable<string> messages, bool isPending)
        {
            this.ValidationErrors = messages;
            this.IsPending = isPending;
        }
    }
}
