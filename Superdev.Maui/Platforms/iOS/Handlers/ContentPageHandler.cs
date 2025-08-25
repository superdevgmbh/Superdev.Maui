using System.Diagnostics;
using Foundation;
using Superdev.Maui.Controls;
using UIKit;
using ContentView = Microsoft.Maui.Platform.ContentView;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<ContentPage, ContentPageHandler>;

    public class ContentPageHandler : Superdev.Maui.Handlers.PageHandler
    {
        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.PageHandler.Mapper)
        {
            [PageExtensions.HasKeyboardOffset] = UpdateHasKeyboardOffset
        };

        private double? contentOriginalHeight;
        private Thickness? contentOriginalMargin;

        private NSObject onKeyboardShowObserver;
        private NSObject onKeyboardHideObserver;

        public ContentPageHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
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
                    contentPageHandler.onKeyboardShowObserver = UIKeyboard.Notifications.ObserveWillShow((sender, args) =>
                    {
                        if (contentPageHandler.VirtualView is not ContentPage contentPage)
                        {
                            return;
                        }

                        if (contentPageHandler.contentOriginalHeight != null &&
                            contentPageHandler.contentOriginalMargin != null)
                        {
                            return;
                        }

                        var hasKeyboardOffset = PageExtensions.GetHasKeyboardOffset(contentPage);
                        if (hasKeyboardOffset == true)
                        {
                            var keyboardHeight = args.FrameEnd.Height;
                            ApplyKeyboardOffset(contentPageHandler, contentPage, keyboardHeight);
                        }
                        else if (hasKeyboardOffset == false)
                        {
                            RemoveKeyboardOffset(contentPageHandler, contentPage);
                        }
                    });
                }

                if (contentPageHandler.onKeyboardHideObserver == null)
                {
                    contentPageHandler.onKeyboardHideObserver = UIKeyboard.Notifications.ObserveWillHide((sender, args) =>
                    {
                        if (contentPageHandler.VirtualView is not ContentPage contentPage)
                        {
                            return;
                        }

                        RemoveKeyboardOffset(contentPageHandler, contentPage);
                    });
                }
            }
        }

        private static void ApplyKeyboardOffset(ContentPageHandler contentPageHandler, ContentPage contentPage, double keyboardHeight)
        {
            Debug.WriteLine($"ApplyKeyboardOffset: keyboardHeight={keyboardHeight}" +
                            $"{(contentPage.AutomationId != null ? $", contentPage={contentPage.AutomationId}" : "")}");

            var contentHeight = contentPage.Content.Height;
            var contentMargin = contentPage.Content.Margin;
            contentPageHandler.contentOriginalHeight = contentHeight;
            contentPageHandler.contentOriginalMargin = contentMargin;

            contentPage.Content.HeightRequest = contentHeight - keyboardHeight;
            contentPage.Content.Margin = new Thickness(0, 0, 0, keyboardHeight);
        }

        private static void RemoveKeyboardOffset(ContentPageHandler contentPageHandler, ContentPage contentPage)
        {
            if (contentPageHandler.contentOriginalHeight is double contentHeight &&
                contentPageHandler.contentOriginalMargin is Thickness contentMargin)
            {
                Debug.WriteLine($"RemoveKeyboardOffset" +
                                $"{(contentPage.AutomationId != null ? $": contentPage={contentPage.AutomationId}" : "")}");

                contentPage.Content.HeightRequest = contentHeight;
                contentPage.Content.Margin = contentMargin;

                contentPageHandler.contentOriginalHeight = null;
                contentPageHandler.contentOriginalMargin = null;
            }
        }

        protected override void DisconnectHandler(ContentView platformView)
        {
            base.DisconnectHandler(platformView);

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

            this.contentOriginalHeight = null;
            this.contentOriginalMargin = null;
        }
    }
}