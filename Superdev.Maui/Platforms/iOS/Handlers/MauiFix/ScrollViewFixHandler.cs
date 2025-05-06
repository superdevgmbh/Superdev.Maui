using Microsoft.Maui.Handlers;

namespace Superdev.Maui.Platforms.Handlers.MauiFix
{
    using PM = PropertyMapper<ScrollView, ScrollViewFixHandler>;

    /// <summary>
    /// Workaround for https://github.com/dotnet/maui/issues/15374
    /// ScrollView does not work if content is shrinking; only if content is growing.
    /// </summary>
    /// <remarks>
    /// Original source: https://github.com/dotnet/maui/issues/14257#issuecomment-1646408225
    /// </remarks>
    public class ScrollViewFixHandler : ScrollViewHandler
    {
        public new static readonly PM Mapper = new PM(ScrollViewHandler.Mapper)
        {
            [nameof(IScrollView.ContentSize)] = UpdateContentSize,
        };

        public ScrollViewFixHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public ScrollViewFixHandler()
            : base(Mapper)
        {
        }

        private static void UpdateContentSize(IScrollViewHandler scrollViewHandler, IScrollView scrollView)
        {
            var contentSize = scrollView.ContentSize;
            if (contentSize.IsZero)
            {
                return;
            }

            var uiScrollView = scrollViewHandler.PlatformView;
            var container = uiScrollView.Subviews.FirstOrDefault(x => x.Tag == 0x845fed);

            if (container != null && Math.Abs(container.Bounds.Height - contentSize.Height) > 0.001d)
            {
                container.Bounds = new CoreGraphics.CGRect(
                    container.Bounds.X,
                    container.Bounds.Y,
                    contentSize.Width,
                    contentSize.Height);

                scrollView.InvalidateMeasure();
            }
        }
    }
}