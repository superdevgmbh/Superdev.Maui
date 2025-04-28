using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.iOS.Utils;

namespace Superdev.Maui.Platforms.Handlers
{
    public class TimePickerHandler : Microsoft.Maui.Handlers.TimePickerHandler
    {
        static TimePickerHandler()
        {
            Mapper.AppendToMapping(DialogExtensions.DoneButtonTextProperty.PropertyName, UpdateDoneButtonText);
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