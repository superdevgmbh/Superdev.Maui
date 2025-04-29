namespace Superdev.Maui.Controls
{
    public static class ScrollViewExtensions
    {
        public static readonly BindableProperty ScrollToProperty =
            BindableProperty.CreateAttached(
                "ScrollTo",
                typeof(ScrollToPosition),
                typeof(ScrollViewExtensions),
                ScrollToPosition.MakeVisible,
                propertyChanged: OnScrollToPropertyChanged);

        public static object GetScrollTo(BindableObject bindable)
        {
            return bindable.GetValue(ScrollToProperty);
        }

        public static void SetScrollTo(BindableObject view, object value)
        {
            view.SetValue(ScrollToProperty, value);
        }

        public static readonly BindableProperty ScrollAnimationProperty =
            BindableProperty.CreateAttached(
                "ScrollAnimation",
                typeof(ScrollAnimation),
                typeof(ScrollViewExtensions),
                ScrollAnimation.Default);

        public static object GetScrollAnimation(BindableObject bindable)
        {
            return bindable.GetValue(ScrollAnimationProperty);
        }

        public static void SetScrollAnimation(BindableObject view, object value)
        {
            view.SetValue(ScrollAnimationProperty, value);
        }

        private static void OnScrollToPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is ScrollView scrollView) || !(newValue is ScrollToPosition newScrollToPosition))
            {
                return;
            }

            var scrollAnimation = GetScrollAnimation(scrollView) as ScrollAnimation ?? ScrollAnimation.Default;

            var targetElement = scrollView.Content;
            if (targetElement == null)
            {
                return;
            }

            var point = scrollView.GetScrollPositionForElement(targetElement, newScrollToPosition);

            var animation = new Animation(
                y => scrollView.ScrollToAsync(x: 0d, y, animated: false),
                scrollView.ScrollY,
                point.Y);
            animation.Commit(
                scrollView,
                "Scroll",
                rate: scrollAnimation.Rate,
                length: scrollAnimation.Length,
                easing: scrollAnimation.Easing);
        }
    }
}