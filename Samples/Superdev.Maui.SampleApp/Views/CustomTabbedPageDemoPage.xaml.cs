using Superdev.Maui.Controls;

namespace Superdev.Maui.SampleApp.Views
{
    public partial class CustomTabbedPageDemoPage : CustomTabbedPage
    {
        public CustomTabbedPageDemoPage()
        {
            this.InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            //Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);
            Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetIsSwipePagingEnabled(this, true);
        }
    }
}