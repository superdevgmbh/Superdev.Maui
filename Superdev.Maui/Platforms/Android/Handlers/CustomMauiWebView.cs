using Android.Content;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Superdev.Maui.Platforms.Handlers
{
    public class CustomMauiWebView : MauiWebView
    {
        public CustomMauiWebView(WebViewHandler handler, Context context)
            : base(handler, context)
        {
        }

        public IDictionary<string, string> AdditionalHttpHeaders { get; set; }

        public override void LoadUrl(string url)
        {
            if (this.AdditionalHttpHeaders is IDictionary<string, string> additionalHttpHeaders)
            {
                base.LoadUrl(url, additionalHttpHeaders);
            }
            else
            {
                base.LoadUrl(url);
            }
        }
    }
}