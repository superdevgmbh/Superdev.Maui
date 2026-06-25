using System.Diagnostics;
using Superdev.Maui.Utils.Threading;

namespace Superdev.Maui.Controls
{
    public static class CollectionViewExtensions
    {
        private static readonly AsyncLock ScrollToLock = new AsyncLock();

        /// <summary>
        /// The minimum duration between ScrollTo calls.
        /// This delay is helpful to avoid exceptions if users issue ScrollTo updates with high frequency.
        /// </summary>
        private static readonly TimeSpan ScrollToMinimumLockDuration = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// CollectionView.ScrollTo extension which allows to scroll to a certain item in the <see cref="ItemsView"/>
        /// (e.g. <see cref="CollectionView"/> or <see cref="CarouselView"/>).
        /// You can either bind the target item directly to the CollectionViewExtensions.ScrollTo property
        /// or use a <see cref="ScrollToItem"/> object to also define the ScrollPosition as well as the Animated flag.
        /// </summary>
        public static readonly BindableProperty ScrollToProperty =
            BindableProperty.CreateAttached(
                "ScrollTo",
                typeof(object),
                typeof(CollectionViewExtensions),
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

        private static async void OnScrollToPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not ItemsView itemsView)
            {
                return;
            }

            object? item;
            object? group;
            ScrollToPosition scrollToPosition;
            bool animated;

            if (newValue is ScrollToItem scrollToTarget)
            {
                item = scrollToTarget.Item ?? TryGetItem(itemsView, scrollToTarget.Position);
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

            using (await ScrollToLock.LockAsync())
            {
                Debug.WriteLine($"OnScrollToPropertyChanged: scrollToPosition={scrollToPosition}");

                itemsView.ScrollTo(item, group, scrollToPosition, animated);

                await Task.Delay(ScrollToMinimumLockDuration);
            }
        }

        private static object? TryGetItem(ItemsView itemsView, ScrollToPosition scrollToPosition)
        {
            try
            {
                var listOfObjects = itemsView.ItemsSource?.Cast<object>().ToArray();
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
                    var centerIndex = listOfObjects.Length / 2;
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
