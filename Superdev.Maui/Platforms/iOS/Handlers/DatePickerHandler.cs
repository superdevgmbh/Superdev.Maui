using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.iOS.Utils;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<DatePicker, DatePickerHandler>;

    public class DatePickerHandler : Microsoft.Maui.Handlers.DatePickerHandler
    {
        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.DatePickerHandler.Mapper)
        {
            [DialogExtensions.DoneButtonText] = UpdateDoneButtonText,
        };

        public DatePickerHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public DatePickerHandler()
            : base(Mapper)
        {
        }

        private new DatePicker VirtualView => (DatePicker)base.VirtualView;

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
            var newDoneButton = UIToolbarHelper.CreateDoneButton(this.VirtualView, (_, _) => { });
            UIToolbarHelper.ReplaceDoneButton(mauiDatePicker.InputAccessoryView, newDoneButton);
        }

        private static void UpdateDoneButtonText(DatePickerHandler datePickerHandler, DatePicker datePicker)
        {
            var mauiDatePicker = datePickerHandler.PlatformView;
            datePickerHandler.UpdateDoneButton(mauiDatePicker);
        }
    }
}