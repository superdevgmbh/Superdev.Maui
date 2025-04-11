using System.ComponentModel;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls.Platform;
using Superdev.Maui.Effects;
using Superdev.Maui.Extensions;
using UIKit;

namespace Superdev.Maui.Platform.Effects
{
    public class SafeAreaPaddingPlatformEffect : PlatformEffect
    {
        private const bool LoggingEnabled
#if DEBUG
            = false;
#else
            = false;
#endif

        protected static readonly ILogger Logger;

        private readonly Dictionary<UIInterfaceOrientation, UIEdgeInsets> orientationToInsetsMapping = new Dictionary<UIInterfaceOrientation, UIEdgeInsets>();
        private Thickness? originalPadding;
        private NSObject orientationObserver;

        static SafeAreaPaddingPlatformEffect()
        {
            Logger = LoggingEnabled
                ? IPlatformApplication.Current.Services.GetService<ILogger<TintImagePlatformEffect>>()
                : new NullLogger<TintImagePlatformEffect>();
        }

        protected override void OnAttached()
        {
            Logger.LogDebug($"SafeAreaPaddingEffect.OnAttached to {this.Element?.GetType().GetFormattedName()}");

            this.orientationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidChangeStatusBarOrientationNotification, this.DeviceOrientationDidChange);
            UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();

            this.UpdatePadding();
        }

        private void DeviceOrientationDidChange(NSNotification obj)
        {
            Logger.LogDebug($"SafeAreaPaddingEffect.DeviceOrientationDidChange on {this.Element?.GetType().GetFormattedName()}");
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
                Logger.LogDebug($"SafeAreaPaddingEffect.SetSafeArea set {element.GetType().GetFormattedName()}.Padding={{{layout.Padding.Left}, {layout.Padding.Top}, {layout.Padding.Right}, {layout.Padding.Bottom}}}");
            }

            if (element is Page page)
            {
                if (this.originalPadding == null)
                {
                    this.originalPadding = page.Padding;
                }

                page.Padding = this.GetSafeAreaPadding(this.originalPadding.Value, safeAreaPaddingLayout, safeAreaInsets, includeStatusBar);
                Logger.LogDebug($"SafeAreaPaddingEffect.SetSafeArea set {element.GetType().GetFormattedName()}.Padding={{{page.Padding.Left}, {page.Padding.Top}, {page.Padding.Right}, {page.Padding.Bottom}}}");
            }
        }

        private void UpdatePadding()
        {
            Logger.LogDebug($"SafeAreaPaddingEffect.UpdatePadding for {this.Element.GetType().GetFormattedName()}");
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

        private bool GetHasInsets(UIInterfaceOrientation orientation, UIEdgeInsets insets)
        {
            bool hasInsets;

            switch (orientation)
            {
                case UIInterfaceOrientation.Portrait:
                case UIInterfaceOrientation.PortraitUpsideDown:
                    hasInsets = insets.Top > 0 || insets.Bottom > 0;
                    break;
                case UIInterfaceOrientation.LandscapeLeft:
                case UIInterfaceOrientation.LandscapeRight:
                    hasInsets = insets.Left > 0 || insets.Right > 0;
                    break;
                default:
                    hasInsets = insets.Top > 0 || insets.Bottom > 0;
                    break;
            }

            Logger.LogDebug($"SafeAreaPaddingEffect.GetHasInsets returns hasInsets={hasInsets} for {this.Element.GetType().GetFormattedName()}");
            return hasInsets;
        }

        protected override void OnDetached()
        {
            Logger.LogDebug($"SafeAreaPaddingEffect.OnDetached from {this.Element?.GetType().GetFormattedName()}");

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
        }

        private UIEdgeInsets SafeAreaInsets
        {
            get
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                {
                    var insets = UIApplication.SharedApplication.Windows[0].SafeAreaInsets;
                    return insets;
                }

                return new UIEdgeInsets(0, 0, 0, 0);
            }
        }
    }
}