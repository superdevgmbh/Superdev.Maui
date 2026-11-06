using System.Diagnostics;
using Foundation;
using Microsoft.Extensions.Logging;
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
            [nameof(NullableTimePicker.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
            [nameof(DialogExtensions.NeutralButtonText)] = MapNeutralButtonText
        };

        private bool isClearing;

        public NullableTimePickerHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public NullableTimePickerHandler()
            : base(Mapper)
        {
        }

        public new NullableTimePicker? VirtualView => base.VirtualView as NullableTimePicker;

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

        private static void MapHorizontalTextAlignment(NullableTimePickerHandler nullableTimePickerHandler, NullableTimePicker nullableTimePicker)
        {
            nullableTimePickerHandler.PlatformView.TextAlignment = nullableTimePicker.HorizontalTextAlignment.ToPlatformHorizontal();
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
                this.isClearing = false;
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

            if (uiDatePicker != null)
            {
                uiDatePicker.Locale = GetPickerLocale(cultureInfo, format);
            }

            try
            {
                mauiTimePicker.Text = nullableTime.ToStringExtended(format, cultureInfo);
            }
            catch (Exception ex)
            {
                // The placeholder shows instead of a value the format cannot write.
                IPlatformApplication.Current?.Services.GetService<ILogger<NullableTimePickerHandler>>()?
                    .LogError(ex, "Failed to format {Time} with format '{Format}'", nullableTime, format);
                mauiTimePicker.Text = string.Empty;
            }

            mauiTimePicker.UpdateCharacterSpacing(nullableTimePicker);
        }

        /// <summary>
        /// The native picker follows the user's culture - language, digits and separators - and only takes the
        /// hour cycle from <paramref name="format"/>, so that a 24-hour format is not paired with an AM/PM picker
        /// or vice versa. The hour cycle is set through ICU's "hours" locale keyword.
        /// </summary>
        internal static NSLocale GetPickerLocale(System.Globalization.CultureInfo cultureInfo, string? format)
        {
            var localeIdentifier = string.IsNullOrEmpty(cultureInfo.Name)
                ? NSLocale.CurrentLocale.Identifier
                : cultureInfo.Name.Replace('-', '_');

            var hourCycle = GetHourCycle(format);
            if (hourCycle != null)
            {
                localeIdentifier += (localeIdentifier.Contains('@') ? ";" : "@") + $"hours={hourCycle}";
            }

            return new NSLocale(localeIdentifier);
        }

        private static string? GetHourCycle(string? format)
        {
            if (string.IsNullOrEmpty(format))
            {
                return null;
            }

            if (format.Contains('H'))
            {
                return "h23";
            }

            if (format.Contains('h'))
            {
                return "h12";
            }

            return null;
        }
    }
}