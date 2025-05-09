using System.Collections;
using System.Diagnostics;
using Superdev.Maui.Utils;

namespace Superdev.Maui.Controls
{
    public partial class ValidatablePicker : Grid
    {
        private const int SelectedIndexDefaultValue = -1;

        public ValidatablePicker()
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

            this.SetValue(VisualElement.BackgroundColorProperty, Colors.Aqua);
            this.AnnotationLabel.SetValue(VisualElement.BackgroundColorProperty, Colors.Yellow);
            this.Picker.SetValue(VisualElement.BackgroundColorProperty, Colors.LightPink);
            this.ReadonlyLabel.SetValue(VisualElement.BackgroundColorProperty, Colors.Fuchsia);
        }

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(
                nameof(Placeholder),
                typeof(string),
                typeof(ValidatablePicker),
                null,
                BindingMode.OneWay,
                null,
                OnPlaceholderPropertyChanged);

        private static void OnPlaceholderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ValidatablePicker)bindable;
            picker.OnPropertyChanged(nameof(picker.AnnotationText));
        }

        public string Placeholder
        {
            get => (string)this.GetValue(PlaceholderProperty);
            set => this.SetValue(PlaceholderProperty, value);
        }

        public string AnnotationText
        {
            get
            {
                if (this.SelectedIndex != SelectedIndexDefaultValue || this.IsReadonly)
                {
                    return this.Placeholder;
                }

                return " ";
            }
        }

        public static readonly BindableProperty PickerStyleProperty =
            BindableProperty.Create(
                nameof(PickerStyle),
                typeof(Style),
                typeof(ValidatablePicker));

        public Style PickerStyle
        {
            get => (Style)this.GetValue(PickerStyleProperty);
            set => this.SetValue(PickerStyleProperty, value);
        }

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(ValidatablePicker),
                propertyChanged: OnItemsSourcePropertyChanged);

        private static void OnItemsSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ValidatablePicker)bindable;
            picker.OnPropertyChanged(nameof(picker.Placeholder));
            picker.OnPropertyChanged(nameof(picker.AnnotationText));
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        public BindingBase ItemDisplayBinding
        {
            get => this.Picker.ItemDisplayBinding;
            set => this.Picker.ItemDisplayBinding = value;
        }

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(
                nameof(SelectedItem),
                typeof(object),
                typeof(ValidatablePicker),
                null,
                BindingMode.TwoWay,
                null,
                OnSelectedItemPropertyChanged);

        private static void OnSelectedItemPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ValidatablePicker)bindable;
            picker.OnPropertyChanged(nameof(picker.Placeholder));
            picker.OnPropertyChanged(nameof(picker.ReadonlyText));
        }

        public object SelectedItem
        {
            get => this.GetValue(SelectedItemProperty);
            set
            {
                this.SetValue(SelectedItemProperty, value);
                this.OnPropertyChanged(nameof(this.AnnotationText));
            }
        }

        public static readonly BindableProperty SelectedValueProperty =
            BindableProperty.Create(
                nameof(SelectedValue),
                typeof(object),
                typeof(ValidatablePicker),
                null,
                BindingMode.TwoWay);

        public object SelectedValue
        {
            get => this.GetValue(SelectedValueProperty);
            set => this.SetValue(SelectedValueProperty, value);
        }

        public static readonly BindableProperty SelectedValuePathProperty =
            BindableProperty.Create(
                nameof(SelectedValuePath),
                typeof(string),
                typeof(ValidatablePicker));

        public string SelectedValuePath
        {
            get => (string)this.GetValue(SelectedValuePathProperty);
            set => this.SetValue(SelectedValuePathProperty, value);
        }

        public static readonly BindableProperty SelectedIndexProperty =
            BindableProperty.Create(
                nameof(SelectedIndex),
                typeof(int),
                typeof(ValidatablePicker),
                SelectedIndexDefaultValue,
                BindingMode.TwoWay,
                propertyChanged: OnSelectedIndexPropertyChanged);

        public int SelectedIndex
        {
            get => (int)this.GetValue(SelectedIndexProperty);
            set => this.SetValue(SelectedIndexProperty, value);
        }

        private static void OnSelectedIndexPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ValidatablePicker)bindable;
            picker.OnPropertyChanged(nameof(picker.AnnotationText));
        }

        public static readonly BindableProperty IsReadonlyProperty =
            BindableProperty.Create(
                nameof(IsReadonly),
                typeof(bool),
                typeof(ValidatablePicker),
                false,
                propertyChanged: OnIsReadonlyPropertyChanged);

        private static void OnIsReadonlyPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ValidatablePicker)bindable;
            picker.OnPropertyChanged(nameof(picker.AnnotationText));
        }

        public bool IsReadonly
        {
            get => (bool)this.GetValue(IsReadonlyProperty);
            set => this.SetValue(IsReadonlyProperty, value);
        }

        public static readonly BindableProperty ReadonlyTextProperty =
            BindableProperty.Create(
                nameof(ReadonlyText),
                typeof(string),
                typeof(ValidatablePicker));

        public string ReadonlyText
        {
            get
            {
                var readonlyText = (string)this.GetValue(ReadonlyTextProperty);
                if (readonlyText == null && this.SelectedItem is object selectedItem)
                {
                    if (this.ItemDisplayBinding is Binding { Path: var path } &&
                        path != Binding.SelfPath && path != nameof(this.ItemDisplayBinding))
                    {
                        var selectedItemValue = ReflectionHelper.GetPropertyValue(selectedItem, path);
                        readonlyText = selectedItemValue?.ToString();
                    }
                    else
                    {
                        // In case readonly text is null, we try to take SelectedItem as ReadonlyText
                        readonlyText = selectedItem.ToString();
                    }
                }

                return readonlyText;
            }
            set => this.SetValue(ReadonlyTextProperty, value);
        }

        public static readonly BindableProperty AnnotationLabelStyleProperty =
            BindableProperty.Create(
                nameof(AnnotationLabelStyle),
                typeof(Style),
                typeof(ValidatablePicker));

        public Style AnnotationLabelStyle
        {
            get => (Style)this.GetValue(AnnotationLabelStyleProperty);
            set => this.SetValue(AnnotationLabelStyleProperty, value);
        }

        public static readonly BindableProperty ReadonlyLabelStyleProperty =
            BindableProperty.Create(
                nameof(ReadonlyLabelStyle),
                typeof(Style),
                typeof(ValidatablePicker));

        public Style ReadonlyLabelStyle
        {
            get => (Style)this.GetValue(ReadonlyLabelStyleProperty);
            set => this.SetValue(ReadonlyLabelStyleProperty, value);
        }

        public static readonly BindableProperty ValidationErrorLabelStyleProperty =
            BindableProperty.Create(
                nameof(ValidationErrorLabelStyle),
                typeof(Style),
                typeof(ValidatablePicker));

        public Style ValidationErrorLabelStyle
        {
            get => (Style)this.GetValue(ValidationErrorLabelStyleProperty);
            set => this.SetValue(ValidationErrorLabelStyleProperty, value);
        }

        public static readonly BindableProperty ValidationErrorsProperty =
            BindableProperty.Create(
                nameof(ValidationErrors),
                typeof(IEnumerable<string>),
                typeof(ValidatablePicker));

        public IEnumerable<string> ValidationErrors
        {
            get => (IEnumerable<string>)this.GetValue(ValidationErrorsProperty);
            set => this.SetValue(ValidationErrorsProperty, value);
        }
    }
}