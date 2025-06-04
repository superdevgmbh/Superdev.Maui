using Android.OS;
using Android.Views;
using Microsoft.Maui.Platform;
using Superdev.Maui.Services;
using Color = Microsoft.Maui.Graphics.Color;
using Window = Android.Views.Window;

namespace Superdev.Maui.Platforms.Services
{
    public class StatusBarService : IStatusBarService
    {
        private static readonly Lazy<IStatusBarService> Implementation = new Lazy<IStatusBarService>(CreateStatusBar, LazyThreadSafetyMode.PublicationOnly);

        public static IStatusBarService Current => Implementation.Value;

        private static IStatusBarService CreateStatusBar()
        {
            return new StatusBarService();
        }

        private StatusBarService()
        {
        }

        public void SetColor(Color color)
        {
            var currentActivity = Platform.CurrentActivity;
            var window = currentActivity.Window;
            this.SetStatusBarColor(window, color.ToPlatform());
        }

        private void SetStatusBarColor(Window window, AColor color)
        {
            window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            window.SetStatusBarColor(color);
        }

        public void SetStatusBarMode(StatusBarStyle statusBarStyle)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                return;
            }

            var currentActivity = Platform.CurrentActivity;
            var window = currentActivity.Window;
            var windowLightStatusBar = statusBarStyle == StatusBarStyle.Light;

            var newUiVisibility = (int)window.DecorView.SystemUiVisibility;
            if (windowLightStatusBar)
            {
                newUiVisibility |= (int)SystemUiFlags.LightStatusBar;
            }
            else
            {
                newUiVisibility &= ~(int)SystemUiFlags.LightStatusBar;
            }

            window.DecorView.SystemUiVisibility = (StatusBarVisibility)newUiVisibility;
        }
    }
}