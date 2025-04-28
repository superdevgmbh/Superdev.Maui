using System.Diagnostics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.iOS.Utils;
using Superdev.Maui.Utils;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    public class NullableDatePickerHandler : CustomDatePickerHandler
    {
        private UIDatePicker _uiDatePicker;

        static NullableDatePickerHandler()
        {
            Mapper.AppendToMapping(DatePicker.FormatProperty.PropertyName, UpdateFormat);
            Mapper.AppendToMapping(DatePicker.DateProperty.PropertyName, UpdateDate);
            Mapper.AppendToMapping(NullableDatePicker.NullableDateProperty.PropertyName, UpdateNullableDate);
            Mapper.AppendToMapping(DialogExtensions.NeutralButtonTextProperty.PropertyName, UpdateNeutralButtonText);
            Mapper.AppendToMapping(NullableDatePicker.PlaceholderProperty.PropertyName, UpdatePlaceholder);
        }

        private new NullableDatePicker VirtualView => (NullableDatePicker)base.VirtualView;

        protected override void SetupUIToolbar(MauiDatePicker mauiDatePicker)
        {
            lock (mauiDatePicker.InputAccessoryView)
            {
                var buttonItems = new List<UIBarButtonItem>();
                var clearButton = CreateClearButton(this.VirtualView, mauiDatePicker);
                if (clearButton != null)
                {
                    buttonItems.Add(clearButton);
                }

                buttonItems.Add(new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace));
                buttonItems.Add(UIToolbarHelper.CreateDoneButton(this.VirtualView, this.HandleDoneButton));

                mauiDatePicker.InputAccessoryView = UIToolbarHelper.CreateUIToolbar(buttonItems.ToArray());
                mauiDatePicker.InputAccessoryView.SetNeedsDisplay();
            }
        }

        private static UIBarButtonItem CreateClearButton(NullableDatePicker nullableDatePicker, MauiDatePicker mauiDatePicker)
        {
            var clearButtonText = DialogExtensions.GetNeutralButtonText(nullableDatePicker);
            if (!string.IsNullOrEmpty(clearButtonText))
            {
                var clearButton = new UIBarButtonItem(clearButtonText, UIBarButtonItemStyle.Plain, (_, _) =>
                {
                    HandleClearButton(nullableDatePicker, mauiDatePicker);
                });
                return clearButton;
            }

            return null;
        }

        private void HandleDoneButton(object sender, EventArgs e)
        {
            this.VirtualView.Date = this._uiDatePicker.Date.ToDateTime();
            this.VirtualView.NullableDate = this._uiDatePicker.Date.ToDateTime();
            SetNullableText(this.PlatformView, this.VirtualView);
            this.PlatformView.ResignFirstResponder();
        }

        protected override void ConnectHandler(MauiDatePicker platformView)
        {
            this._uiDatePicker = ReflectionHelper.GetPropertyValue<UIDatePicker>(this.PlatformView, "DatePickerDialog");
            this.VirtualView.AddCleanUpEvent();
            base.ConnectHandler(platformView);

            // HACK:
            // For some reason, the MauiDatePicker.Text property is always set to a date when initialized,
            // even after the last Date property changed event was fired.
            // We found no better way than to overwrite the Text property after a few milliseconds delay.
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(10);
                SetNullableText(this.PlatformView, this.VirtualView);
            });
        }

        protected override void DisconnectHandler(MauiDatePicker platformView)
        {
            this._uiDatePicker = null;
            base.DisconnectHandler(platformView);
        }

        private static void UpdatePlaceholder(IDatePickerHandler datePickerHandler, IDatePicker datePicker)
        {
            if (datePickerHandler is NullableDatePickerHandler handler && datePicker is NullableDatePicker nullableDatePicker)
            {
                handler.PlatformView.Placeholder = nullableDatePicker.Placeholder;
            }
        }

        protected override void UpdateDoneButton(MauiDatePicker mauiDatePicker)
        {
            this.SetupUIToolbar(this.PlatformView);
        }

        private static void UpdateNeutralButtonText(IDatePickerHandler datePickerHandler, IDatePicker datePicker)
        {
            if (datePickerHandler is NullableDatePickerHandler handler && datePicker is NullableDatePicker nullableDatePicker)
            {
                handler.SetupUIToolbar(handler.PlatformView);
            }
        }

        private static void HandleClearButton(NullableDatePicker nullableDatePicker, MauiDatePicker mauiDatePicker)
        {
            nullableDatePicker.Date = DateTime.Now;
            nullableDatePicker.NullableDate = null;
            SetNullableText(mauiDatePicker, nullableDatePicker);
            mauiDatePicker.ResignFirstResponder();
        }

        private static void UpdateFormat(IDatePickerHandler datePickerHandler, IDatePicker datePicker)
        {
            Debug.WriteLine("UpdateFormat");

            if (datePicker is NullableDatePicker nullableDatePicker)
            {
                SetNullableText(datePickerHandler.PlatformView, nullableDatePicker);
            }
        }

        private static void UpdateDate(IDatePickerHandler datePickerHandler, IDatePicker datePicker)
        {
            Debug.WriteLine("UpdateDate");

            if (datePicker is NullableDatePicker nullableDatePicker &&
                datePickerHandler is NullableDatePickerHandler)
            {
                UpdateNullableDate(datePickerHandler, nullableDatePicker);
            }
        }

        private static void UpdateNullableDate(IDatePickerHandler datePickerHandler, IDatePicker datePicker)
        {
            Debug.WriteLine("UpdateNullableDate");

            if (datePicker is NullableDatePicker nullableDatePicker)
            {
                SetNullableText(datePickerHandler.PlatformView, nullableDatePicker);
            }
        }

        private static void SetNullableText(MauiDatePicker mauiDatePicker, NullableDatePicker nullableDatePicker)
        {
            var format = nullableDatePicker.Format;
            var originalText = mauiDatePicker.Text;

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

            Debug.WriteLine($"SetNullableText: mauiDatePicker.Text=\"{originalText}\" --> mauiDatePicker.Text=\"{mauiDatePicker.Text}\"");
        }
    }
}