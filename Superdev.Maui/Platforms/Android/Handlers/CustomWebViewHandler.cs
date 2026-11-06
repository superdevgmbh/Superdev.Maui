using Android.Webkit;
using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<CustomWebView, CustomWebViewHandler>;

    public class CustomWebViewHandler : WebViewHandler
    {
        private new static readonly PM Mapper = new PM(WebViewHandler.Mapper)
        {
            [nameof(CustomWebView.Headers)] = MapHeaders,
            [nameof(CustomWebView.AllowMixedContent)] = MapAllowMixedContent
        };

        public CustomWebViewHandler()
            : base(Mapper)
        {
        }

        protected override AWebView CreatePlatformView()
        {
            var platformView = new CustomMauiWebView(this, this.Context)
            {
                LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
            };

            // TODO: Check this configuration and compare:
            // https://github.com/bitfoundation/bitplatform/blob/3cd0cb02ff2fa37c17b1cb7464c7e219e7d23b20/src/BlazorUI/Demo/Client/Bit.BlazorUI.Demo.Client.Maui/MauiProgram.cs#L125
            platformView.Settings.JavaScriptEnabled = true;
            platformView.Settings.DomStorageEnabled = true;
            platformView.Settings.SetSupportMultipleWindows(false);

            platformView.Settings.JavaScriptCanOpenWindowsAutomatically = true;

            // The mixed content mode is not set here: it follows CustomWebView.AllowMixedContent, which the
            // mapper applies on connect and on every later change.

#if DEBUG
            global::Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
#else
            global::Android.Webkit.WebView.SetWebContentsDebuggingEnabled(false);
#endif

            return platformView;
        }

        private static void MapHeaders(CustomWebViewHandler customWebViewHandler, CustomWebView customWebView)
        {
            customWebViewHandler.UpdateHeaders(customWebView);
        }

        private static void MapAllowMixedContent(CustomWebViewHandler customWebViewHandler, CustomWebView customWebView)
        {
            // NeverAllow is the platform's own default on every API level this library supports. Overriding
            // it with AlwaysAllow let a secure page silently load insecure scripts in every consumer of the
            // control, so it is opt-in per control instead.
            customWebViewHandler.PlatformView.Settings.MixedContentMode = customWebView.AllowMixedContent
                ? MixedContentHandling.AlwaysAllow
                : MixedContentHandling.NeverAllow;
        }

        private void UpdateHeaders(CustomWebView customWebView)
        {
            if (this.PlatformView is CustomMauiWebView webView)
            {
                webView.AdditionalHttpHeaders = customWebView.Headers;
            }
        }

        protected override void DisconnectHandler(AWebView platformView)
        {
            // The virtual view is deliberately not disposed here. Disconnecting a handler does not end
            // the life of the control it was connected to, and disposing it detaches the command
            // forwarding permanently, so a control which is reconnected later stops raising its commands.
            base.DisconnectHandler(platformView);
        }
    }
}