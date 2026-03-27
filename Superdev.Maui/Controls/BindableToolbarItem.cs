using System.Collections.Concurrent;

namespace Superdev.Maui.Controls
{
    /// <summary>
    /// ToolbarItem with <see cref="IsVisible"/> property to toggle visibility of toolbar items.
    /// </summary>
    public class BindableToolbarItem : ToolbarItem
    {
        private readonly ConcurrentQueue<bool> visibilityUpdateQueue = new ConcurrentQueue<bool>();

        private bool bindingContextChanged;
        private bool isRemoving;

        public BindableToolbarItem()
        {
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            this.bindingContextChanged = this.BindingContext != null;

            if (this.bindingContextChanged)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(1); // Not good
                    OnIsVisibleChanged(this, null, this.IsVisible);
                });
            }
        }

        public static readonly BindableProperty IsVisibleProperty =
            BindableProperty.Create(
                nameof(IsVisible),
                typeof(bool),
                typeof(ToolbarItem),
                true,
                BindingMode.TwoWay,
                propertyChanged: OnIsVisibleChanged);

        public bool IsVisible
        {
            get => (bool)this.GetValue(IsVisibleProperty);
            set => this.SetValue(IsVisibleProperty, value);
        }

        private static void OnIsVisibleChanged(BindableObject bindable, object? oldValue, object? newValue)
        {
            var toolbarItem = bindable as BindableToolbarItem;

            if (toolbarItem?.Parent is not Page parentPage)
            {
                return;
            }

            if (toolbarItem.bindingContextChanged == false)
            {
                return;
            }

            if (toolbarItem.isRemoving)
            {
                toolbarItem.isRemoving = false;
                return;
            }

            var toolbarItems = parentPage.ToolbarItems;

            if (Equals(toolbarItems, null))
            {
                return;
            }

            if (newValue is bool isVisible)
            {
                if (isVisible && !toolbarItems.Contains(toolbarItem))
                {
                    toolbarItem.visibilityUpdateQueue.Enqueue(true);
                }
                else if (!isVisible && toolbarItems.Contains(toolbarItem))
                {
                    toolbarItem.visibilityUpdateQueue.Enqueue(false);
                }
            }

            if (!toolbarItem.visibilityUpdateQueue.IsEmpty)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    while (toolbarItem.visibilityUpdateQueue.TryDequeue(out var value))
                    {
                        if (value)
                        {
                            toolbarItems.Add(toolbarItem);
                        }
                        else
                        {
                            toolbarItem.isRemoving = true;
                            var parent = toolbarItem.Parent;
                            if (toolbarItems.Remove(toolbarItem))
                            {
                                toolbarItem.Parent = parent;
                            }
                        }
                    }
                });
            }
        }
    }
}