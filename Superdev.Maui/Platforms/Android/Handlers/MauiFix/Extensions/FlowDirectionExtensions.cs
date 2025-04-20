using ALayoutDirection = Android.Views.LayoutDirection;
using ATextDirection = Android.Views.TextDirection;
using AView = Android.Views.View;

namespace Superdev.Platforms.Android.Handlers.MauiFix.Extensions
{
    internal static class FlowDirectionExtensions
    {
        internal static ATextDirection ToTextDirection(this ALayoutDirection direction)
        {
            switch (direction)
            {
                case ALayoutDirection.Ltr:
                    return ATextDirection.Ltr;
                case ALayoutDirection.Rtl:
                    return ATextDirection.Rtl;
                default:
                    return ATextDirection.Inherit;
            }
        }
    }
}