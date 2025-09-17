using System.Globalization;

namespace Superdev.Maui.Controls
{
    public partial class ValidatableDateTimePicker : Grid
    {
        public ValidatableDateTimePicker()
        {
            this.InitializeComponent();
            this.DebugLayoutBounds();
        }

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(
                nameof(Placeholder),
                typeof(string),
                typeof(ValidatableDateTimePicker),
                propertyChanged: OnPlaceholderPropertyChanged);

        private static void OnPlaceholderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ValidatableDateTimePicker)bindable;
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
                typeof(ValidatableDateTimePicker),
                defaultBindingMode: BindingMode.TwoWay,
                propertyChanged: OnDatePropertyChanged);

        private static void OnDatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var validatableDateTimePicker = (ValidatableDateTimePicker)bindable;
            validatableDateTimePicker.OnPropertyChanged(nameof(validatableDateTimePicker.AnnotationText));
            validatableDateTimePicker.OnPropertyChanged(nameof(validatableDateTimePicker.ReadonlyText));
        }

        public DateTime? Date
        {
            get => (DateTime?)this.GetValue(DateProperty);
            set => this.SetValue(DateProperty, value);
        }

        public static readonly BindableProperty TimeProperty =
            BindableProperty.Create(
                nameof(Time),
                typeof(TimeSpan?),
                typeof(ValidatableDateTimePicker),
                null,
                BindingMode.TwoWay,
                null,
                OnTimePropertyChanged);

        private static void OnTimePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var validatableDateTimePicker = (ValidatableDateTimePicker)bindable;
            validatableDateTimePicker.OnPropertyChanged(nameof(validatableDateTimePicker.AnnotationText));
        }

        public TimeSpan? Time
        {
            get => (TimeSpan?)this.GetValue(TimeProperty);
            set => this.SetValue(TimeProperty, value);
        }

        public static readonly BindableProperty ValidityRangeProperty =
            BindableProperty.Create(
                nameof(ValidityRange),
                typeof(DateRange),
                typeof(ValidatableDateTimePicker),
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
                typeof(ValidatableDateTimePicker),
                false,
                propertyChanged: OnIsReadonlyPropertyChanged);

        private static void OnIsReadonlyPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (ValidatableDateTimePicker)bindable;
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
                typeof(ValidatableDateTimePicker));

        public string ReadonlyText
        {
            get
            {
                var readonlyText = (string)this.GetValue(ReadonlyTextProperty);
                if (readonlyText == null && this.Date is DateTime date)
                {
                    // if (this.Time is TimeSpan time)
                    // {
                    //     date += time;
                    // }

                    // In case ReadonlyText is null, we try to take Format(Date+Time) as ReadonlyText
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
                typeof(ValidatableDateTimePicker));

        public Style AnnotationLabelStyle
        {
            get => (Style)this.GetValue(AnnotationLabelStyleProperty);
            set => this.SetValue(AnnotationLabelStyleProperty, value);
        }

        public static readonly BindableProperty DatePickerStyleProperty =
            BindableProperty.Create(
                nameof(DatePickerStyle),
                typeof(Style),
                typeof(ValidatableDateTimePicker));

        public Style DatePickerStyle
        {
            get => (Style)this.GetValue(DatePickerStyleProperty);
            set => this.SetValue(DatePickerStyleProperty, value);
        }

        public static readonly BindableProperty TimePickerStyleProperty =
            BindableProperty.Create(
                nameof(TimePickerStyle),
                typeof(Style),
                typeof(ValidatableDateTimePicker));

        public Style TimePickerStyle
        {
            get => (Style)this.GetValue(TimePickerStyleProperty);
            set => this.SetValue(TimePickerStyleProperty, value);
        }

        public static readonly BindableProperty ReadonlyLabelStyleProperty =
            BindableProperty.Create(
                nameof(ReadonlyLabelStyle),
                typeof(Style),
                typeof(ValidatableDateTimePicker));

        public Style ReadonlyLabelStyle
        {
            get => (Style)this.GetValue(ReadonlyLabelStyleProperty);
            set => this.SetValue(ReadonlyLabelStyleProperty, value);
        }

        public static readonly BindableProperty ValidationErrorLabelStyleProperty =
            BindableProperty.Create(
                nameof(ValidationErrorLabelStyle),
                typeof(Style),
                typeof(ValidatableDateTimePicker));

        public Style ValidationErrorLabelStyle
        {
            get => (Style)this.GetValue(ValidationErrorLabelStyleProperty);
            set => this.SetValue(ValidationErrorLabelStyleProperty, value);
        }

        public static readonly BindableProperty ValidationErrorsProperty =
            BindableProperty.Create(
                nameof(ValidationErrors),
                typeof(IEnumerable<string>),
                typeof(ValidatableDateTimePicker));

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
                // There is no way to forward attached bindable properties from ValidatableDateTimePicker to the
                // internal NullableDatePicker control. Thus, we call SetValue on the NullableDatePicker and forward the value.
                this.DatePicker?.SetValue(DialogExtensions.NeutralButtonTextProperty, DialogExtensions.GetNeutralButtonText(this));
                this.TimePicker?.SetValue(DialogExtensions.NeutralButtonTextProperty, DialogExtensions.GetNeutralButtonText(this));
            }
        }
    }
}