namespace Superdev.Maui.Controls
{
    public partial class DefaultActivityIndicatorPage : ContentPage, IActivityIndicatorPage
    {
        public DefaultActivityIndicatorPage()
        {
            this.InitializeComponent();
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
        }

        public void SetCaption(string text)
        {
            this.ActivityIndicator.Caption = text;
        }
    }
}