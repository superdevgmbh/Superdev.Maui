using System.Diagnostics;
using Android.App;
using Android.Content;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Utils;

namespace Superdev.Maui.Platforms.Handlers
{
    public class DatePickerHandler : Microsoft.Maui.Handlers.DatePickerHandler
    {
        private const int PositiveButtonId = (int)DialogButtonType.Positive;
        private const int NegativeButtonId = (int)DialogButtonType.Negative;

        private DatePickerDialog dialog;

        public DatePickerHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public DatePickerHandler()
            : base(Mapper)
        {
        }

        protected override MauiDatePicker CreatePlatformView()
        {
            var mauiDatePicker = new MauiDatePicker(this.Context)
            {
                ShowPicker = this.ShowDialog,
                HidePicker = this.HideAndDisposeDialog
            };

            return mauiDatePicker;
        }

        protected override DatePickerDialog CreateDatePickerDialog(int year, int month, int day)
        {
            var dialog = base.CreateDatePickerDialog(year, month, day);

            this.UpdatePositiveButton(this.VirtualView, dialog);
            this.UpdateNegativeButton(this.VirtualView, dialog);

            return dialog;
        }

        protected virtual DateTime GetSelectedDate()
        {
            return this.VirtualView.Date;
        }

        private void ShowDialog()
        {
            if (this.dialog is { IsShowing: true })
            {
                return;
            }

            var date = this.GetSelectedDate();
            this.dialog = this.CreateDatePickerDialog(date.Year, date.Month - 1, date.Day);
            this.dialog.Show();
        }

        private void HideAndDisposeDialog()
        {
            if (this.dialog != null)
            {
                this.dialog.Hide();
                this.dialog.Dispose();
                this.dialog = null;
            }
        }

        private void UpdatePositiveButton(IDatePicker datePicker, DatePickerDialog dialog)
        {
            var positiveButtonText = GetPositiveButtonText((BindableObject)datePicker);
            Debug.WriteLine($"UpdatePositiveButton --> positiveButtonText={positiveButtonText}");

            dialog.SetButton(PositiveButtonId, positiveButtonText, (_, a) =>
            {
                if (a.Which == PositiveButtonId)
                {
                    this.HandlePositiveButtonTap(datePicker, dialog);
                }
            });
        }

        protected virtual void HandlePositiveButtonTap(IDatePicker datePicker, DatePickerDialog dialog)
        {
            datePicker.Date = dialog.DatePicker.DateTime;
        }

        private void UpdateNegativeButton(IDatePicker datePicker, DatePickerDialog dialog)
        {
            var negativeButtonText = GetNegativeButtonText((BindableObject)datePicker);
            Debug.WriteLine($"UpdateNegativeButton --> negativeButtonText={negativeButtonText}");

            dialog.SetButton(NegativeButtonId, negativeButtonText, (_, a) =>
            {
                if (a.Which == NegativeButtonId)
                {
                    this.HandleNegativeButtonTab(datePicker, a);
                }
            });
        }

        protected virtual void HandleNegativeButtonTab(IDatePicker datePicker, DialogClickEventArgs a)
        {
        }

        protected override void ConnectHandler(MauiDatePicker platformView)
        {
#if !NET9_0_OR_GREATER
            this.VirtualView.AddCleanUpEvent();
#endif
            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(MauiDatePicker platformView)
        {
            this.HideAndDisposeDialog();
            base.DisconnectHandler(platformView);
        }

        private static string GetPositiveButtonText(BindableObject element)
        {
            if (DialogExtensions.GetPositiveButtonText(element) is string positiveButtonText)
            {
                return positiveButtonText;
            }

            positiveButtonText = AApplication.Context.Resources.GetString(AR.String.Ok);
            return positiveButtonText;
        }

        private static string GetNegativeButtonText(BindableObject element)
        {
            if (DialogExtensions.GetNegativeButtonText(element) is string negativeButtonText)
            {
                return negativeButtonText;
            }

            negativeButtonText = AApplication.Context.Resources.GetString(AR.String.Cancel);
            return negativeButtonText;
        }
    }
}