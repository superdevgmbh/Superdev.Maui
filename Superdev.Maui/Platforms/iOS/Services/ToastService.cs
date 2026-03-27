using Foundation;
using Superdev.Maui.Services;
using UIKit;

namespace Superdev.Maui.Platforms.Services
{
    public class ToastService : IToastService
    {
        private static readonly Lazy<IToastService> Implementation = new Lazy<IToastService>(CreateInstance, LazyThreadSafetyMode.PublicationOnly);

        public static IToastService Current => Implementation.Value;

        private static IToastService CreateInstance()
        {
            return new ToastService();
        }

        private ToastService()
        {
        }

        private const double LongDelay = 3.5;
        private const double ShortDelay = 2.0;

        private NSTimer? alertDelay;
        private UIAlertController? alert;

        public void LongAlert(string message)
        {
            this.ShowAlert(message, LongDelay);
        }

        public void ShortAlert(string message)
        {
            this.ShowAlert(message, ShortDelay);
        }

        private void ShowAlert(string message, double seconds)
        {
            this.alertDelay = NSTimer.CreateScheduledTimer(seconds, _ => { this.DismissMessage(); });
            this.alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(this.alert, true, null);
        }

        private void DismissMessage()
        {
            this.alert?.DismissViewController(true, null);
            this.alertDelay?.Dispose();
        }
    }
}