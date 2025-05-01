namespace Superdev.Maui.Controls
{
    public partial class PageActivityIndicator : Grid
    {
        public PageActivityIndicator()
        {
            this.InitializeComponent();
        }

        public static readonly BindableProperty IsBusyProperty =
            BindableProperty.Create(
                nameof(IsBusy),
                typeof(bool),
                typeof(PageActivityIndicator),
                false);

        public bool IsBusy
        {
            get => (bool)this.GetValue(IsBusyProperty);
            set => this.SetValue(IsBusyProperty, value);
        }

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(
                nameof(Title),
                typeof(string),
                typeof(PageActivityIndicator));

        public string Title
        {
            get => (string)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(
                nameof(Text),
                typeof(string),
                typeof(PageActivityIndicator));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public static readonly BindableProperty TitleStyleProperty =
            BindableProperty.Create(
                nameof(TitleStyle),
                typeof(Style),
                typeof(PageActivityIndicator));

        public Style TitleStyle
        {
            get => (Style)this.GetValue(TitleStyleProperty);
            set => this.SetValue(TitleStyleProperty, value);
        }

        public static readonly BindableProperty TextStyleProperty =
            BindableProperty.Create(
                nameof(TextStyle),
                typeof(Style),
                typeof(PageActivityIndicator));

        public Style TextStyle
        {
            get => (Style)this.GetValue(TextStyleProperty);
            set => this.SetValue(TextStyleProperty, value);
        }

        public static readonly BindableProperty ColorProperty =
            BindableProperty.Create(
                nameof(Color),
                typeof(Color),
                typeof(PageActivityIndicator),
                KnownColor.Default);

        public Color Color
        {
            get => (Color)this.GetValue(ColorProperty);
            set => this.SetValue(ColorProperty, value);
        }
    }
}