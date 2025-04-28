using Android.Widget;
using Superdev.Maui.Services;
using Application = Android.App.Application;

namespace Superdev.Maui.Platforms.Services
{
    public class ToastService : IToastService
    {
        public void LongAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
        }

        public void ShortAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
        }
    }
}