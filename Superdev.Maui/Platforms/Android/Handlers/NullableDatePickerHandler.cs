using Android.App;
using Android.Content;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using DatePicker = Microsoft.Maui.Controls.DatePicker;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<NullableDatePicker, NullableDatePickerHandler>;

    public class NullableDatePickerHandler : DatePickerHandler
    {
        public new static readonly PM Mapper = new PM(DatePickerHandler.Mapper)
        {
            [nameof(DatePicker.Format)] = MapFormat,
            [nameof(DatePicker.Date)] = MapDate,
            [nameof(NullableDatePicker.NullableDate)] = MapNullableDate,
            [nameof(NullableDatePicker.Placeholder)] = MapPlaceholder,
            [nameof(NullableDatePicker.PlaceholderColor)] = MapPlaceholderColor
        };

        private const int NeutralButtonId = (int)DialogButtonType.Neutral;

        public NullableDatePickerHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public NullableDatePickerHandler()
            : base(Mapper)
        {
        }

        private new NullableDatePicker VirtualView => (NullableDatePicker)base.VirtualView;

        protected override DateTime GetSelectedDate()
        {
            return this.VirtualView.NullableDate ?? DateTime.Now;
        }

        protected override DatePickerDialog CreateDatePickerDialog(int year, int month, int day)
        {
            var dialog = base.CreateDatePickerDialog(year, month, day);

            this.UpdateNeutralButton(this.PlatformView, this.VirtualView, dialog);

            return dialog;
        }

        private static void MapPlaceholder(NullableDatePickerHandler datePickerHandler, NullableDatePicker nullableDatePicker)
        {
            datePickerHandler.PlatformView.Hint = nullableDatePicker.Placeholder;
        }

        private static void MapPlaceholderColor(NullableDatePickerHandler datePickerHandler, NullableDatePicker nullableDatePicker)
        {
            if (nullableDatePicker.PlaceholderColor is Color placeholderColor)
            {
                datePickerHandler.PlatformView.SetHintTextColor(placeholderColor.ToPlatform());
            }
        }

        private static void MapFormat(NullableDatePickerHandler datePickerHandler, NullableDatePicker nullableDatePicker)
        {
            SetNullableText(datePickerHandler.PlatformView, nullableDatePicker);
        }

        private static void MapDate(NullableDatePickerHandler datePickerHandler, NullableDatePicker nullableDatePicker)
        {
            SetNullableText(datePickerHandler.PlatformView, nullableDatePicker);
        }

        private static void MapNullableDate(NullableDatePickerHandler datePickerHandler, NullableDatePicker nullableDatePicker)
        {
            SetNullableText(datePickerHandler.PlatformView, nullableDatePicker);
        }

        protected override void HandlePositiveButtonTap(IDatePicker datePicker, DatePickerDialog dialog)
        {
            var nullableDatePicker = (NullableDatePicker)datePicker;
            nullableDatePicker.Date = dialog.DatePicker.DateTime;
            nullableDatePicker.NullableDate = dialog.DatePicker.DateTime;
        }

        private void UpdateNeutralButton(MauiDatePicker mauiDatePicker, NullableDatePicker nullableDatePicker, DatePickerDialog dialog)
        {
            var neutralButtonText = GetNeutralButtonText(nullableDatePicker);
            if (!string.IsNullOrEmpty(neutralButtonText))
            {
                dialog.SetButton(NeutralButtonId, neutralButtonText, (_, a) =>
                {
                    if (a.Which == NeutralButtonId)
                    {
                        this.HandleNeutralButtonTap(mauiDatePicker, nullableDatePicker);
                    }
                });
            }
        }

        protected virtual void HandleNeutralButtonTap(MauiDatePicker mauiDatePicker, NullableDatePicker nullableDatePicker)
        {
            nullableDatePicker.Unfocus();
            nullableDatePicker.NullableDate = null;
            SetNullableText(mauiDatePicker, nullableDatePicker);
        }

        private static void SetNullableText(MauiDatePicker mauiDatePicker, NullableDatePicker nullableDatePicker)
        {
            var format = nullableDatePicker.Format;

            if (nullableDatePicker.NullableDate is DateTime dateTime && dateTime != DateTime.MinValue &&
                !string.IsNullOrEmpty(format))
            {
                var localDateTime = dateTime.ToLocalTime();
                mauiDatePicker.Text = localDateTime.ToString(format);
            }
            else
            {
                mauiDatePicker.Text = string.Empty;
            }
        }

        private static string GetNeutralButtonText(BindableObject element)
        {
            if (DialogExtensions.GetNeutralButtonText(element) is string neutralButtonText)
            {
                return neutralButtonText;
            }

            return null;
        }
    }
}