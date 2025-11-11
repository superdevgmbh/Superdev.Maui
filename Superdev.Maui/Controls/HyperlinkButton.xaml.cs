using System.Windows.Input;

namespace Superdev.Maui.Controls
{
    [XamlCompilation(XamlCompilationOptions.Skip)]
    public partial class HyperlinkButton : ContentView
    {
        public HyperlinkButton()
        {
            this.InitializeComponent();
        }

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(
                nameof(Text),
                typeof(string),
                typeof(HyperlinkButton));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public static readonly BindableProperty LabelStyleProperty =
            BindableProperty.Create(
                nameof(LabelStyle),
                typeof(Style),
                typeof(HyperlinkButton));

        public Style LabelStyle
        {
            get => (Style)this.GetValue(LabelStyleProperty);
            set => this.SetValue(LabelStyleProperty, value);
        }

        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(
                nameof(TextColor),
                typeof(Color),
                typeof(HyperlinkButton));

        public Color TextColor
        {
            get => (Color)this.GetValue(TextColorProperty);
            set => this.SetValue(TextColorProperty, value);
        }

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(
                nameof(Command),
                typeof(ICommand),
                typeof(HyperlinkButton));

        public ICommand Command
        {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        public static readonly BindableProperty ActivityIndicatorStyleProperty =
            BindableProperty.Create(
                nameof(ActivityIndicatorStyle),
                typeof(Style),
                typeof(HyperlinkButton));

        public Style ActivityIndicatorStyle
        {
            get => (Style)this.GetValue(ActivityIndicatorStyleProperty);
            set => this.SetValue(ActivityIndicatorStyleProperty, value);
        }
    }
}