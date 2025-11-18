using Superdev.Maui.Controls;

namespace SuperdevMauiDemoApp.Views
{
    public partial class CustomTabbedPageDemoPage : CustomTabbedPage
    {
        public CustomTabbedPageDemoPage()
        {
            this.InitializeComponent();
        }

        private void ToggleIsSwipePagingEnabled(object sender, EventArgs e)
        {
            var isSwipePagingEnabled = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.GetIsSwipePagingEnabled(this);
            Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetIsSwipePagingEnabled(this, !isSwipePagingEnabled);
        }

        private void ToggleHideTabs(object sender, EventArgs e)
        {
            this.HideTabs = !this.HideTabs;
        }
    }
}