using Superdev.Maui.Effects;

namespace Superdev.Maui.Controls
{
    /// <summary>
    /// A swipe item showing an icon above a text, both tinted with <see cref="TintColor"/>.
    /// </summary>
    /// <remarks>
    /// Every row of a list builds its swipe items along with the row, so the item sets its children
    /// from property-changed callbacks instead of binding them to itself.
    /// </remarks>
    public abstract partial class SwipeItemBase : SwipeItemView
    {
        protected SwipeItemBase()
        {
            this.InitializeComponent();

            // Implicit styles are applied from the base class constructor, before the children above exist,
            // so the values set until now are applied here.
            this.UpdateText();
            this.UpdateIcon();
            this.UpdateTintColor();
        }

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(
                nameof(Text),
                typeof(string),
                typeof(SwipeItemBase),
                propertyChanged: OnTextPropertyChanged);

        public string? Text
        {
            get
            {
                var text = (string?)this.GetValue(TextProperty);
                return this.AllCaps ? text?.ToUpperInvariant() : text;
            }
            set => this.SetValue(TextProperty, value);
        }

        private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((SwipeItemBase)bindable).UpdateText();
        }

        public static readonly BindableProperty IconImageSourceProperty =
            BindableProperty.Create(
                nameof(IconImageSource),
                typeof(ImageSource),
                typeof(SwipeItemBase),
                propertyChanged: OnIconImageSourcePropertyChanged);

        public ImageSource IconImageSource
        {
            get => (ImageSource)this.GetValue(IconImageSourceProperty);
            set => this.SetValue(IconImageSourceProperty, value);
        }

        private static void OnIconImageSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((SwipeItemBase)bindable).UpdateIcon();
        }

        public static readonly BindableProperty TintColorProperty =
            BindableProperty.Create(
                nameof(TintColor),
                typeof(Color),
                typeof(SwipeItemBase),
                Colors.White,
                BindingMode.OneWay,
                propertyChanged: OnTintColorPropertyChanged);

        public Color TintColor
        {
            get => (Color)this.GetValue(TintColorProperty);
            set => this.SetValue(TintColorProperty, value);
        }

        private static void OnTintColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((SwipeItemBase)bindable).UpdateTintColor();
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
            swipeItemBase.UpdateText();
        }

        public bool AllCaps
        {
            get => (bool)this.GetValue(AllCapsProperty);
            set => this.SetValue(AllCapsProperty, value);
        }

        private void UpdateText()
        {
            if (this.TextLabel is not CustomLabel textLabel)
            {
                return;
            }

            var text = this.Text;
            textLabel.Text = text;
            textLabel.IsVisible = !string.IsNullOrEmpty(text);
        }

        private void UpdateIcon()
        {
            if (this.IconImage is Image iconImage)
            {
                iconImage.Source = this.IconImageSource;
            }
        }

        private void UpdateTintColor()
        {
            if (this.IconImage is not Image iconImage || this.TextLabel is not CustomLabel textLabel)
            {
                return;
            }

            var tintColor = this.TintColor;
            TintImageEffect.SetTintColor(iconImage, tintColor);
            textLabel.TextColor = tintColor;
        }
    }
}
