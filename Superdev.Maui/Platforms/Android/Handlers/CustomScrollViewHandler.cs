using AndroidX.AppCompat.View;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.Android.Controls;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<CustomScrollView, CustomScrollViewHandler>;

    public class CustomScrollViewHandler : ScrollViewHandler
    {
        private global::Android.Views.OverScrollMode originalOverScrollMode;

        public new static readonly PM Mapper = new PM(ScrollViewHandler.Mapper)
        {
            [nameof(CustomScrollView.IsBounceEnabled)] = MapIsBounceEnabled,
            [nameof(CustomScrollView.IsScrollEnabled)] = MapIsScrollEnabled,
            [nameof(CustomScrollView.IsHorizontalScrollbarVisible)] = MapIsHorizontalScrollbarVisible,
            [nameof(CustomScrollView.IsVerticalScrollbarVisible)] = MapIsVerticalScrollbarVisible,
        };

        public CustomScrollViewHandler()
            : base(Mapper)
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

        private static void MapIsHorizontalScrollbarVisible(CustomScrollViewHandler handler, CustomScrollView view)
        {
            handler.PlatformView.HorizontalScrollBarEnabled = view.IsHorizontalScrollbarVisible;
        }

        private static void MapIsVerticalScrollbarVisible(CustomScrollViewHandler handler, CustomScrollView view)
        {
            handler.PlatformView.VerticalScrollBarEnabled = view.IsVerticalScrollbarVisible;
        }
    }
}