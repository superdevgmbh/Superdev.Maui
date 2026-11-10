using System.Diagnostics;
using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;
using Superdev.Maui.Utils;
using WebKit;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<CustomWebView, CustomWebViewHandler>;

    public class CustomWebViewHandler : WebViewHandler
    {
        private new static readonly PM Mapper = new PM(WebViewHandler.Mapper)
        {
            [nameof(CustomWebView.Headers)] = MapHeaders
        };

        public CustomWebViewHandler()
            : base(Mapper)
        {
        }

        protected override WKWebView CreatePlatformView()
        {
            return new CustomMauiWKWebView(this);
        }

        private static void MapHeaders(CustomWebViewHandler customWebViewHandler, CustomWebView customWebView)
        {
            customWebViewHandler.UpdateHeaders(customWebView);
        }

        private void UpdateHeaders(CustomWebView customWebView)
        {
            if (this.PlatformView is CustomMauiWKWebView webView)
            {
                webView.AdditionalHttpHeaders = customWebView.Headers;
            }
        }

        protected override void DisconnectHandler(WKWebView platformView)
        {
            // The virtual view is deliberately not disposed here. Disconnecting a handler does not end
            // the life of the control it was connected to, and disposing it detaches the command
            // forwarding permanently, so a control which is reconnected later stops raising its commands.
            base.DisconnectHandler(platformView);
        }
    }
}