using System.Diagnostics;

namespace Superdev.Maui.Controls
{
    /// <summary>
    /// DatePicker with <see cref="NullableDate"/>.
    /// </summary>
    public class NullableDatePicker : DatePicker
    {
        public static readonly BindableProperty NullableDateProperty =
            BindableProperty.Create(
                nameof(NullableDate),
                typeof(DateTime?),
                typeof(NullableDatePicker),
                null,
                BindingMode.TwoWay);

        public DateTime? NullableDate
        {
            get => (DateTime?)this.GetValue(NullableDateProperty);
            set => this.SetValue(NullableDateProperty, value);
        }

        public static readonly BindableProperty ValidityRangeProperty =
            BindableProperty.Create(
                nameof(ValidityRange),
                typeof(DateRange),
                typeof(NullableDatePicker),
                null,
                BindingMode.OneWay,
                null,
                OnValidityRangePropertyChanged);

        private static void OnValidityRangePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Debug.WriteLine($"OnValidityRangePropertyChanged: oldValue={oldValue}, newValue={newValue}");

            if (newValue is DateRange dateRange)
            {
                var picker = (NullableDatePicker)bindable;
                picker.MaximumDate = DateTime.MaxValue;
                picker.MinimumDate = DateTime.MinValue;

                picker.MaximumDate = dateRange.End;
                picker.MinimumDate = dateRange.Start;
            }
        }

        public DateRange ValidityRange
        {
            get => (DateRange)this.GetValue(ValidityRangeProperty);
            set => this.SetValue(ValidityRangeProperty, value);
        }

        public static readonly BindableProperty HorizontalTextAlignmentProperty =
            BindableProperty.Create(
                nameof(HorizontalTextAlignment),
                typeof(TextAlignment),
                typeof(NullableDatePicker),
                TextAlignment.Start);

        public TextAlignment HorizontalTextAlignment
        {
            get => (TextAlignment)this.GetValue(HorizontalTextAlignmentProperty);
            set => this.SetValue(HorizontalTextAlignmentProperty, value);
        }

        public static readonly BindableProperty HasBorderProperty =
            BindableProperty.Create(
                nameof(HasBorder),
                typeof(bool),
                typeof(NullableDatePicker),
                true);

        public bool HasBorder
        {
            get => (bool)this.GetValue(HasBorderProperty);
            set => this.SetValue(HasBorderProperty, value);
        }

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(
                nameof(Placeholder),
                typeof(string),
                typeof(NullableDatePicker));

        public string Placeholder
        {
            get => (string)this.GetValue(PlaceholderProperty);
            set => this.SetValue(PlaceholderProperty, value);
        }

        public static readonly BindableProperty PlaceholderColorProperty =
            BindableProperty.Create(
                nameof(PlaceholderColor),
                typeof(Color),
                typeof(NullableDatePicker),
                Colors.Transparent);

        public Color PlaceholderColor
        {
            get => (Color)this.GetValue(PlaceholderColorProperty);
            set => this.SetValue(PlaceholderColorProperty, value);
        }
    }
}