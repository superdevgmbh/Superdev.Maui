using System.Diagnostics;
using Foundation;
using Microsoft.Maui.Converters;
using Superdev.Maui.Controls;
using Superdev.Maui.Extensions;
using UIKit;
using ContentView = Microsoft.Maui.Platform.ContentView;
using NavigationPage = Superdev.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<ContentPage, ContentPageHandler>;

    public class ContentPageHandler : Superdev.Maui.Handlers.PageHandler
    {
        private static readonly ThicknessTypeConverter ThicknessTypeConverter = new ThicknessTypeConverter();

        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.PageHandler.Mapper)
        {
            [PageExtensions.HasKeyboardOffset] = UpdateHasKeyboardOffset,
            [NavigationPage.SwipeBackEnabledProperty.PropertyName] = UpdateSwipeBackEnabled
        };

        private Thickness? contentOriginalMargin;

        private NSObject? onKeyboardShowObserver;
        private NSObject? onKeyboardHideObserver;
        private double keyboardHeight;

        public ContentPageHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public ContentPageHandler()
            : base(Mapper)
        {
        }

        private static void UpdateHasKeyboardOffset(ContentPageHandler contentPageHandler, ContentPage contentPage)
        {
            var hasKeyboardOffset = PageExtensions.GetHasKeyboardOffset(contentPage);
            if (hasKeyboardOffset != null)
            {
                if (contentPageHandler.onKeyboardShowObserver == null)
                {
                    contentPageHandler.onKeyboardShowObserver = UIKeyboard.Notifications.ObserveWillShow((_, args) =>
                    {
                        var keyboardFrame = UIKeyboard.FrameEndFromNotification(args.Notification);
                        contentPageHandler.keyboardHeight = keyboardFrame.Height;
                        contentPageHandler.OnShowKeyboard(contentPageHandler.keyboardHeight);
                    });
                }

                if (contentPageHandler.onKeyboardHideObserver == null)
                {
                    contentPageHandler.onKeyboardHideObserver = UIKeyboard.Notifications.ObserveWillHide((_, _) =>
                    {
                        contentPageHandler.keyboardHeight = 0d;
                        contentPageHandler.OnHideKeyboard();
                    });
                }
            }
        }

        private void OnHideKeyboard()
        {
            this.RemoveKeyboardOffset();
        }

        private void OnShowKeyboard(double keyboardHeight)
        {
            if (this.VirtualView is not ContentPage contentPage)
            {
                return;
            }

            if (this.contentOriginalMargin != null)
            {
                return;
            }

            var hasKeyboardOffset = PageExtensions.GetHasKeyboardOffset(contentPage);
            if (hasKeyboardOffset == true)
            {
                this.ApplyKeyboardOffset(keyboardHeight);
            }
            else if (hasKeyboardOffset == false)
            {
                this.RemoveKeyboardOffset();
            }
        }

        private void ApplyKeyboardOffset(double keyboardHeight)
        {
            if (this.VirtualView is not ContentPage contentPage)
            {
                return;
            }

            // var selectedControl = this.PlatformView.FindFirstResponder();
            // var rootUiView = contentPage.Content.ToPlatform(this.MauiContext);
            //
            // var distance = selectedControl.GetOverlapDistance(rootUiView, keyboardHeight);

            if (keyboardHeight > 0d)
            {
                var contentMargin = contentPage.Content.Margin;
                this.contentOriginalMargin = contentMargin;

                var newMargin = new Thickness(
                    contentMargin.Left,
                    0,
                    contentMargin.Right,
                    keyboardHeight);

                Debug.WriteLine($"ApplyKeyboardOffset:{Environment.NewLine}" +
                                $"> contentPage: {GetContentPageName(contentPage)}{Environment.NewLine}" +
                                $"> keyboardHeight: {keyboardHeight}{Environment.NewLine}" +
                                $"> originalMargin: {ThicknessTypeConverter.ConvertToString(contentMargin)}{Environment.NewLine}" +
                                $"> newMargin: {ThicknessTypeConverter.ConvertToString(newMargin)}");

                contentPage.Content.Margin = newMargin;
            }
        }

        private void RemoveKeyboardOffset()
        {
            if (this.VirtualView is not ContentPage contentPage)
            {
                return;
            }

            if (this.contentOriginalMargin is Thickness contentMargin)
            {
                Debug.WriteLine($"RemoveKeyboardOffset:{Environment.NewLine}" +
                                $"> contentPage: {GetContentPageName(contentPage)}");

                contentPage.Content.Margin = contentMargin;

                this.contentOriginalMargin = null;
            }
        }

        private static string GetContentPageName(ContentPage contentPage)
        {
            return $"{contentPage.AutomationId ?? contentPage.GetType().Name}";
        }

        private static void UpdateSwipeBackEnabled(ContentPageHandler contentPageHandler, ContentPage contentPage)
        {
            if (contentPageHandler.ViewController?.NavigationController is { InteractivePopGestureRecognizer: UIGestureRecognizer gestureRecognizer })
            {
                var swipeBackEnabled = NavigationPage.GetSwipeBackEnabled(contentPage);

                Debug.WriteLine($"UpdateSwipeBackEnabled: SwipeBackEnabled={swipeBackEnabled} for Page={contentPage.GetType().GetFormattedName()}");
                gestureRecognizer.Enabled = swipeBackEnabled;
            }
        }

        protected override void ConnectHandler(ContentView platformView)
        {
            base.ConnectHandler(platformView);

            if (this.VirtualView is ContentPage contentPage)
            {
                contentPage.Loaded += this.OnPageLoaded;
            }
        }

        private void OnPageLoaded(object? sender, EventArgs e)
        {
            var contentPage = (ContentPage)sender!;
            UpdateSwipeBackEnabled(this, contentPage);
        }

        protected override void DisconnectHandler(ContentView platformView)
        {
            if (this.VirtualView is ContentPage contentPage)
            {
                contentPage.Loaded -= this.OnPageLoaded;
            }

            if (this.onKeyboardShowObserver != null)
            {
                this.onKeyboardShowObserver.Dispose();
                this.onKeyboardShowObserver = null;
            }

            if (this.onKeyboardHideObserver != null)
            {
                this.onKeyboardHideObserver.Dispose();
                this.onKeyboardHideObserver = null;
            }

            this.contentOriginalMargin = null;

            base.DisconnectHandler(platformView);
        }
    }
}