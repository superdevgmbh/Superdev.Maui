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
            object group;
            ScrollToPosition scrollToPosition;
            bool animated;

            if (newValue is ScrollToItem scrollToTarget)
            {
                item = scrollToTarget.Item ?? TryGetItem(listView, scrollToTarget.Position);
                group = scrollToTarget.Group;
                scrollToPosition = scrollToTarget.Position;
                animated = scrollToTarget.Animated;
            }
            else
            {
                item = newValue;
                group = null;
                scrollToPosition = ScrollToPosition.MakeVisible;
                animated = true;
            }

            if (item == null)
            {
                return;
            }

            if (group == null)
            {
                listView.ScrollTo(item, scrollToPosition, animated);
            }
            else
            {
                listView.ScrollTo(item, group, scrollToPosition, animated);
            }
        }

        private static object TryGetItem(ListView listView, ScrollToPosition scrollToPosition)
        {
            try
            {
                var listOfObjects = listView.ItemsSource?.Cast<object>();
                if (listOfObjects == null)
                {
                    return null;
                }

                if (scrollToPosition == ScrollToPosition.Start)
                {
                    return listOfObjects.FirstOrDefault();
                }

                if (scrollToPosition == ScrollToPosition.Center)
                {
                    var centerIndex = listOfObjects.Count() / 2;
                    return listOfObjects.ElementAtOrDefault(centerIndex);
                }

                if (scrollToPosition == ScrollToPosition.End)
                {
                    return listOfObjects.LastOrDefault();
                }
            }
            catch
            {
                // Ignore
            }

            return null;
        }
    }
}