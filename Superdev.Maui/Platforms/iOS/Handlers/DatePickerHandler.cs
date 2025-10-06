using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.iOS.Utils;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<DatePicker, DatePickerHandler>;

    public class DatePickerHandler : Microsoft.Maui.Handlers.DatePickerHandler
    {
        protected MauiDatePicker mauiDatePicker;

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

        protected UIDatePicker Picker { get => this.mauiDatePicker.InputView as UIDatePicker; }

        protected override MauiDatePicker CreatePlatformView()
        {
            this.mauiDatePicker = base.CreatePlatformView();
            this.SetupUIToolbar(this.mauiDatePicker);
            return this.mauiDatePicker;
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

        protected override void ConnectHandler(MauiDatePicker platformView)
        {
            base.ConnectHandler(platformView);

            this.mauiDatePicker.EditingDidEnd += this.OnEditingDidEnd;
        }

        private void OnEditingDidEnd(object sender, EventArgs e)
        {
            if (this.Picker is UIDatePicker picker)
            {
                var date = picker.Date.ToDateTime();
                this.OnEditingDidEnd(date);
            }
        }

        protected virtual void OnEditingDidEnd(DateTime date)
        {
            this.VirtualView.Date = date;
        }

        protected override void DisconnectHandler(MauiDatePicker platformView)
        {
            base.DisconnectHandler(platformView);

            if (this.mauiDatePicker != null)
            {
                this.mauiDatePicker.EditingDidEnd -= this.OnEditingDidEnd;
            }
        }
    }
}