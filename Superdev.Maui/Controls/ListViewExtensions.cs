namespace Superdev.Maui.Controls
{
    public class ListViewExtensions
    {
        /// <summary>
        /// ListView.ScrollTo extension which allows to scroll to a certain item in the ListView.
        /// You can either bind the target item directly to the ListViewExtension.ScrollTo property
        /// or use a <see cref="ScrollToItem"/> object to also define the ScrollPosition as well as the Animated flag.
        /// </summary>
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
            if (bindable is not ListView listView)
            {
                return;
            }

            object item;
            ScrollToPosition scrollToPosition;
            bool animated;

            if (newValue is ScrollToItem scrollToTarget)
            {
                item = scrollToTarget.Item;
                scrollToPosition = scrollToTarget.Position;
                animated = scrollToTarget.Animated;
            }
            else
            {
                item = newValue;
                scrollToPosition = ScrollToPosition.MakeVisible;
                animated = true;
            }

            if (item == null)
            {
                return;
            }

            listView.ScrollTo(item, scrollToPosition, animated);
        }
    }
}