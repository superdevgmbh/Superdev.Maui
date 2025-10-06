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
            [nameof(DatePicker.Date)] = MapDate,
            [nameof(DatePicker.Format)] = MapFormat,
            [nameof(NullableDatePicker.NullableDate)] = MapNullableDate,
            [nameof(NullableDatePicker.Placeholder)] = MapPlaceholder,
            [nameof(NullableDatePicker.PlaceholderColor)] = MapPlaceholderColor,
            [nameof(DialogExtensions.NeutralButtonText)] = MapNeutralButtonText
        };

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
            var date = this.Picker.Date.ToDateTime();
            this.VirtualView.Date = date;
            this.VirtualView.NullableDate = date;
            this.PlatformView.ResignFirstResponder();
        }

        protected override void OnEditingDidEnd(DateTime date)
        {
            this.VirtualView.Date = date;
            this.VirtualView.NullableDate = date;
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

        protected override void DisconnectHandler(MauiDatePicker platformView)
        {
            this.mauiDatePicker = null;
            base.DisconnectHandler(platformView);
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

        protected override void UpdateDoneButton(MauiDatePicker mauiDatePicker)
        {
            this.SetupUIToolbar(this.PlatformView);
        }

        private static void HandleClearButton(NullableDatePicker nullableDatePicker, MauiDatePicker mauiDatePicker)
        {
            nullableDatePicker.Date = DateTime.Now;
            nullableDatePicker.NullableDate = null;
            mauiDatePicker.ResignFirstResponder();
        }

        private static void MapNeutralButtonText(NullableDatePickerHandler nullableDatePickerHandler, NullableDatePicker nullableDatePicker)
        {
            nullableDatePickerHandler.SetupUIToolbar(nullableDatePickerHandler.PlatformView);
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