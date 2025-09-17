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

        protected override void ConnectHandler(WKWebView platformView)
        {
            Debug.WriteLine("ConnectHandler");
#if !NET9_0_OR_GREATER
            this.VirtualView.AddCleanUpEvent();
#endif
            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(WKWebView platformView)
        {
            Debug.WriteLine("DisconnectHandler");
            base.DisconnectHandler(platformView);

            if (this.VirtualView is CustomWebView customWebView)
            {
                customWebView.Dispose();
            }
        }
    }
}