using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.iOS.Utils;

namespace Superdev.Maui.Platforms.iOS.Handlers
{
    public class DatePickerHandler : Microsoft.Maui.Handlers.DatePickerHandler
    {
        static DatePickerHandler()
        {
            Mapper.AppendToMapping(DialogExtensions.DoneButtonTextProperty.PropertyName, UpdateDoneButtonText);
        }

        protected override MauiDatePicker CreatePlatformView()
        {
            var mauiDatePicker = base.CreatePlatformView();
            this.SetupUIToolbar(mauiDatePicker);
            return mauiDatePicker;
        }

        protected virtual void SetupUIToolbar(MauiDatePicker mauiDatePicker)
        {
            this.UpdateDoneButton(mauiDatePicker);
        }

        protected virtual void UpdateDoneButton(MauiDatePicker mauiDatePicker)
        {
            var newDoneButton = UIToolbarHelper.CreateDoneButton((BindableObject)this.VirtualView, (_, _) => { });
            UIToolbarHelper.ReplaceDoneButton(mauiDatePicker.InputAccessoryView, newDoneButton);
        }

        private static void UpdateDoneButtonText(IDatePickerHandler datePickerHandler, IDatePicker datePicker)
        {
            if (datePickerHandler is DatePickerHandler handler)
            {
                handler.UpdateDoneButton(handler.PlatformView);
            }
        }
    }
}