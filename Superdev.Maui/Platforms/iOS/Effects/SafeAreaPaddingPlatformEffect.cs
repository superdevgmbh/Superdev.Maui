using System.ComponentModel;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls.Platform;
using Superdev.Maui.Effects;
using Superdev.Maui.Extensions;
using UIKit;

namespace Superdev.Maui.Platforms.Effects
{
    public class SafeAreaPaddingPlatformEffect : PlatformEffect
    {
        protected static readonly ILogger Logger;

        private readonly Dictionary<UIInterfaceOrientation, UIEdgeInsets> orientationToInsetsMapping = new Dictionary<UIInterfaceOrientation, UIEdgeInsets>();
        private Thickness? originalPadding;
        private NSObject orientationObserver;

        static SafeAreaPaddingPlatformEffect()
        {
            Logger = IPlatformApplication.Current?.Services.GetService<ILogger<SafeAreaPaddingPlatformEffect>>()
                ?? NullLogger<SafeAreaPaddingPlatformEffect>.Instance;
        }

        /// <summary>
        /// Debug messages are written only while <see cref="SafeAreaPadding.LoggingEnabled"/> is on.
        /// </summary>
        private protected static bool IsDebugLogging => SafeAreaPadding.LoggingEnabled && Logger.IsEnabled(LogLevel.Debug);

        protected override void OnAttached()
        {
            if (IsDebugLogging)
            {
                Logger.LogDebug("SafeAreaPaddingEffect attached to {Element}", this.Element?.GetType().GetFormattedName());
            }

            this.orientationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidChangeStatusBarOrientationNotification, this.DeviceOrientationDidChange);
            UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();

            this.UpdatePadding();
        }

        private void DeviceOrientationDidChange(NSNotification obj)
        {
            if (IsDebugLogging)
            {
                Logger.LogDebug("SafeAreaPaddingEffect saw the orientation change on {Element}", this.Element?.GetType().GetFormattedName());
            }

            this.UpdatePadding();
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(args);

            if (args.PropertyName == SafeAreaPadding.SafeAreaInsetsProperty.PropertyName)
            {
                this.UpdatePadding();
            }
            else if (args.PropertyName == SafeAreaPadding.LayoutProperty.PropertyName)
            {
                this.UpdatePadding();
            }
            else if (args.PropertyName == SafeAreaPadding.ShouldIncludeStatusBarProperty.PropertyName)
            {
                this.UpdatePadding();
            }
        }

        private void SetSafeArea(Element element, SafeAreaPaddingLayout safeAreaPaddingLayout, Thickness safeAreaInsets, bool includeStatusBar)
        {
            if (element is Layout layout)
            {
                if (this.originalPadding == null)
                {
                    this.originalPadding = layout.Padding;
                }

                layout.Padding = this.GetSafeAreaPadding(this.originalPadding.Value, safeAreaPaddingLayout, safeAreaInsets, includeStatusBar);
                LogPadding(element, layout.Padding);
            }

            if (element is Page page)
            {
                if (this.originalPadding == null)
                {
                    this.originalPadding = page.Padding;
                }

                page.Padding = this.GetSafeAreaPadding(this.originalPadding.Value, safeAreaPaddingLayout, safeAreaInsets, includeStatusBar);
                LogPadding(element, page.Padding);
            }
        }

        private static void LogPadding(Element element, Thickness padding)
        {
            if (IsDebugLogging)
            {
                Logger.LogDebug("SafeAreaPaddingEffect set the padding of {Element} to {Left}, {Top}, {Right}, {Bottom}",
                    element.GetType().GetFormattedName(), padding.Left, padding.Top, padding.Right, padding.Bottom);
            }
        }

        private void UpdatePadding()
        {
            var layout = SafeAreaPadding.GetLayout(this.Element);
            var safeAreaInsets = SafeAreaPadding.GetSafeAreaInsets(this.Element);
            var includeStatusBar = SafeAreaPadding.GetShouldIncludeStatusBar(this.Element);
            this.SetSafeArea(this.Element, layout, safeAreaInsets, includeStatusBar);
        }

        protected virtual Thickness GetSafeAreaPadding(Thickness originalPadding, SafeAreaPaddingLayout safeAreaPaddingLayout, Thickness safeAreaInsets, bool includeStatusBar)
        {
            var orientation = UIApplication.SharedApplication.StatusBarOrientation;
            UIEdgeInsets insets;
            //bool hasInsets;
            if (this.orientationToInsetsMapping.TryGetValue(orientation, out var cachedInsets))
            {
                insets = cachedInsets;
            }
            else
            {
                insets = this.SafeAreaInsets;
                this.orientationToInsetsMapping.Add(orientation, insets);

                //hasInsets = GetHasInsets(orientation, insets);
            }

            int topPadding = includeStatusBar ? (int)(UIApplication.SharedApplication?.StatusBarFrame.Height ?? 20.0) : 0;

            Thickness safeAreaPadding;
            //if (hasInsets) // iPhone X
            {
                safeAreaPadding = new Thickness(
                    originalPadding.Left + insets.Left + safeAreaInsets.Left,
                    originalPadding.Top + insets.Top + safeAreaInsets.Top + topPadding,
                    originalPadding.Right + insets.Right + safeAreaInsets.Right,
                    originalPadding.Bottom + insets.Bottom + safeAreaInsets.Bottom);
            }
            //else
            //{
            //    safeAreaPadding = new Thickness(
            //        originalPadding.Left,
            //        originalPadding.Top + topPadding,
            //        originalPadding.Right,
            //        originalPadding.Bottom);
            //}

            if (safeAreaPaddingLayout != null)
            {
                safeAreaPadding = safeAreaPaddingLayout.Transform(safeAreaPadding);
            }

            return safeAreaPadding;
        }

        protected override void OnDetached()
        {
            if (IsDebugLogging)
            {
                Logger.LogDebug("SafeAreaPaddingEffect detached from {Element}", this.Element?.GetType().GetFormattedName());
            }

            if (this.Element is Layout layout && this.originalPadding != null)
            {
                layout.Padding = this.originalPadding.Value;
            }
            else if (this.Element is Page page && this.originalPadding != null)
            {
                page.Padding = this.originalPadding.Value;
            }

            this.originalPadding = null;
            NSNotificationCenter.DefaultCenter.RemoveObserver(this.orientationObserver);

            // Paired with BeginGeneratingDeviceOrientationNotifications in OnAttached; the notifications are
            // reference counted by UIKit, so every attached effect has to end what it began.
            UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
        }

        private UIEdgeInsets SafeAreaInsets => UIApplication.SharedApplication.Windows[0].SafeAreaInsets;
    }
}