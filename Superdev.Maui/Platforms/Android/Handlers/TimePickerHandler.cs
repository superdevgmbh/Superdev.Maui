using System.Diagnostics;
using Android.App;
using Android.Content;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using DateFormat = Android.Text.Format.DateFormat;
using TimePicker = Microsoft.Maui.Controls.TimePicker;

namespace Superdev.Maui.Platforms.Handlers
{
    public class TimePickerHandler : Microsoft.Maui.Handlers.TimePickerHandler
    {
        private const int PositiveButtonId = (int)DialogButtonType.Positive;
        private const int NegativeButtonId = (int)DialogButtonType.Negative;

        public TimePickerHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public TimePickerHandler()
            : base(Mapper)
        {
        }

        public new ITimePicker? VirtualView => ((ElementHandler)this).VirtualView as ITimePicker;

        public new MauiTimePicker? PlatformView => ((ElementHandler)this).PlatformView as MauiTimePicker;

        private bool Use24HourView => this.VirtualView is TimePicker timePicker && ((DateFormat.Is24HourFormat(this.PlatformView?.Context) &&
                                                                                     timePicker.Format == "t") || timePicker.Format == "HH:mm");

        protected override TimePickerDialog CreateTimePickerDialog(int hours, int minutes)
        {
            var dialog = new TimePickerDialog(this.Context, this.OnTimeSetCallback, hours, minutes, this.Use24HourView);

            if (this.VirtualView is TimePicker timePicker)
            {
                this.UpdatePositiveButton(timePicker, dialog);
                this.UpdateNegativeButton(timePicker, dialog);
            }

            return dialog;
        }


        private void OnTimeSetCallback(object? obj, TimePickerDialog.TimeSetEventArgs args)
        {
            if (this.VirtualView is not ITimePicker timePicker || this.PlatformView == null)
            {
                return;
            }

            this.HandlePositiveButtonTap(new TimeSpan(args.HourOfDay, args.Minute, 0));
            timePicker.IsFocused = false;

            // if (_dialog != null)
            // {
            //     _dialog = null;
            // }
        }


        private void UpdatePositiveButton(TimePicker timePicker, TimePickerDialog dialog)
        {
            var positiveButtonText = GetPositiveButtonText(timePicker);
            Debug.WriteLine($"UpdatePositiveButton --> positiveButtonText={positiveButtonText}");

            dialog.SetButton(PositiveButtonId, positiveButtonText, (_, a) =>
            {
                // Is handled via OnTimeSetCallback
            });
        }

        protected virtual void HandlePositiveButtonTap(TimeSpan time)
        {
            if (this.VirtualView is not TimePicker timePicker)
            {
                return;
            }

            timePicker.Time = time;
        }

        private void UpdateNegativeButton(TimePicker timePicker, TimePickerDialog dialog)
        {
            var negativeButtonText = GetNegativeButtonText(timePicker);
            Debug.WriteLine($"UpdateNegativeButton --> negativeButtonText={negativeButtonText}");

            dialog.SetButton(NegativeButtonId, negativeButtonText, (_, a) =>
            {
                if (a.Which == NegativeButtonId)
                {
                    this.HandleNegativeButtonTab(timePicker, a);
                }
            });
        }

        protected virtual void HandleNegativeButtonTab(ITimePicker timePicker, DialogClickEventArgs a)
        {
        }

        private static string GetPositiveButtonText(BindableObject element)
        {
            if (DialogExtensions.GetPositiveButtonText(element) is string positiveButtonText)
            {
                return positiveButtonText;
            }

            positiveButtonText = AApplication.Context.Resources!.GetString(AR.String.Ok);
            return positiveButtonText;
        }

        private static string GetNegativeButtonText(BindableObject element)
        {
            if (DialogExtensions.GetNegativeButtonText(element) is string negativeButtonText)
            {
                return negativeButtonText;
            }

            negativeButtonText = AApplication.Context.Resources!.GetString(AR.String.Cancel);
            return negativeButtonText;
        }
    }
}