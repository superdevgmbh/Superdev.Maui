using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.iOS.Utils;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<TimePicker, TimePickerHandler>;

    public class TimePickerHandler : Microsoft.Maui.Handlers.TimePickerHandler
    {
        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.TimePickerHandler.Mapper)
        {
            [DialogExtensions.DoneButtonText] = UpdateDoneButtonText,
        };

        public TimePickerHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public TimePickerHandler()
            : base(Mapper)
        {
        }

        private new TimePicker VirtualView => (TimePicker)base.VirtualView;

        protected override MauiTimePicker CreatePlatformView()
        {
            var mauiTimePicker = base.CreatePlatformView();
            this.SetupUIToolbar(mauiTimePicker);
            return mauiTimePicker;
        }

        protected virtual void SetupUIToolbar(MauiTimePicker mauiTimePicker)
        {
            this.UpdateDoneButton(mauiTimePicker);
        }

        protected virtual void UpdateDoneButton(MauiTimePicker mauiDatePicker)
        {
            var newDoneButton = UIToolbarHelper.CreateDoneButton(this.VirtualView, (_, _) => { });
            UIToolbarHelper.ReplaceDoneButton(mauiDatePicker.InputAccessoryView, newDoneButton);
        }

        private static void UpdateDoneButtonText(TimePickerHandler timePickerHandler, TimePicker timePicker)
        {
            var mauiTimePicker = timePickerHandler.PlatformView;
            timePickerHandler.UpdateDoneButton(mauiTimePicker);
        }
    }
}