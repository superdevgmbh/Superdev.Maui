using AndroidX.AppCompat.View;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.Android.Controls;

namespace Superdev.Maui.Platforms.Android.Handlers
{
    public class CustomScrollViewHandler : ScrollViewHandler
    {
        private global::Android.Views.OverScrollMode originalOverScrollMode;

        public static IPropertyMapper<CustomScrollView, CustomScrollViewHandler> PropertyMapper = new PropertyMapper<CustomScrollView, CustomScrollViewHandler>(ScrollViewHandler.Mapper)
        {
            [nameof(CustomScrollView.IsBounceEnabled)] = MapIsBounceEnabled,
            [nameof(CustomScrollView.IsScrollEnabled)] = MapIsScrollEnabled,
        };

        public CustomScrollViewHandler() : base(PropertyMapper)
        {
        }

        protected override MauiScrollView CreatePlatformView()
        {
            var scrollView = new LockableMauiScrollView(
             new ContextThemeWrapper(this.MauiContext!.Context, Resource.Style.scrollViewTheme), null!, Resource.Attribute.scrollViewStyle)
            {
                ClipToOutline = true,
                FillViewport = true
            };

            return scrollView;
        }

        private static void MapIsBounceEnabled(CustomScrollViewHandler scrollViewHandler, CustomScrollView customScrollView)
        {
            scrollViewHandler.originalOverScrollMode = scrollViewHandler.PlatformView.OverScrollMode;

            scrollViewHandler.PlatformView.OverScrollMode = !customScrollView.IsBounceEnabled ? global::Android.Views.OverScrollMode.Never : scrollViewHandler.originalOverScrollMode;
        }

        private static void MapIsScrollEnabled(CustomScrollViewHandler handler, CustomScrollView view)
        {
            var lockableMauiScrollView = (LockableMauiScrollView)handler.PlatformView;
            lockableMauiScrollView.ScrollEnabled = view.IsScrollEnabled;
        }
    }
}
