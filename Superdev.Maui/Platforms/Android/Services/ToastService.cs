using Android.Widget;
using Superdev.Maui.Services;
using Application = Android.App.Application;

namespace Superdev.Maui.Platforms.Services
{
    public class ToastService : IToastService
    {
        private static readonly Lazy<IToastService> Implementation = new Lazy<IToastService>(CreateToastService, LazyThreadSafetyMode.PublicationOnly);

        public static IToastService Current => Implementation.Value;

        private static IToastService CreateToastService()
        {
            return new ToastService();
        }

        private ToastService()
        {
        }

        public void LongAlert(string message)
        {
            var makeText = Toast.MakeText(Application.Context, message, ToastLength.Long);
            makeText?.Show();
        }

        public void ShortAlert(string message)
        {
            var makeText = Toast.MakeText(Application.Context, message, ToastLength.Short);
            makeText?.Show();
        }
    }
}