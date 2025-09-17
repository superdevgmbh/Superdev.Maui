namespace Superdev.Maui.Controls
{
    public partial class ErrorBox : Frame
    {
        public ErrorBox()
        {
            this.InitializeComponent();
        }

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(
                nameof(Text),
                typeof(string),
                typeof(ErrorBox),
                default(string));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
    }
}