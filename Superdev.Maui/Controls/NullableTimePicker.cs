using System.Diagnostics;

namespace Superdev.Maui.Controls
{
    /// <summary>
    /// TimePicker with <see cref="NullableTime"/>.
    /// </summary>
    public class NullableTimePicker : TimePicker
    {
        public static readonly BindableProperty NullableTimeProperty =
            BindableProperty.Create(
                nameof(NullableTime),
                typeof(TimeSpan?),
                typeof(NullableTimePicker),
                null,
                BindingMode.TwoWay);

        public TimeSpan? NullableTime
        {
            get => (TimeSpan?)this.GetValue(NullableTimeProperty);
            set => this.SetValue(NullableTimeProperty, value);
        }

        public static readonly BindableProperty HorizontalTextAlignmentProperty =
            BindableProperty.Create(
                nameof(HorizontalTextAlignment),
                typeof(TextAlignment),
                typeof(NullableTimePicker),
                TextAlignment.Start);

        public TextAlignment HorizontalTextAlignment
        {
            get => (TextAlignment)this.GetValue(HorizontalTextAlignmentProperty);
            set => this.SetValue(HorizontalTextAlignmentProperty, value);
        }

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(
                nameof(Placeholder),
                typeof(string),
                typeof(NullableTimePicker));

        public string Placeholder
        {
            get => (string)this.GetValue(PlaceholderProperty);
            set => this.SetValue(PlaceholderProperty, value);
        }

        public static readonly BindableProperty PlaceholderColorProperty =
            BindableProperty.Create(
                nameof(PlaceholderColor),
                typeof(Color),
                typeof(NullableTimePicker),
                Colors.Transparent);

        public Color PlaceholderColor
        {
            get => (Color)this.GetValue(PlaceholderColorProperty);
            set => this.SetValue(PlaceholderColorProperty, value);
        }
    }
}