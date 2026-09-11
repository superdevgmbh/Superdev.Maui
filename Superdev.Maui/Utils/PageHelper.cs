using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Superdev.Maui.Utils
{
    internal static class PageHelper
    {
        internal static PageHandler CreatePageHandler(Page page, ContentPage contentPage)
        {
            var mauiContext = page.Handler?.MauiContext ?? throw new NullReferenceException(nameof(IMauiContext));
            page.AddLogicalChild(contentPage);
            return (PageHandler)contentPage.ToHandler(mauiContext);
        }
    }
}