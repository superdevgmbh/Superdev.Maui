namespace Superdev.Maui.Controls
{
    public partial class SwipeItemBase : SwipeItemView
    {
        public SwipeItemBase()
        {
            this.InitializeComponent();
        }

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(
                nameof(Text),
                typeof(string),
                typeof(SwipeItemBase));

        public string Text
        {
            get
            {
                var text = (string)this.GetValue(TextProperty);
                return this.AllCaps ? text?.ToUpperInvariant() : text;
            }
            set => this.SetValue(TextProperty, value);
        }

        public static readonly BindableProperty IconImageSourceProperty =
            BindableProperty.Create(
                nameof(IconImageSource),
                typeof(ImageSource),
                typeof(SwipeItemBase));

        public ImageSource IconImageSource
        {
            get => (ImageSource)this.GetValue(IconImageSourceProperty);
            set => this.SetValue(IconImageSourceProperty, value);
        }

        public static readonly BindableProperty AllCapsProperty =
            BindableProperty.Create(
                nameof(AllCaps),
                typeof(bool),
                typeof(SwipeItemBase),
                false,
                propertyChanged: OnAllCapsPropertyChanged);

        private static void OnAllCapsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var swipeItemBase = (SwipeItemBase)bindable;
            swipeItemBase.OnPropertyChanged(nameof(Text));
        }

        public bool AllCaps
        {
            get => (bool)this.GetValue(AllCapsProperty);
            set => this.SetValue(AllCapsProperty, value);
        }
    }
}