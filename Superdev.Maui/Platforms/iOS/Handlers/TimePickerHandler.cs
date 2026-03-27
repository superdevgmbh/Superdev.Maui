using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<TimePicker, TimePickerHandler>;

    public class TimePickerHandler : Microsoft.Maui.Handlers.TimePickerHandler
    {
        protected MauiDoneAccessoryView? inputAccessoryView;

        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.TimePickerHandler.Mapper)
        {
            [DialogExtensions.DoneButtonText] = MapDoneButtonText,
        };

        public TimePickerHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public TimePickerHandler()
            : base(Mapper)
        {
        }

        public new TimePicker? VirtualView => ((ElementHandler)this).VirtualView as TimePicker;

        public new MauiTimePicker? PlatformView => ((ElementHandler)this).PlatformView as MauiTimePicker;

        protected override MauiTimePicker CreatePlatformView()
        {
            var mauiTimePicker = base.CreatePlatformView();

            this.inputAccessoryView = new MauiDoneAccessoryView();
            this.inputAccessoryView.SetDoneButtonAction(this.HandleDoneButton);
            mauiTimePicker.InputAccessoryView = this.inputAccessoryView;

            return mauiTimePicker;
        }

        protected override void ConnectHandler(MauiTimePicker platformView)
        {
            base.ConnectHandler(platformView);

            platformView.EditingDidEnd += this.OnEditingDidEnd;
        }

        protected override void DisconnectHandler(MauiTimePicker platformView)
        {
            platformView.EditingDidEnd -= this.OnEditingDidEnd;

            platformView.InputAccessoryView = null;
            this.inputAccessoryView?.Dispose();
            this.inputAccessoryView = null;

            base.DisconnectHandler(platformView);
        }

        private void HandleDoneButton()
        {
            if (this.VirtualView is not TimePicker timePicker ||
                this.PlatformView is not MauiTimePicker mauiTimePicker)
            {
                return;
            }

            var timeOfDay = mauiTimePicker.Date.ToDateTime().TimeOfDay;
            var time = new TimeSpan(hours: timeOfDay.Hours, minutes: timeOfDay.Minutes, 0);
            timePicker.Time = time;
            mauiTimePicker.ResignFirstResponder();
        }

        private static void MapDoneButtonText(TimePickerHandler timePickerHandler, TimePicker timePicker)
        {
            timePickerHandler.DoneButtonText(timePicker);
        }

        private void DoneButtonText(TimePicker timePicker)
        {
            if (this.PlatformView is not MauiTimePicker mauiTimePicker || this.inputAccessoryView == null)
            {
                return;
            }

            var doneButtonText = DialogExtensions.GetDoneButtonText(timePicker);
            mauiTimePicker.InputAccessoryView = MauiDoneAccessoryView.SetDoneButtonText(ref this.inputAccessoryView, doneButtonText);
        }

        private void OnEditingDidEnd(object? sender, EventArgs e)
        {
            if (sender is not MauiTimePicker mauiTimePicker)
            {
                return;
            }

            if (mauiTimePicker.Picker is UIDatePicker uiDatePicker)
            {
                var timeOfDay = uiDatePicker.Date.ToDateTime().TimeOfDay;
                var time = new TimeSpan(hours: timeOfDay.Hours, minutes: timeOfDay.Minutes, 0);
                this.OnEditingDidEnd(time);
            }
        }

        protected virtual void OnEditingDidEnd(TimeSpan time)
        {
            if (this.VirtualView is not TimePicker timePicker)
            {
                return;
            }

            timePicker.Time = time;
        }
    }
}