namespace Superdev.Maui.Controls
{
    /// <summary>
    /// A bordered box showing an error message; it hides itself while <see cref="Text"/> is empty.
    /// </summary>
    public partial class ErrorBox : Border
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