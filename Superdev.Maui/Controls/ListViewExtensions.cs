namespace Superdev.Maui.Controls
{
    public class ListViewExtensions
    {
        public static BindableProperty ScrollToProperty =
            BindableProperty.CreateAttached(
                "ScrollTo",
                typeof(object),
                typeof(ListViewExtensions),
                null,
                propertyChanged: OnScrollToPropertyChanged);

        public static object GetScrollTo(BindableObject bindable)
        {
            return bindable.GetValue(ScrollToProperty);
        }

        public static void SetScrollTo(BindableObject view, object value)
        {
            view.SetValue(ScrollToProperty, value);
        }

        private static void OnScrollToPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is ListView listView))
            {
                return;
            }

            listView.ScrollTo(newValue, ScrollToPosition.MakeVisible, true);
        }
    }
}