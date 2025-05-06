using Microsoft.Maui.Handlers;
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

        private static void UpdateDoneButtonText(ITimePickerHandler timePickerHandler, ITimePicker timePicker)
        {
            if (timePickerHandler is TimePickerHandler handler)
            {
                var newDoneButton = UIToolbarHelper.CreateDoneButton((BindableObject)timePicker, (_, _) => { });
                UIToolbarHelper.ReplaceDoneButton(handler.PlatformView.InputAccessoryView, newDoneButton);
            }
        }

        protected override MauiTimePicker CreatePlatformView()
        {
            var mauiTimePicker = base.CreatePlatformView();

            var newDoneButton = UIToolbarHelper.CreateDoneButton((BindableObject)this.VirtualView, (_, _) => { });
            UIToolbarHelper.ReplaceDoneButton(mauiTimePicker.InputAccessoryView, newDoneButton);

            return mauiTimePicker;
        }
    }
}