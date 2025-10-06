using System.Diagnostics;
using Foundation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Extensions;
using Superdev.Maui.Platforms.iOS.Utils;
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

        public NullableTimePickerHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public NullableTimePickerHandler()
            : base(Mapper)
        {
        }

        private new NullableTimePicker VirtualView => (NullableTimePicker)base.VirtualView;

        protected override void SetupUIToolbar(MauiTimePicker mauiTimePicker)
        {
            lock (mauiTimePicker.InputAccessoryView)
            {
                var buttonItems = new List<UIBarButtonItem>();
                var clearButton = CreateClearButton(this.VirtualView, mauiTimePicker);
                if (clearButton != null)
                {
                    buttonItems.Add(clearButton);
                }

                buttonItems.Add(new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace));
                buttonItems.Add(UIToolbarHelper.CreateDoneButton(this.VirtualView, this.HandleDoneButton));

                mauiTimePicker.InputAccessoryView = UIToolbarHelper.CreateUIToolbar(buttonItems.ToArray());
                mauiTimePicker.InputAccessoryView.SetNeedsDisplay();
            }
        }

        private static UIBarButtonItem CreateClearButton(NullableTimePicker nullableTimePicker, MauiTimePicker mauiTimePicker)
        {
            var clearButtonText = DialogExtensions.GetNeutralButtonText(nullableTimePicker);
            if (!string.IsNullOrEmpty(clearButtonText))
            {
                var clearButton = new UIBarButtonItem(clearButtonText, UIBarButtonItemStyle.Plain, (_, _) =>
                {
                    HandleClearButton(nullableTimePicker, mauiTimePicker);
                });
                return clearButton;
            }

            return null;
        }

        private void HandleDoneButton(object sender, EventArgs e)
        {
            var time = this.PlatformView.Date.ToDateTime().TimeOfDay;
            this.VirtualView.Time = time;
            this.VirtualView.NullableTime = time;
            this.PlatformView.ResignFirstResponder();
        }

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

        protected override void UpdateDoneButton(MauiTimePicker mauiTimePicker)
        {
            this.SetupUIToolbar(this.PlatformView);
        }

        private static void MapNeutralButtonText(NullableTimePickerHandler nullableTimePickerHandler, NullableTimePicker nullableTimePicker)
        {
            nullableTimePickerHandler.SetupUIToolbar(nullableTimePickerHandler.PlatformView);
        }

        private static void HandleClearButton(NullableTimePicker nullableTimePicker, MauiTimePicker mauiTimePicker)
        {
            nullableTimePicker.Time = TimeSpan.Zero;
            nullableTimePicker.NullableTime = null;
            mauiTimePicker.ResignFirstResponder();
        }

        protected override void OnEditingDidEnd(TimeSpan time)
        {
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