namespace Superdev.Maui.Controls
{
    public partial class ItemTemplateActivityIndicator : Grid
    {
        public ItemTemplateActivityIndicator()
        {
            this.InitializeComponent();
        }

        public static readonly BindableProperty IsBusyProperty =
            BindableProperty.Create(
                nameof(IsBusy),
                typeof(bool),
                typeof(ItemTemplateActivityIndicator),
                false);

        public bool IsBusy
        {
            get => (bool)this.GetValue(IsBusyProperty);
            set => this.SetValue(IsBusyProperty, value);
        }
    }
}