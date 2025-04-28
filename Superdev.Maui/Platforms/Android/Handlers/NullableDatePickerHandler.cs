using Android.App;
using Android.Content;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using DatePicker = Microsoft.Maui.Controls.DatePicker;

namespace Superdev.Maui.Platforms.Handlers
{
    public class NullableDatePickerHandler : CustomDatePickerHandler
    {
        private const int NeutralButtonId = (int)DialogButtonType.Neutral;

        static NullableDatePickerHandler()
        {
            Mapper.AppendToMapping(DatePicker.FormatProperty.PropertyName, UpdateFormat);
            Mapper.AppendToMapping(DatePicker.DateProperty.PropertyName, UpdateDate);
            Mapper.AppendToMapping(NullableDatePicker.PlaceholderProperty.PropertyName, UpdatePlaceholder);
            Mapper.AppendToMapping(NullableDatePicker.NullableDateProperty.PropertyName, UpdateNullableDate);
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

        private static void UpdatePlaceholder(IDatePickerHandler datePickerHandler, IDatePicker datePicker)
        {
            if (datePicker is NullableDatePicker nullableDatePicker)
            {
                datePickerHandler.PlatformView.Hint = nullableDatePicker.Placeholder;
            }
        }

        private static void UpdateFormat(IDatePickerHandler datePickerHandler, IDatePicker datePicker)
        {
            if (datePicker is NullableDatePicker nullableDatePicker)
            {
                SetNullableText(datePickerHandler.PlatformView, nullableDatePicker);
            }
        }

        private static void UpdateDate(IDatePickerHandler datePickerHandler, IDatePicker datePicker)
        {
            if (datePicker is NullableDatePicker nullableDatePicker)
            {
                SetNullableText(datePickerHandler.PlatformView, nullableDatePicker);
            }
        }

        private static void UpdateNullableDate(IDatePickerHandler datePickerHandler, IDatePicker datePicker)
        {
            if (datePicker is NullableDatePicker nullableDatePicker)
            {
                SetNullableText(datePickerHandler.PlatformView, nullableDatePicker);
            }
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