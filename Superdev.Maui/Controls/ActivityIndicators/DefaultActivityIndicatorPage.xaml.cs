namespace Superdev.Maui.Controls
{
    public partial class DefaultActivityIndicatorPage : ContentPage, IActivityIndicatorPage
    {
        public DefaultActivityIndicatorPage()
        {
            this.InitializeComponent();
        }

        public void SetTitle(string title)
        {
            this.ActivityIndicator.Title = title;
        }
    }
}