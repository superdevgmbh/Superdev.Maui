using CoreGraphics;
using Foundation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using WebKit;
using Superdev.Maui.Platforms.Extensions;

namespace Superdev.Maui.Platforms.Handlers
{
    internal class CustomMauiWKWebView : MauiWKWebView
    {
        public CustomMauiWKWebView(WebViewHandler handler)
            : base(handler)
        {
        }

        public CustomMauiWKWebView(CGRect frame, WebViewHandler handler)
            : base(frame, handler)
        {
        }

        public CustomMauiWKWebView(CGRect frame, WebViewHandler handler, WKWebViewConfiguration configuration)
            : base(frame, handler, configuration)
        {
        }

        public IDictionary<string,string> AdditionalHttpHeaders { get; set; }

        public override WKNavigation LoadRequest(NSUrlRequest request)
        {
            var webRequest = new NSMutableUrlRequest(request.Url);

            if (this.AdditionalHttpHeaders is IDictionary<string, string> headers)
            {
                var requestHeaders = headers.ToNSDictionary();
                webRequest.Headers = requestHeaders;
            }

            return base.LoadRequest(webRequest);
        }
    }
}