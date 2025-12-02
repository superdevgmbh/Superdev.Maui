using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<DatePicker, DatePickerHandler>;

    public class DatePickerHandler : Microsoft.Maui.Handlers.DatePickerHandler
    {
        protected MauiDoneAccessoryView? inputAccessoryView;

        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.DatePickerHandler.Mapper)
        {
            [DialogExtensions.DoneButtonText] = MapDoneButtonText,
        };

        public DatePickerHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public DatePickerHandler()
            : base(Mapper)
        {
        }

        public new DatePicker? VirtualView => ((ElementHandler)this).VirtualView as DatePicker;

        public new MauiDatePicker? PlatformView => ((ElementHandler)this).PlatformView as MauiDatePicker;

        protected UIDatePicker? Picker => this.PlatformView?.InputView as UIDatePicker;

        protected override MauiDatePicker CreatePlatformView()
        {
            var mauiDatePicker = base.CreatePlatformView();

            this.inputAccessoryView = new MauiDoneAccessoryView();
            this.inputAccessoryView.SetDoneButtonAction(this.HandleDoneButton);
            mauiDatePicker.InputAccessoryView = this.inputAccessoryView;

            return mauiDatePicker;
        }

        protected override void ConnectHandler(MauiDatePicker platformView)
        {
            base.ConnectHandler(platformView);

            platformView.EditingDidEnd += this.OnEditingDidEnd;
        }

        protected override void DisconnectHandler(MauiDatePicker platformView)
        {
            platformView.EditingDidEnd -= this.OnEditingDidEnd;

            platformView.InputAccessoryView = null;
            this.inputAccessoryView?.Dispose();
            this.inputAccessoryView = null;

            base.DisconnectHandler(platformView);
        }

        private void HandleDoneButton()
        {
            var datePicker = this.VirtualView;
            var mauiDatePicker = this.PlatformView;
            var uiDatePicker = this.Picker;

            if (datePicker == null || uiDatePicker == null || mauiDatePicker == null)
            {
                return;
            }

            var date = uiDatePicker.Date.ToDateTime().Date;
            datePicker.Date = date;

            mauiDatePicker.ResignFirstResponder();
        }

        private static void MapDoneButtonText(DatePickerHandler datePickerHandler, DatePicker datePicker)
        {
            datePickerHandler.DoneButtonText(datePicker);
        }

        private void DoneButtonText(DatePicker datePicker)
        {
            if (this.PlatformView is not MauiDatePicker mauiDatePicker || this.inputAccessoryView == null)
            {
                return;
            }

            var doneButtonText = DialogExtensions.GetDoneButtonText(datePicker);
            mauiDatePicker.InputAccessoryView = MauiDoneAccessoryView.SetDoneButtonText(ref this.inputAccessoryView, doneButtonText);
        }

        private void OnEditingDidEnd(object? sender, EventArgs e)
        {
            if (this.Picker is not UIDatePicker picker)
            {
                return;
            }

            var date = picker.Date.ToDateTime();
            this.OnEditingDidEnd(date);
        }

        protected virtual void OnEditingDidEnd(DateTime date)
        {
            if (this.VirtualView is not DatePicker datePicker)
            {
                return;
            }

            datePicker.Date = date;
        }
    }
}