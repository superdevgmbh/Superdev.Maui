using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;

namespace Superdev.Maui.Platforms.iOS.Handlers
{
    public class CustomScrollViewHandler : ScrollViewHandler
    {
        public static IPropertyMapper<CustomScrollView, CustomScrollViewHandler> PropertyMapper = new PropertyMapper<CustomScrollView, CustomScrollViewHandler>(ScrollViewHandler.Mapper)
        {
            [nameof(CustomScrollView.IsBounceEnabled)] = MapIsBounceEnabled,
            [nameof(CustomScrollView.IsScrollEnabled)] = MapIsScrollEnabled,
            [nameof(CustomScrollView.IsHorizontalScollbarVisible)] = MapIsHorizontalScollbarVisible,
            [nameof(CustomScrollView.IsVerticalScollbarVisible)] = MapIsVerticalScollbarVisible,
        };

        public CustomScrollViewHandler() : base(PropertyMapper)
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
            handler.PlatformView.ShowsHorizontalScrollIndicator = view.IsHorizontalScollbarVisible;
        }

        private static void MapIsVerticalScollbarVisible(CustomScrollViewHandler handler, CustomScrollView view)
        {
            handler.PlatformView.ShowsVerticalScrollIndicator = view.IsVerticalScollbarVisible;
        }
    }
}
