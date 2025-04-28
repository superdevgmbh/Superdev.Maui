using Android.App;
using Android.Content;
using Superdev.Maui.Controls;

namespace Superdev.Maui.Platforms.Handlers
{
    public class TimePickerHandler : Microsoft.Maui.Handlers.TimePickerHandler
    {
        protected override TimePickerDialog CreateTimePickerDialog(int hours, int minutes)
        {
            var dialog = base.CreateTimePickerDialog(hours, minutes);

            var positiveButtonText = GetPositiveButtonText((BindableObject)this.VirtualView);
            dialog.SetButton((int)DialogButtonType.Positive, positiveButtonText, (_, _) =>
            {
                // Is handled in Maui's TimePickerRenderer
            });

            var negativeButtonText = GetNegativeButtonText((BindableObject)this.VirtualView);
            dialog.SetButton((int)DialogButtonType.Negative, negativeButtonText, (_, _) =>
            {
                // Is handled in Maui's TimePickerRenderer
            });

            return dialog;
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