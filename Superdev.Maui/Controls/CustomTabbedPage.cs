namespace Superdev.Maui.Controls
{
    public class CustomTabbedPage : TabbedPage
    {
        public static readonly BindableProperty HideTabsProperty =
           BindableProperty.Create(
               nameof(HideTabs),
               typeof(bool),
               typeof(CustomTabbedPage),
               false);

        public bool HideTabs
        {
            get => (bool)this.GetValue(HideTabsProperty);
            set => this.SetValue(HideTabsProperty, value);
        }
    }
}
