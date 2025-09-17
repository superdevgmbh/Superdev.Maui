using System.Diagnostics;
using Foundation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Utils;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.iOS.Utils;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<NullableDatePicker, NullableDatePickerHandler>;

    public class NullableDatePickerHandler : DatePickerHandler
    {
        public new static readonly PM Mapper = new PM(DatePickerHandler.Mapper)
        {
            [nameof(DatePicker.Format)] = UpdateFormat,
            [nameof(DatePicker.Date)] = UpdateDate,
            [nameof(NullableDatePicker.NullableDate)] = UpdateNullableDate,
            [nameof(NullableDatePicker.Placeholder)] = UpdatePlaceholder,
            [nameof(NullableDatePicker.PlaceholderColor)] = UpdatePlaceholder,
            [nameof(DialogExtensions.NeutralButtonText)] = UpdateNeutralButtonText
        };

        private UIDatePicker uiDatePicker;

        public NullableDatePickerHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public NullableDatePickerHandler()
            : base(Mapper)
        {
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
            this.VirtualView.Date = this.uiDatePicker.Date.ToDateTime();
            this.VirtualView.NullableDate = this.uiDatePicker.Date.ToDateTime();
            SetNullableText(this.PlatformView, this.VirtualView);
            this.PlatformView.ResignFirstResponder();
        }

        protected override void ConnectHandler(MauiDatePicker platformView)
        {
            this.uiDatePicker = ReflectionHelper.GetPropertyValue<UIDatePicker>(this.PlatformView, "DatePickerDialog");
#if !NET9_0_OR_GREATER
            this.VirtualView.AddCleanUpEvent();
#endif
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
            this.uiDatePicker = null;
            base.DisconnectHandler(platformView);
        }

        private static void UpdatePlaceholder(NullableDatePickerHandler nullableDatePickerHandler, NullableDatePicker nullableDatePicker)
        {
            var placeholderText = nullableDatePicker.Placeholder;
            var mauiDatePicker = nullableDatePickerHandler.PlatformView;

            if (nullableDatePicker.PlaceholderColor is Color placeholderColor)
            {
                var foregroundColor = placeholderColor.ToPlatform();
                var attributedPlaceholder = new NSAttributedString(placeholderText, foregroundColor: foregroundColor);
                mauiDatePicker.AttributedPlaceholder = attributedPlaceholder;
            }
            else
            {
                mauiDatePicker.Placeholder = placeholderText;
            }
        }

        protected override void UpdateDoneButton(MauiDatePicker mauiDatePicker)
        {
            this.SetupUIToolbar(this.PlatformView);
        }

        private static void UpdateNeutralButtonText(NullableDatePickerHandler nullableDatePickerHandler,
            NullableDatePicker nullableDatePicker)
        {
            nullableDatePickerHandler.SetupUIToolbar(nullableDatePickerHandler.PlatformView);
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