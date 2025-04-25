using System;
using System.Threading;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Microsoft.Maui.Platform;
using Superdev.Maui.Services;
using Color = Microsoft.Maui.Graphics.Color;
using Window = Android.Views.Window;
using AColor = global::Android.Graphics.Color;

namespace Superdev.Maui.Platforms.Android.Services
{
    public class StatusBarService : IStatusBarService
    {
        private static readonly Lazy<IStatusBarService> Implementation = new Lazy<IStatusBarService>(CreateStatusBar, LazyThreadSafetyMode.PublicationOnly);

        public static IStatusBarService Current => Implementation.Value;

        private static IStatusBarService CreateStatusBar()
        {
            return new StatusBarService();
        }

        public void SetHexColor(string hexColor)
        {
            var color = Color.FromArgb(hexColor);
            this.SetColor(color);
        }

        public void SetColor(Color color)
        {
            var currentActivity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
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
            var currentActivity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            var window = currentActivity.Window;
            var windowLightStatusBar = statusBarStyle == StatusBarStyle.Light;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
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
}