using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;
using Superdev.Maui.Services;
using UIKit;

namespace Superdev.Maui.Platforms.Services
{
    public class StatusBarService : IStatusBarService
    {
        private const string ViewControllerBasedStatusBarAppearanceKey = "UIViewControllerBasedStatusBarAppearance";

        private static readonly Lazy<IStatusBarService> Implementation = new Lazy<IStatusBarService>(CreateStatusBar, LazyThreadSafetyMode.PublicationOnly);

        private readonly ILogger? logger;

        private UIColor? uiColor;

        /// <summary>
        /// The overlay which paints the status bar area. It is created once and reused, so that setting a
        /// colour - which also happens on every orientation change - does not stack up views on the window.
        /// </summary>
        private UIView? statusBarOverlay;

        private bool hasWarnedAboutStatusBarStyle;

        public static IStatusBarService Current => Implementation.Value;

        private static IStatusBarService CreateStatusBar()
        {
            return new StatusBarService();
        }

        private StatusBarService()
        {
            this.logger = IPlatformApplication.Current?.Services.GetService<ILogger<StatusBarService>>();

            NSNotificationCenter.DefaultCenter.AddObserver(
                UIApplication.DidChangeStatusBarOrientationNotification, this.DidChangeStatusBarOrientationNotification);
        }

        private void DidChangeStatusBarOrientationNotification(NSNotification obj)
        {
            this.ApplyStatusBarColor();
        }

        public void SetStatusBarColor(Color? color)
        {
            this.uiColor = color?.ToPlatform();
            this.ApplyStatusBarColor();
        }

        private void ApplyStatusBarColor()
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                var legacyStatusBar = UIApplication.SharedApplication.ValueForKey(new NSString("statusBar")) as UIView;
                if (legacyStatusBar != null && legacyStatusBar.RespondsToSelector(new ObjCRuntime.Selector("setBackgroundColor:")))
                {
                    legacyStatusBar.BackgroundColor = this.uiColor;
                }

                return;
            }

            var window = GetKeyWindow();
            if (window?.WindowScene?.StatusBarManager is not { } statusBarManager)
            {
                return;
            }

            if (this.statusBarOverlay == null)
            {
                this.statusBarOverlay = new UIView();
                window.AddSubview(this.statusBarOverlay);
            }
            else if (!ReferenceEquals(this.statusBarOverlay.Superview, window))
            {
                // The window was replaced, e.g. by a new scene.
                this.statusBarOverlay.RemoveFromSuperview();
                window.AddSubview(this.statusBarOverlay);
            }

            this.statusBarOverlay.Frame = statusBarManager.StatusBarFrame;
            this.statusBarOverlay.BackgroundColor = this.uiColor;
        }

        public void SetNavigationBarColor(Color? color)
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

            this.WarnIfStatusBarStyleIsIgnored();

            UIApplication.SharedApplication.SetStatusBarStyle(uiStatusBarStyle, false);

            // Ask the view controller hierarchy to re-read the style, so that the change is picked up
            // without waiting for the next appearance update.
            GetKeyWindow()?.RootViewController?.SetNeedsStatusBarAppearanceUpdate();
        }

        /// <summary>
        /// Reports the one configuration mistake which makes <see cref="SetStyle" /> do nothing at all.
        /// </summary>
        /// <remarks>
        /// iOS ignores the application-level status bar style unless the app opts out of
        /// view-controller-based status bar appearance in its <c>Info.plist</c>. A library cannot set that
        /// key on the application's behalf, so the next best thing is to say so instead of failing silently.
        /// </remarks>
        private void WarnIfStatusBarStyleIsIgnored()
        {
            if (this.hasWarnedAboutStatusBarStyle)
            {
                return;
            }

            this.hasWarnedAboutStatusBarStyle = true;

            var value = NSBundle.MainBundle.ObjectForInfoDictionary(ViewControllerBasedStatusBarAppearanceKey);
            var isViewControllerBased = value is not NSNumber number || number.BoolValue;

            if (isViewControllerBased)
            {
                this.logger?.LogWarning(
                    "SetStyle has no effect because {Key} is not set to false in Info.plist. " +
                    "Add <key>{Key}</key><false/> to the application's Info.plist to control the status bar style.",
                    ViewControllerBasedStatusBarAppearanceKey,
                    ViewControllerBasedStatusBarAppearanceKey);
            }
        }

        /// <summary>
        /// Returns the key window of the active scene. <see cref="UIApplication.KeyWindow" /> is deprecated
        /// and returns the wrong window in a multi-scene application.
        /// </summary>
        private static UIWindow? GetKeyWindow()
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                return UIApplication.SharedApplication.Delegate?.GetWindow();
            }

            foreach (var scene in UIApplication.SharedApplication.ConnectedScenes)
            {
                if (scene is not UIWindowScene windowScene)
                {
                    continue;
                }

                foreach (var window in windowScene.Windows)
                {
                    if (window.IsKeyWindow)
                    {
                        return window;
                    }
                }
            }

            return null;
        }
    }
}
