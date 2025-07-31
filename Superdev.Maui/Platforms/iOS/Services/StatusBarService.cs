using Foundation;
using Microsoft.Maui.Platform;
using Superdev.Maui.Services;
using UIKit;

namespace Superdev.Maui.Platforms.Services
{
    public class StatusBarService : IStatusBarService
    {
        private UIColor uiColor;

        private static readonly Lazy<IStatusBarService> Implementation = new Lazy<IStatusBarService>(CreateStatusBar, LazyThreadSafetyMode.PublicationOnly);

        public static IStatusBarService Current => Implementation.Value;

        private static IStatusBarService CreateStatusBar()
        {
            return new StatusBarService();
        }

        private StatusBarService()
        {
            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidChangeStatusBarOrientationNotification, this.DidChangeStatusBarOrientationNotification);
        }

        private void DidChangeStatusBarOrientationNotification(NSNotification obj)
        {
            SetStatusBarColor(this.uiColor);
        }

        public void SetStatusBarColor(Color color)
        {
            var uiColor = this.uiColor = color.ToPlatform();
            SetStatusBarColor(uiColor);
        }

        private static void SetStatusBarColor(UIColor uiColor)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                var statusBar = new UIView(UIApplication.SharedApplication.KeyWindow.WindowScene.StatusBarManager.StatusBarFrame);
                statusBar.BackgroundColor = uiColor;
                UIApplication.SharedApplication.KeyWindow.AddSubview(statusBar);
            }
            else
            {
                var statusBar = UIApplication.SharedApplication.ValueForKey(new NSString("statusBar")) as UIView;
                if (statusBar.RespondsToSelector(new ObjCRuntime.Selector("setBackgroundColor:")))
                {
                    statusBar.BackgroundColor = uiColor;
                }
            }
        }

        public void SetNavigationBarColor(Color color)
        {
            // API does not exist on iOS.
        }

        public void SetStyle(StatusBarStyle statusBarStyle)
        {
            var uiStatusBarStyle = statusBarStyle switch
            {
                StatusBarStyle.Default => UIStatusBarStyle.Default,
                StatusBarStyle.Light => UIStatusBarStyle.LightContent,
                StatusBarStyle.Dark => UIStatusBarStyle.DarkContent,
                _ => throw new NotSupportedException($"{nameof(StatusBarStyle)} {statusBarStyle} is not yet supported on iOS")
            };

            UIApplication.SharedApplication.SetStatusBarStyle(uiStatusBarStyle, false);
        }
    }
}