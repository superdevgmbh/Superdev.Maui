using System.Diagnostics;
using System.Windows.Input;

namespace Superdev.Maui.Controls
{
    public partial class ValidatableEntry : Grid
    {
        public ValidatableEntry()
        {
            this.InitializeComponent();
            this.DebugLayoutBounds();
        }

        [Conditional("DEBUG")]
        private void DebugLayoutBounds()
        {
            if (!DebugHelper.ShowLayoutBounds)
            {
                return;
            }

            this.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentBright");
            this.AnnotationLabel.SetValue(VisualElement.BackgroundColorProperty, Colors.Yellow);
            this.Entry.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentDark");
            this.ReadonlyLabel.SetDynamicResource(VisualElement.BackgroundColorProperty, "Theme.Color.SemiTransparentDark");
        }

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(
                nameof(Text),
                typeof(string),
                typeof(ValidatableEntry),
                null,
                BindingMode.TwoWay,
                propertyChanged: OnTextPropertyChanged);

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = (ValidatableEntry)bindable;
            entry.OnPropertyChanged(nameof(entry.AnnotationText));
        }

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(
                nameof(Placeholder),
                typeof(string),
                typeof(ValidatableEntry),
                propertyChanged: OnPlaceholderPropertyChanged);

        public string Placeholder
        {
            get => (string)this.GetValue(PlaceholderProperty);
            set => this.SetValue(PlaceholderProperty, value);
        }

        private static void OnPlaceholderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = (ValidatableEntry)bindable;
            entry.OnPropertyChanged(nameof(entry.AnnotationText));
        }

        public string AnnotationText
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Text))
                {
                    return this.Placeholder;
                }

                return " ";
            }
        }

        public static readonly BindableProperty IsReadonlyProperty =
            BindableProperty.Create(
                nameof(IsReadonly),
                typeof(bool),
                typeof(ValidatableEntry),
                false);

        public bool IsReadonly
        {
            get => (bool)this.GetValue(IsReadonlyProperty);
            set => this.SetValue(IsReadonlyProperty, value);
        }

        public static readonly BindableProperty KeyboardProperty =
            BindableProperty.Create(
                nameof(Keyboard),
                typeof(Keyboard),
                typeof(ValidatableEntry),
                Keyboard.Default);

        public Keyboard Keyboard
        {
            get => (Keyboard)this.GetValue(KeyboardProperty);
            set => this.SetValue(KeyboardProperty, value);
        }

        public static readonly BindableProperty IsPasswordProperty =
            BindableProperty.Create(
                nameof(IsPassword),
                typeof(bool),
                typeof(ValidatableEntry),
                false);

        public bool IsPassword
        {
            get => (bool)this.GetValue(IsPasswordProperty);
            set => this.SetValue(IsPasswordProperty, value);
        }

        public static readonly BindableProperty IsEntryFocusedProperty =
            BindableProperty.Create(
                nameof(IsEntryFocused),
                typeof(bool),
                typeof(ValidatableEntry),
                false,
                BindingMode.TwoWay,
                propertyChanged: OnIsFocusedPropertyChanged);

        private static void OnIsFocusedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var validatableEntry = (ValidatableEntry)bindable;
            if (newValue is bool isFocused && isFocused)
            {
                validatableEntry.Entry.Focus();
            }
            else
            {
                validatableEntry.Entry.Unfocus();
            }
        }

        public bool IsEntryFocused
        {
            get => (bool)this.GetValue(IsEntryFocusedProperty);
            set => this.SetValue(IsEntryFocusedProperty, value);
        }

        public static readonly BindableProperty AnnotationLabelStyleProperty =
            BindableProperty.Create(
                nameof(AnnotationLabelStyle),
                typeof(Style),
                typeof(ValidatableEntry),
                null);

        public Style AnnotationLabelStyle
        {
            get => (Style)this.GetValue(AnnotationLabelStyleProperty);
            set => this.SetValue(AnnotationLabelStyleProperty, value);
        }

        public static readonly BindableProperty ReadonlyLabelStyleProperty =
            BindableProperty.Create(
                nameof(ReadonlyLabelStyle),
                typeof(Style),
                typeof(ValidatableEntry));

        public Style ReadonlyLabelStyle
        {
            get => (Style)this.GetValue(ReadonlyLabelStyleProperty);
            set => this.SetValue(ReadonlyLabelStyleProperty, value);
        }

        public static readonly BindableProperty ValidationErrorLabelStyleProperty =
            BindableProperty.Create(
                nameof(ValidationErrorLabelStyle),
                typeof(Style),
                typeof(ValidatableEntry),
                null);

        public Style ValidationErrorLabelStyle
        {
            get => (Style)this.GetValue(ValidationErrorLabelStyleProperty);
            set => this.SetValue(ValidationErrorLabelStyleProperty, value);
        }

        public static readonly BindableProperty EntryStyleProperty =
            BindableProperty.Create(
                nameof(EntryStyle),
                typeof(Style),
                typeof(ValidatableEntry));

        public Style EntryStyle
        {
            get => (Style)this.GetValue(EntryStyleProperty);
            set => this.SetValue(EntryStyleProperty, value);
        }

        public static readonly BindableProperty ReturnTypeProperty =
            BindableProperty.Create(
                nameof(ReturnType),
                typeof(ReturnType),
                typeof(ValidatableEntry),
                default(ReturnType));

        public ReturnType ReturnType
        {
            get => (ReturnType)this.GetValue(ReturnTypeProperty);
            set => this.SetValue(ReturnTypeProperty, value);
        }

        public static readonly BindableProperty ReturnCommandProperty =
            BindableProperty.Create(
                nameof(ReturnCommand),
                typeof(ICommand),
                typeof(ValidatableEntry));

        public ICommand ReturnCommand
        {
            get => (ICommand)this.GetValue(ReturnCommandProperty);
            set => this.SetValue(ReturnCommandProperty, value);
        }

        public static readonly BindableProperty MaxLengthProperty =
            BindableProperty.Create(
                nameof(MaxLength),
                typeof(int),
                typeof(ValidatableEntry),
                int.MaxValue);

        public int MaxLength
        {
            get => (int)this.GetValue(MaxLengthProperty);
            set => this.SetValue(MaxLengthProperty, value);
        }

        public static readonly BindableProperty ValidationErrorsProperty =
            BindableProperty.Create(
                nameof(ValidationErrors),
                typeof(IEnumerable<string>),
                typeof(ValidatableEntry));

        public IEnumerable<string> ValidationErrors
        {
            get => (IEnumerable<string>)this.GetValue(ValidationErrorsProperty);
            set => this.SetValue(ValidationErrorsProperty, value);
        }

        public static readonly BindableProperty TrailingIconProperty =
            BindableProperty.Create(
                nameof(TrailingIcon),
                typeof(ImageSource),
                typeof(ValidatableEntry));

        public ImageSource TrailingIcon
        {
            get => (ImageSource)this.GetValue(TrailingIconProperty);
            set => this.SetValue(TrailingIconProperty, value);
        }

        public static readonly BindableProperty TrailingIconCommandProperty =
            BindableProperty.Create(
                nameof(TrailingIconCommand),
                typeof(ICommand),
                typeof(ValidatableEntry));

        public ICommand TrailingIconCommand
        {
            get => (ICommand)this.GetValue(TrailingIconCommandProperty);
            set => this.SetValue(TrailingIconCommandProperty, value);
        }

        public static readonly BindableProperty TrailingIconCommandParameterProperty =
            BindableProperty.Create(
                nameof(TrailingIconCommandParameter),
                typeof(object),
                typeof(ValidatableEntry));

        public object TrailingIconCommandParameter
        {
            get => this.GetValue(TrailingIconCommandParameterProperty);
            set => this.SetValue(TrailingIconCommandParameterProperty, value);
        }

        public static readonly BindableProperty TextContentTypeProperty =
            BindableProperty.Create(
                nameof(TextContentType),
                typeof(TextContentType),
                typeof(ValidatableEntry),
                default(TextContentType));

        public TextContentType TextContentType
        {
            get => (TextContentType)this.GetValue(TextContentTypeProperty);
            set => this.SetValue(TextContentTypeProperty, value);
        }

        public event EventHandler Completed
        {
            add => this.Entry.Completed += value;
            remove => this.Entry.Completed -= value;
        }

        public new event EventHandler<FocusEventArgs> Focused
        {
            add => this.Entry.Focused += value;
            remove => this.Entry.Focused -= value;
        }

        public new event EventHandler<FocusEventArgs> Unfocused
        {
            add => this.Entry.Unfocused += value;
            remove => this.Entry.Unfocused -= value;
        }

        public event EventHandler<TextChangedEventArgs> TextChanged
        {
            add => this.Entry.TextChanged += value;
            remove => this.Entry.TextChanged -= value;
        }

        private void Entry_OnFocused(object sender, FocusEventArgs e)
        {
            this.IsEntryFocused = true;
        }

        private void Entry_OnUnfocused(object sender, FocusEventArgs e)
        {
            this.IsEntryFocused = false;
        }
    }
}