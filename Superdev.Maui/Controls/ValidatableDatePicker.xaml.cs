using System.Globalization;

namespace Superdev.Maui.Controls
{
    public partial class ValidatableDatePicker : Grid
    {
        public ValidatableDatePicker()
        {
            this.InitializeComponent();
            this.DebugLayoutBounds();
        }

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(
                nameof(Placeholder),
                typeof(string),
                typeof(ValidatableDatePicker),
                propertyChanged: OnPlaceholderPropertyChanged);

        private static void OnPlaceholderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ValidatableDatePicker)bindable;
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
                if (this.Date != null || this.IsReadonly)
                {
                    return this.Placeholder;
                }

                return " ";
            }
        }

        public static readonly BindableProperty DateProperty =
            BindableProperty.Create(
                nameof(Date),
                typeof(DateTime?),
                typeof(ValidatableDatePicker),
                defaultBindingMode: BindingMode.TwoWay,
                propertyChanged: OnDatePropertyChanged);

        private static void OnDatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            //Debug.WriteLine($"OnDatePropertyChanged: oldValue={oldValue}, newValue={newValue}");

            var validatableDatePicker = (ValidatableDatePicker)bindable;
            validatableDatePicker.OnPropertyChanged(nameof(validatableDatePicker.AnnotationText));
            validatableDatePicker.OnPropertyChanged(nameof(validatableDatePicker.ReadonlyText));
        }

        public DateTime? Date
        {
            get => (DateTime?)this.GetValue(DateProperty);
            set => this.SetValue(DateProperty, value);
        }

        public static readonly BindableProperty ValidityRangeProperty =
            BindableProperty.Create(
                nameof(ValidityRange),
                typeof(DateRange),
                typeof(ValidatableDatePicker),
                defaultBindingMode: BindingMode.OneWay,
                propertyChanged: OnValidityRangePropertyChanged);

        private static void OnValidityRangePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // Debug.WriteLine($"OnValidityRangePropertyChanged: oldValue={oldValue}, newValue={newValue}");
        }

        public DateRange ValidityRange
        {
            get => (DateRange)this.GetValue(ValidityRangeProperty);
            set => this.SetValue(ValidityRangeProperty, value);
        }

        public static readonly BindableProperty IsReadonlyProperty =
            BindableProperty.Create(
                nameof(IsReadonly),
                typeof(bool),
                typeof(ValidatableDatePicker),
                false,
                propertyChanged: OnIsReadonlyPropertyChanged);

        private static void OnIsReadonlyPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ValidatableDatePicker)bindable;
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
                typeof(ValidatableDatePicker));

        public string ReadonlyText
        {
            get
            {
                var readonlyText = (string)this.GetValue(ReadonlyTextProperty);
                if (readonlyText == null && this.Date is DateTime date)
                {
                    // In case ReadonlyText is null, we try to take Date+Format as ReadonlyText
                    var localDateTime = date.ToLocalTime();

                    if (this.DatePicker?.Format is string dateFormat && !string.IsNullOrEmpty(dateFormat))
                    {
                        readonlyText = localDateTime.ToString(dateFormat);
                    }
                    else
                    {
                        readonlyText = localDateTime.ToString(CultureInfo.CurrentCulture);
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
                typeof(ValidatableDatePicker));

        public Style AnnotationLabelStyle
        {
            get => (Style)this.GetValue(AnnotationLabelStyleProperty);
            set => this.SetValue(AnnotationLabelStyleProperty, value);
        }

        public static readonly BindableProperty DatePickerStyleProperty =
            BindableProperty.Create(
                nameof(DatePickerStyle),
                typeof(Style),
                typeof(ValidatableDatePicker));

        public Style DatePickerStyle
        {
            get => (Style)this.GetValue(DatePickerStyleProperty);
            set => this.SetValue(DatePickerStyleProperty, value);
        }

        public static readonly BindableProperty ReadonlyLabelStyleProperty =
            BindableProperty.Create(
                nameof(ReadonlyLabelStyle),
                typeof(Style),
                typeof(ValidatableDatePicker));

        public Style ReadonlyLabelStyle
        {
            get => (Style)this.GetValue(ReadonlyLabelStyleProperty);
            set => this.SetValue(ReadonlyLabelStyleProperty, value);
        }

        public static readonly BindableProperty ValidationErrorLabelStyleProperty =
            BindableProperty.Create(
                nameof(ValidationErrorLabelStyle),
                typeof(Style),
                typeof(ValidatableDatePicker));

        public Style ValidationErrorLabelStyle
        {
            get => (Style)this.GetValue(ValidationErrorLabelStyleProperty);
            set => this.SetValue(ValidationErrorLabelStyleProperty, value);
        }

        public static readonly BindableProperty ValidationErrorsProperty =
            BindableProperty.Create(
                nameof(ValidationErrors),
                typeof(IEnumerable<string>),
                typeof(ValidatableDatePicker));

        public IEnumerable<string> ValidationErrors
        {
            get => (IEnumerable<string>)this.GetValue(ValidationErrorsProperty);
            set => this.SetValue(ValidationErrorsProperty, value);
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(this.DatePicker.Format))
            {
                base.OnPropertyChanged(nameof(this.ReadonlyText));
            }
            else if (propertyName == nameof(this.Date))
            {
                base.OnPropertyChanged(nameof(this.ReadonlyText));
            }
            else if (propertyName == DialogExtensions.NeutralButtonTextProperty.PropertyName)
            {
                // There is no way to forward attached bindable properties from ValidatableDatePicker to the
                // internal NullableDatePicker control. Thus, we call SetValue on the NullableDatePicker and forward the value.
                this.DatePicker?.SetValue(DialogExtensions.NeutralButtonTextProperty, DialogExtensions.GetNeutralButtonText(this));
            }
        }
    }
}