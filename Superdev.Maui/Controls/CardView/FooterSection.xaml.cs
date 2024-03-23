
namespace Superdev.Maui.Controls
{
    public partial class FooterSection : ContentView
    {
        public FooterSection()
        {
            this.InitializeComponent();
        }

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(
                nameof(Text),
                typeof(string),
                typeof(FooterSection),
                null,
                BindingMode.OneWay);

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
    }
}