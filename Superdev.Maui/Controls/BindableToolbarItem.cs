namespace Superdev.Maui.Controls
{
    /// <summary>
    /// ToolbarItem with <see cref="IsVisible"/> property to toggle visibility of toolbar items.
    /// </summary>
    public class BindableToolbarItem : ToolbarItem
    {
        public BindableToolbarItem()
        {
            OnIsVisibleChanged(this, false, this.IsVisible);
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

        private static void OnIsVisibleChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var item = bindable as BindableToolbarItem;

            if (item is { Parent: null })
            {
                return;
            }

            if (item != null)
            {
                var items = ((Page)item.Parent)?.ToolbarItems;

                if (Equals(items, null))
                {
                    return;
                }

                if ((bool)newvalue && !items.Contains(item))
                {
                    MainThread.BeginInvokeOnMainThread(() => { items.Add(item); });
                }
                else if (!(bool)newvalue && items.Contains(item))
                {
                    MainThread.BeginInvokeOnMainThread(() => { items.Remove(item); });
                }
            }
        }
    }
}