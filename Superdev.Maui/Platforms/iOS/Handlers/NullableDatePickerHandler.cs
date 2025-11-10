using System.Diagnostics;
using Foundation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<NullableDatePicker, NullableDatePickerHandler>;

    public class NullableDatePickerHandler : DatePickerHandler
    {
        public new static readonly PM Mapper = new PM(DatePickerHandler.Mapper)
        {
            [nameof(DatePicker.Date)] = MapDate,
            [nameof(DatePicker.Format)] = MapFormat,
            [nameof(NullableDatePicker.NullableDate)] = MapNullableDate,
            [nameof(NullableDatePicker.Placeholder)] = MapPlaceholder,
            [nameof(NullableDatePicker.PlaceholderColor)] = MapPlaceholderColor,
            [nameof(DialogExtensions.NeutralButtonText)] = MapNeutralButtonText
        };

        private bool isClearing;

        public NullableDatePickerHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public NullableDatePickerHandler()
            : base(Mapper)
        {
        }

        protected override MauiDatePicker CreatePlatformView()
        {
            var mauiDatePicker = base.CreatePlatformView();

            var inputAccessoryView = (MauiDoneAccessoryView)mauiDatePicker.InputAccessoryView!;
            inputAccessoryView.SetDoneButtonAction(this.HandleDoneButton);
            inputAccessoryView.SetClearButtonAction(this.HandleClearButton);
            mauiDatePicker.InputAccessoryView = inputAccessoryView;

            return mauiDatePicker;
        }

        protected override void DisconnectHandler(MauiDatePicker platformView)
        {
            platformView.InputAccessoryView = null;
            this.inputAccessoryView?.Dispose();
            this.inputAccessoryView = null;

            base.DisconnectHandler(platformView);
        }

        private new NullableDatePicker VirtualView => (NullableDatePicker)base.VirtualView;

        private void HandleDoneButton()
        {
            var nullableDatePicker = this.VirtualView;
            var uiDatePicker = this.Picker;

            if (nullableDatePicker == null || uiDatePicker == null)
            {
                return;
            }

            var date = uiDatePicker.Date.ToDateTime().Date;
            nullableDatePicker.Date = date;
            nullableDatePicker.NullableDate = date;

            var mauiDatePicker = this.PlatformView;
            mauiDatePicker.ResignFirstResponder();
        }

        protected override void OnEditingDidEnd(DateTime date)
        {
            if (this.isClearing)
            {
                return;
            }

            var nullableDatePicker = this.VirtualView;
            nullableDatePicker.Date = date;
            nullableDatePicker.NullableDate = date;
        }

        protected override void ConnectHandler(MauiDatePicker platformView)
        {
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
                UpdateDate(this.PlatformView, this.VirtualView);
            });
        }

        private static void MapPlaceholder(NullableDatePickerHandler nullableDatePickerHandler, NullableDatePicker nullableDatePicker)
        {
            Debug.WriteLine("MapPlaceholder");
            UpdatePlaceholder(nullableDatePickerHandler, nullableDatePicker);
        }

        private static void MapPlaceholderColor(NullableDatePickerHandler nullableDatePickerHandler, NullableDatePicker nullableDatePicker)
        {
            Debug.WriteLine("MapPlaceholderColor");
            UpdatePlaceholder(nullableDatePickerHandler, nullableDatePicker);
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

        private static void MapNeutralButtonText(NullableDatePickerHandler nullableDatePickerHandler, NullableDatePicker nullableDatePicker)
        {
            nullableDatePickerHandler.UpdateClearButtonText(nullableDatePicker);
        }

        private void UpdateClearButtonText(NullableDatePicker nullableDatePicker)
        {
            var clearButtonText = DialogExtensions.GetNeutralButtonText(nullableDatePicker);
            var mauiTextField = this.PlatformView;
            mauiTextField.InputAccessoryView = MauiDoneAccessoryView.SetClearButtonText(this.inputAccessoryView, clearButtonText);
        }

        private void HandleClearButton()
        {
            this.isClearing = true;

            try
            {
                var nullableDatePicker = this.VirtualView;
                nullableDatePicker.Date = DateTime.Now;
                nullableDatePicker.NullableDate = null;

                var mauiDatePicker = this.PlatformView;
                mauiDatePicker.ResignFirstResponder();
            }
            finally
            {
                this.isClearing = true;
            }
        }

        private new static void MapFormat(IDatePickerHandler datePickerHandler, IDatePicker datePicker)
        {
            Debug.WriteLine("MapFormat");

            if (datePicker is NullableDatePicker nullableDatePicker)
            {
                UpdateDate(datePickerHandler.PlatformView, nullableDatePicker);
            }
        }

        private new static void MapDate(IDatePickerHandler datePickerHandler, IDatePicker datePicker)
        {
            Debug.WriteLine("MapDate");

            if (datePickerHandler is NullableDatePickerHandler nullableDatePickerHandler && datePicker is NullableDatePicker nullableDatePicker)
            {
                UpdateUIDatePicker(nullableDatePickerHandler.Picker, nullableDatePicker.Date);
                MapNullableDate(datePickerHandler, nullableDatePicker);
            }
        }

        private static void MapNullableDate(IDatePickerHandler datePickerHandler, IDatePicker datePicker)
        {
            Debug.WriteLine("MapNullableDate");

            if (datePickerHandler is NullableDatePickerHandler nullableDatePickerHandler && datePicker is NullableDatePicker nullableDatePicker)
            {
                UpdateUIDatePicker(nullableDatePickerHandler.Picker, nullableDatePicker.NullableDate);
                UpdateDate(datePickerHandler.PlatformView, nullableDatePicker);
            }
        }

        private static void UpdateUIDatePicker(UIDatePicker datePicker, DateTime? nullableDate)
        {
            if (datePicker != null)
            {
                var dateTime = nullableDate ?? DateTime.Now;
                datePicker.Date = dateTime.ToNSDate();
            }
        }

        private static void UpdateDate(MauiDatePicker mauiDatePicker, NullableDatePicker nullableDatePicker)
        {
            var format = nullableDatePicker.Format;

            if (nullableDatePicker.NullableDate is DateTime dateTime && dateTime != DateTime.MinValue &&
                !string.IsNullOrEmpty(format))
            {
                try
                {
                    var localDateTime = dateTime.ToLocalTime();
                    var text = localDateTime.ToString(format);
                    mauiDatePicker.Text = text;
                    // Debug.WriteLine($"UpdateDate: mauiDatePicker.Text with format={format}, cultureInfo={Culture.CurrentCulture} {Environment.NewLine}" +
                    //                 $"> {text}");
                }
                catch (Exception ex)
                {
                    mauiDatePicker.Text = ex.Message;
                }
            }
            else
            {
                mauiDatePicker.Text = string.Empty;
            }
        }
    }
}