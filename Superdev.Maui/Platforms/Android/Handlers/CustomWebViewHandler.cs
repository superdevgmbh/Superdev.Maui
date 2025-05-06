using System.Diagnostics;
using Android.Webkit;
using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;
using Superdev.Maui.Utils;

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
            platformView.Settings.JavaScriptEnabled = true;
            platformView.Settings.MixedContentMode = MixedContentHandling.AlwaysAllow;

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.Kitkat)
            {
#if DEBUG
                global::Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
#else
                global::Android.Webkit.WebView.SetWebContentsDebuggingEnabled(false);
#endif
            }

            return platformView;
        }

        private static void MapHeaders(CustomWebViewHandler customWebViewHandler, CustomWebView customWebView)
        {
            customWebViewHandler.UpdateHeaders(customWebView);
        }

        private void UpdateHeaders(CustomWebView customWebView)
        {
            if (this.PlatformView is CustomMauiWebView webView)
            {
                webView.AdditionalHttpHeaders = customWebView.Headers;
            }
        }

        protected override void ConnectHandler(AWebView platformView)
        {
            Debug.WriteLine("ConnectHandler");
            this.VirtualView.AddCleanUpEvent();
            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(AWebView platformView)
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