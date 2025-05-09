namespace Superdev.Maui.Controls
{
    public partial class CustomActivityIndicator : ContentView
    {
        public CustomActivityIndicator()
        {
            this.InitializeComponent();

            base.BackgroundColor = Colors.Transparent;
        }

        public new static readonly BindableProperty BackgroundColorProperty =
            BindableProperty.Create(
                nameof(BackgroundColor),
                typeof(Color),
                typeof(CustomActivityIndicator));

        public new Color BackgroundColor
        {
            get => (Color)this.GetValue(BackgroundColorProperty);
            set => this.SetValue(BackgroundColorProperty, value);
        }

        public static readonly BindableProperty CornerRadiusProperty =
            BindableProperty.Create(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(CustomActivityIndicator),
                default(CornerRadius));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)this.GetValue(CornerRadiusProperty);
            set => this.SetValue(CornerRadiusProperty, value);
        }

        public static readonly BindableProperty IsBusyProperty =
            BindableProperty.Create(
                nameof(IsBusy),
                typeof(bool),
                typeof(CustomActivityIndicator),
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
                typeof(CustomActivityIndicator),
                null);

        public string Title
        {
            get => (string)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }

        public static readonly BindableProperty TitleStyleProperty =
            BindableProperty.Create(
                nameof(TitleStyle),
                typeof(Style),
                typeof(CustomActivityIndicator));

        public Style TitleStyle
        {
            get => (Style)this.GetValue(TitleStyleProperty);
            set => this.SetValue(TitleStyleProperty, value);
        }

        public static readonly BindableProperty ActivityIndicatorStyleProperty =
            BindableProperty.Create(
                nameof(ActivityIndicatorStyle),
                typeof(Style),
                typeof(CustomActivityIndicator));

        public Style ActivityIndicatorStyle
        {
            get => (Style)this.GetValue(ActivityIndicatorStyleProperty);
            set => this.SetValue(ActivityIndicatorStyleProperty, value);
        }

    }
}