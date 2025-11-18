using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<CustomScrollView, CustomScrollViewHandler>;

    public class CustomScrollViewHandler : ScrollViewHandler
    {
        public new static readonly PM Mapper = new PM(ScrollViewHandler.Mapper)
        {
            [nameof(CustomScrollView.IsBounceEnabled)] = MapIsBounceEnabled,
            [nameof(CustomScrollView.IsScrollEnabled)] = MapIsScrollEnabled,
            [nameof(CustomScrollView.IsHorizontalScrollbarVisible)] = MapIsHorizontalScollbarVisible,
            [nameof(CustomScrollView.IsVerticalScrollbarVisible)] = MapIsVerticalScollbarVisible,
        };

        public CustomScrollViewHandler()
            : base(Mapper)
        {
        }

        private static void MapIsBounceEnabled(CustomScrollViewHandler scrollViewHandler, CustomScrollView customScrollView)
        {
            scrollViewHandler.PlatformView.Bounces = customScrollView.IsBounceEnabled;
        }

        private static void MapIsScrollEnabled(CustomScrollViewHandler scrollViewHandler, CustomScrollView customScrollView)
        {
            scrollViewHandler.PlatformView.ScrollEnabled = customScrollView.IsScrollEnabled;
        }

        private static void MapIsHorizontalScollbarVisible(CustomScrollViewHandler handler, CustomScrollView view)
        {
            handler.PlatformView.ShowsHorizontalScrollIndicator = view.IsHorizontalScrollbarVisible;
        }

        private static void MapIsVerticalScollbarVisible(CustomScrollViewHandler handler, CustomScrollView view)
        {
            handler.PlatformView.ShowsVerticalScrollIndicator = view.IsVerticalScrollbarVisible;
        }
    }
}