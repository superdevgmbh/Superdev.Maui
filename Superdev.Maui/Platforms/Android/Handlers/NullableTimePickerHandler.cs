using System.Globalization;
using Android.App;
using Android.Content;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Extensions;
using TimePicker = Microsoft.Maui.Controls.TimePicker;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<NullableTimePicker, NullableTimePickerHandler>;

    public class NullableTimePickerHandler : TimePickerHandler
    {
        public new static readonly PM Mapper = new PM(TimePickerHandler.Mapper)
        {
            [nameof(TimePicker.Format)] = MapFormat,
            [nameof(TimePicker.Time)] = MapTime,
            [nameof(NullableTimePicker.NullableTime)] = MapNullableTime,
            [nameof(NullableTimePicker.Placeholder)] = MapPlaceholder,
            [nameof(NullableTimePicker.PlaceholderColor)] = MapPlaceholderColor
        };

        private const int NeutralButtonId = (int)DialogButtonType.Neutral;

        public NullableTimePickerHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public NullableTimePickerHandler()
            : base(Mapper)
        {
        }

        private new NullableTimePicker VirtualView => (NullableTimePicker)base.VirtualView;

        protected override TimePickerDialog CreateTimePickerDialog(int hours, int minutes)
        {
            var dialog = base.CreateTimePickerDialog(hours, minutes);

            this.UpdateNeutralButton(this.PlatformView, this.VirtualView, dialog);

            return dialog;
        }

        private static void MapPlaceholder(NullableTimePickerHandler datePickerHandler, NullableTimePicker nullableTimePicker)
        {
            datePickerHandler.PlatformView.Hint = nullableTimePicker.Placeholder;
        }

        private static void MapPlaceholderColor(NullableTimePickerHandler datePickerHandler, NullableTimePicker nullableTimePicker)
        {
            if (nullableTimePicker.PlaceholderColor is Color placeholderColor)
            {
                datePickerHandler.PlatformView.SetHintTextColor(placeholderColor.ToPlatform());
            }
        }

        private static void MapFormat(NullableTimePickerHandler datePickerHandler, NullableTimePicker nullableTimePicker)
        {
            SetNullableText(datePickerHandler.PlatformView, nullableTimePicker);
        }

        private static void MapTime(NullableTimePickerHandler datePickerHandler, NullableTimePicker nullableTimePicker)
        {
            SetNullableText(datePickerHandler.PlatformView, nullableTimePicker);
        }

        private static void MapNullableTime(NullableTimePickerHandler datePickerHandler, NullableTimePicker nullableTimePicker)
        {
            SetNullableText(datePickerHandler.PlatformView, nullableTimePicker);
        }

        protected override void HandlePositiveButtonTap(TimeSpan time)
        {
            var nullableTimePicker = this.VirtualView;
            nullableTimePicker.Time = time;
            nullableTimePicker.NullableTime = time;
        }

        private void UpdateNeutralButton(MauiTimePicker mauiTimePicker, NullableTimePicker nullableTimePicker, TimePickerDialog dialog)
        {
            var neutralButtonText = GetNeutralButtonText(nullableTimePicker);
            if (!string.IsNullOrEmpty(neutralButtonText))
            {
                dialog.SetButton(NeutralButtonId, neutralButtonText, (_, a) =>
                {
                    if (a.Which == NeutralButtonId)
                    {
                        this.HandleNeutralButtonTap(mauiTimePicker, nullableTimePicker);
                    }
                });
            }
        }

        protected virtual void HandleNeutralButtonTap(MauiTimePicker mauiTimePicker, NullableTimePicker nullableTimePicker)
        {
            nullableTimePicker.Unfocus();
            nullableTimePicker.NullableTime = null;
            SetNullableText(mauiTimePicker, nullableTimePicker);
        }

        private static void SetNullableText(MauiTimePicker mauiTimePicker, NullableTimePicker nullableTimePicker)
        {
            var currentCulture = CultureInfo.CurrentCulture;

            try
            {
                var nullableTime = nullableTimePicker.NullableTime;
                var format = nullableTimePicker.Format;
                mauiTimePicker.Text = nullableTime.ToStringExtended(format, currentCulture);
            }
            catch (Exception ex)
            {
                mauiTimePicker.Text = ex.Message;
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