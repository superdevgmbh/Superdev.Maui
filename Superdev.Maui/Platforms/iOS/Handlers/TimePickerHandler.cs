using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.iOS.Utils;
using UIKit;

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

        protected override void ConnectHandler(MauiTimePicker platformView)
        {
            base.ConnectHandler(platformView);

            var mauiTimePicker = this.PlatformView;
            mauiTimePicker.EditingDidEnd += this.OnEditingDidEnd;
        }

        private void OnEditingDidEnd(object sender, EventArgs e)
        {
            var mauiTimePicker = this.PlatformView;
            if (mauiTimePicker.Picker is UIDatePicker picker)
            {
                var date = picker.Date.ToDateTime();
                this.OnEditingDidEnd(date.TimeOfDay);
            }
        }

        protected virtual void OnEditingDidEnd(TimeSpan time)
        {
            this.VirtualView.Time = time;
        }

        protected override void DisconnectHandler(MauiTimePicker platformView)
        {
            base.DisconnectHandler(platformView);

            var mauiTimePicker = this.PlatformView;
            if (mauiTimePicker != null)
            {
                mauiTimePicker.EditingDidEnd -= this.OnEditingDidEnd;
            }
        }
    }
}