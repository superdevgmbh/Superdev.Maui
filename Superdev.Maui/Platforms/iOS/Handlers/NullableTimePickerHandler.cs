using System.Diagnostics;
using Foundation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Extensions;
using UIKit;
using TimePicker = Microsoft.Maui.Controls.TimePicker;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<NullableTimePicker, NullableTimePickerHandler>;

    public class NullableTimePickerHandler : TimePickerHandler
    {
        private static readonly DateTime DateTime111 = new DateTime(1, 1, 1);

        public new static readonly PM Mapper = new PM(TimePickerHandler.Mapper)
        {
            [nameof(TimePicker.Time)] = MapTime,
            [nameof(TimePicker.Format)] = MapFormat,
            [nameof(NullableTimePicker.NullableTime)] = MapNullableTime,
            [nameof(NullableTimePicker.Placeholder)] = MapPlaceholder,
            [nameof(NullableTimePicker.PlaceholderColor)] = MapPlaceholderColor,
            [nameof(DialogExtensions.NeutralButtonText)] = MapNeutralButtonText
        };

        private bool isClearing;

        public NullableTimePickerHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public NullableTimePickerHandler()
            : base(Mapper)
        {
        }

        protected override MauiTimePicker CreatePlatformView()
        {
            var mauiTimePicker = base.CreatePlatformView();

            var inputAccessoryView = (MauiDoneAccessoryView)mauiTimePicker.InputAccessoryView!;
            inputAccessoryView.SetDoneButtonAction(this.HandleDoneButton);
            inputAccessoryView.SetClearButtonAction(this.HandleClearButton);
            mauiTimePicker.InputAccessoryView = inputAccessoryView;

            return mauiTimePicker;
        }

        private void HandleDoneButton()
        {
            var timePicker = this.VirtualView;
            var mauiTimePicker = this.PlatformView;

            if (timePicker == null || mauiTimePicker == null)
            {
                return;
            }

            var timeOfDay = mauiTimePicker.Date.ToDateTime().TimeOfDay;
            var time = new TimeSpan(hours: timeOfDay.Hours, minutes: timeOfDay.Minutes, 0);
            timePicker.Time = time;
            timePicker.NullableTime = time;
            mauiTimePicker.ResignFirstResponder();
        }

        private new NullableTimePicker VirtualView => (NullableTimePicker)base.VirtualView;

        private static void MapPlaceholder(NullableTimePickerHandler nullableTimePickerHandler, NullableTimePicker nullableTimePicker)
        {
            Debug.WriteLine("MapPlaceholder");
            UpdatePlaceholder(nullableTimePickerHandler, nullableTimePicker);
        }

        private static void MapPlaceholderColor(NullableTimePickerHandler nullableTimePickerHandler, NullableTimePicker nullableTimePicker)
        {
            Debug.WriteLine("MapPlaceholderColor");
            UpdatePlaceholder(nullableTimePickerHandler, nullableTimePicker);
        }

        private static void UpdatePlaceholder(NullableTimePickerHandler nullableTimePickerHandler, NullableTimePicker nullableTimePicker)
        {
            var placeholderText = nullableTimePicker.Placeholder;
            var mauiTimePicker = nullableTimePickerHandler.PlatformView;

            if (nullableTimePicker.PlaceholderColor is Color placeholderColor)
            {
                var foregroundColor = placeholderColor.ToPlatform();
                var attributedPlaceholder = new NSAttributedString(placeholderText, foregroundColor: foregroundColor);
                mauiTimePicker.AttributedPlaceholder = attributedPlaceholder;
            }
            else
            {
                mauiTimePicker.Placeholder = placeholderText;
            }
        }

        private static void MapNeutralButtonText(NullableTimePickerHandler nullableTimePickerHandler, NullableTimePicker nullableTimePicker)
        {
            nullableTimePickerHandler.UpdateClearButtonText(nullableTimePicker);
        }

        private void UpdateClearButtonText(NullableTimePicker nullableTimePicker)
        {
            var clearButtonText = DialogExtensions.GetNeutralButtonText(nullableTimePicker);
            var mauiTimePicker = this.PlatformView;
            mauiTimePicker.InputAccessoryView = MauiDoneAccessoryView.SetClearButtonText(this.inputAccessoryView, clearButtonText);
        }

        private void HandleClearButton()
        {
            this.isClearing = true;

            try
            {
                var nullableTimePicker = this.VirtualView;
                nullableTimePicker.Time = TimeSpan.Zero;
                nullableTimePicker.NullableTime = null;

                var mauiTimePicker = this.PlatformView;
                mauiTimePicker.ResignFirstResponder();
            }
            finally
            {
                this.isClearing = true;
            }
        }

        protected override void OnEditingDidEnd(TimeSpan time)
        {
            if (this.isClearing)
            {
                return;
            }

            var nullableTimePicker = this.VirtualView;
            nullableTimePicker.Time = time;
            nullableTimePicker.NullableTime = time;
        }

        private new static void MapFormat(ITimePickerHandler nullableTimePickerHandler, ITimePicker timePicker)
        {
            Debug.WriteLine("MapFormat");

            if (timePicker is NullableTimePicker nullableTimePicker)
            {
                var uiDatePicker = nullableTimePickerHandler.PlatformView?.Picker;
                UpdateTime(nullableTimePickerHandler.PlatformView, nullableTimePicker, uiDatePicker, nullableTimePicker.NullableTime);
            }
        }

        private new static void MapTime(ITimePickerHandler nullableTimePickerHandler, ITimePicker timePicker)
        {
            Debug.WriteLine("MapTime");

            if (timePicker is NullableTimePicker nullableTimePicker)
            {
                var uiDatePicker = nullableTimePickerHandler.PlatformView?.Picker;
                UpdateUIDatePicker(uiDatePicker, nullableTimePicker.Time);
                UpdateTime(nullableTimePickerHandler.PlatformView, nullableTimePicker, uiDatePicker, nullableTimePicker.Time);
            }
        }

        private static void MapNullableTime(NullableTimePickerHandler nullableTimePickerHandler, NullableTimePicker nullableTimePicker)
        {
            Debug.WriteLine("MapNullableTime");

            var uiDatePicker = nullableTimePickerHandler.PlatformView?.Picker;
            UpdateUIDatePicker(uiDatePicker, nullableTimePicker.NullableTime);
            UpdateTime(nullableTimePickerHandler.PlatformView, nullableTimePicker, uiDatePicker, nullableTimePicker.NullableTime);
        }

        public static void UpdateUIDatePicker(UIDatePicker datePicker, TimeSpan? nullableTime)
        {
            if (datePicker != null)
            {
                var time = nullableTime ?? TimeSpan.Zero;
                datePicker.Date = DateTime111.Add(time).ToNSDate();
            }
        }

        private static void UpdateTime(MauiTimePicker mauiTimePicker, NullableTimePicker nullableTimePicker, UIDatePicker uiDatePicker, TimeSpan? nullableTime)
        {
            var cultureInfo = Culture.CurrentCulture;

            var format = nullableTimePicker.Format;

            // if (string.IsNullOrEmpty(format))
            {
                var locale = new NSLocale(cultureInfo.TwoLetterISOLanguageName);

                if (uiDatePicker != null)
                {
                    uiDatePicker.Locale = locale;
                }
            }

            try
            {
                var text1 = nullableTime.ToStringExtended(format, cultureInfo);
                // var text2 = time.ToFormattedString(format, cultureInfo);
                mauiTimePicker.Text = text1;
                // Debug.WriteLine($"UpdateTime: mauiTimePicker.Text with format={format}, cultureInfo={cultureInfo} {Environment.NewLine}" +
                //                 $"> {text1}{Environment.NewLine}" +
                //                 $"> {text2}");
            }
            catch (Exception ex)
            {
                mauiTimePicker.Text = ex.Message;
            }

            if (format != null)
            {
                if (format.IndexOf('H') != -1)
                {
                    var locale = new NSLocale("de");

                    if (uiDatePicker != null)
                    {
                        uiDatePicker.Locale = locale;
                    }
                }
                else if (format.IndexOf('h') != -1)
                {
                    var locale = new NSLocale("en");

                    if (uiDatePicker != null)
                    {
                        uiDatePicker.Locale = locale;
                    }
                }
                else
                {
                    // uiDatePicker.Locale ???
                }
            }

            mauiTimePicker.UpdateCharacterSpacing(nullableTimePicker);
        }
    }
}