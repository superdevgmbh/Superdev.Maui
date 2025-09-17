using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.iOS.Utils;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<Picker, PickerHandler>;

    public class PickerHandler : Microsoft.Maui.Handlers.PickerHandler
    {
        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.PickerHandler.Mapper)
        {
            [DialogExtensions.DoneButtonText] = UpdateDoneButtonText,
        };

        public PickerHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public PickerHandler()
            : base(Mapper)
        {
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