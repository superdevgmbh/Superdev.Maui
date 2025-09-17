using Superdev.Maui.Extensions;

namespace Superdev.Maui.Effects
{
    public static class LineColorEffect
    {
        public static readonly BindableProperty LineColorProperty = BindableProperty.CreateAttached(
            "LineColor",
            typeof(Color),
            typeof(LineColorEffect),
            null,
            propertyChanged: OnLineColorPropertyChanged);

        public static Color GetLineColor(BindableObject view)
        {
            return (Color)view.GetValue(LineColorProperty);
        }

        public static void SetLineColor(BindableObject view, Color value)
        {
            view.SetValue(LineColorProperty, value);
        }

        private static void OnLineColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not View view)
            {
                return;
            }

            if (newValue is Color)
            {
                if (bindable is Entry)
                {
                    if (!view.Effects.Any(e => e is EntryLineColorEffect))
                    {
                        view.Effects.Add(new EntryLineColorEffect());
                    }
                }
                else if (bindable is Editor)
                {
                    if (!view.Effects.Any(e => e is EditorLineColorEffect))
                    {
                        view.Effects.Add(new EditorLineColorEffect());
                    }
                }
                else if (bindable is DatePicker)
                {
                    if (!view.Effects.Any(e => e is DatePickerLineColorEffect))
                    {
                        view.Effects.Add(new DatePickerLineColorEffect());
                    }
                }
                else if (bindable is TimePicker)
                {
                    if (!view.Effects.Any(e => e is TimePickerLineColorEffect))
                    {
                        view.Effects.Add(new TimePickerLineColorEffect());
                    }
                }
                else if (bindable is Picker)
                {
                    if (!view.Effects.Any(e => e is PickerLineColorEffect))
                    {
                        view.Effects.Add(new PickerLineColorEffect());
                    }
                }
                else
                {
                    throw new NotSupportedException($"LineColorEffect is not supported for {bindable.GetType().Name}");
                }
            }
            else
            {
                var entryLineColorEffectToRemove = view.Effects.FirstOrDefault<EntryLineColorEffect>();
                if (entryLineColorEffectToRemove != null)
                {
                    view.Effects.Remove(entryLineColorEffectToRemove);
                }

                var editorLineColorEffectToRemove = view.Effects.FirstOrDefault<EditorLineColorEffect>();
                if (editorLineColorEffectToRemove != null)
                {
                    view.Effects.Remove(editorLineColorEffectToRemove);
                }

                var datePickerLineColorEffectToRemove = view.Effects.FirstOrDefault<DatePickerLineColorEffect>();
                if (datePickerLineColorEffectToRemove != null)
                {
                    view.Effects.Remove(datePickerLineColorEffectToRemove);
                }

                var timePickerLineColorEffectToRemove = view.Effects.FirstOrDefault<TimePickerLineColorEffect>();
                if (timePickerLineColorEffectToRemove != null)
                {
                    view.Effects.Remove(timePickerLineColorEffectToRemove);
                }

                var pickerLineColorEffectToRemove = view.Effects.FirstOrDefault<PickerLineColorEffect>();
                if (pickerLineColorEffectToRemove != null)
                {
                    view.Effects.Remove(pickerLineColorEffectToRemove);
                }
            }
        }
    }
}