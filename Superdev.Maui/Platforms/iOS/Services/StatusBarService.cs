using System;
using System.Threading;
using Foundation;
using Microsoft.Maui.Platform;
using Superdev.Maui.Services;
using UIKit;

namespace Superdev.Maui.Platforms.Services
{
    public class StatusBarService : IStatusBarService
    {
        private static readonly Lazy<IStatusBarService> Implementation =
            new Lazy<IStatusBarService>(CreateStatusBar, LazyThreadSafetyMode.PublicationOnly);

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
            var uiColor = color.ToPlatform();

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

        public void SetStatusBarMode(StatusBarStyle statusBarStyle)
        {
            switch (statusBarStyle)
            {
                case StatusBarStyle.Light:
                    UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.DarkContent, false);
                    break;
                case StatusBarStyle.Dark:
                    UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, false);
                    break;
            }
        }
    }
}