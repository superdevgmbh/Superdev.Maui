using System.Diagnostics;
using Android.App;
using Android.Content;
using Android.Text.Format;
using Superdev.Maui.Controls;

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

        private bool Use24HourView => this.VirtualView != null && ((DateFormat.Is24HourFormat(this.PlatformView?.Context)
                                                                    && this.VirtualView.Format == "t") || this.VirtualView.Format == "HH:mm");

        protected override TimePickerDialog CreateTimePickerDialog(int hours, int minutes)
        {
            void OnTimeSetCallback(object obj, TimePickerDialog.TimeSetEventArgs args)
            {
                if (this.VirtualView == null || this.PlatformView == null)
                {
                    return;
                }

                this.HandlePositiveButtonTap(new TimeSpan(args.HourOfDay, args.Minute, 0));
                this.VirtualView.IsFocused = false;

                // if (_dialog != null)
                // {
                //     _dialog = null;
                // }
            }

            var dialog = new TimePickerDialog(this.Context!, OnTimeSetCallback, hours, minutes, this.Use24HourView);

            this.UpdatePositiveButton(this.VirtualView, dialog);
            this.UpdateNegativeButton(this.VirtualView, dialog);

            return dialog;
        }

        private void UpdatePositiveButton(ITimePicker timePicker, TimePickerDialog dialog)
        {
            var positiveButtonText = GetPositiveButtonText((BindableObject)timePicker);
            Debug.WriteLine($"UpdatePositiveButton --> positiveButtonText={positiveButtonText}");

            dialog.SetButton(PositiveButtonId, positiveButtonText, (_, a) =>
            {
                // Is handled via OnTimeSetCallback
            });
        }

        protected virtual void HandlePositiveButtonTap(TimeSpan time)
        {
            this.VirtualView.Time = time;
        }

        private void UpdateNegativeButton(ITimePicker timePicker, TimePickerDialog dialog)
        {
            var negativeButtonText = GetNegativeButtonText((BindableObject)timePicker);
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