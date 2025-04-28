using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.iOS.Utils;

namespace Superdev.Maui.Platforms.Handlers
{
    public class PickerHandler : Microsoft.Maui.Handlers.PickerHandler
    {
        static PickerHandler()
        {
            Mapper.AppendToMapping(DialogExtensions.DoneButtonTextProperty.PropertyName, UpdateDoneButtonText);
        }

        private static void UpdateDoneButtonText(IPickerHandler pickerHandler, IPicker picker)
        {
            if (pickerHandler is PickerHandler handler)
            {
                var newDoneButton = UIToolbarHelper.CreateDoneButton((BindableObject)picker, (_, _) => { });
                UIToolbarHelper.ReplaceDoneButton(handler.PlatformView.InputAccessoryView, newDoneButton);
            }
        }

        protected override MauiPicker CreatePlatformView()
        {
            var mauiPicker = base.CreatePlatformView();

            var newDoneButton = UIToolbarHelper.CreateDoneButton((BindableObject)this.VirtualView, (_, _) => { });
            UIToolbarHelper.ReplaceDoneButton(mauiPicker.InputAccessoryView, newDoneButton);

            return mauiPicker;
        }
    }
}