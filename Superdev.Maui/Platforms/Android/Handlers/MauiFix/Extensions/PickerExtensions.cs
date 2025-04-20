using Microsoft.Maui.Platform;

namespace Superdev.Platforms.Android.Handlers.MauiFix.Extensions
{
    public static class PickerExtensions
    {
        internal static void UpdatePicker(this MauiPicker platformPicker, IPicker picker)
        {
            platformPicker.Hint = picker.Title;

            if (picker.SelectedIndex == -1 || picker.SelectedIndex >= picker.GetCount())
            {
                platformPicker.Text = null;
            }
            else
            {
                platformPicker.Text = picker.GetItem(picker.SelectedIndex);
            }
        }

        internal static void UpdateFlowDirection(this AndroidX.AppCompat.App.AlertDialog alertDialog, MauiPicker platformPicker)
        {
            var platformLayoutDirection = platformPicker.LayoutDirection;

            // Propagate the MauiPicker LayoutDirection to the AlertDialog
            var dv = alertDialog.Window?.DecorView;

            if (dv is not null)
            {
                dv.LayoutDirection = platformLayoutDirection;
            }

            var lv = alertDialog?.ListView;

            if (lv is not null)
            {
                lv.LayoutDirection = platformLayoutDirection;
                lv.TextDirection = platformLayoutDirection.ToTextDirection();
            }
        }
    }
}