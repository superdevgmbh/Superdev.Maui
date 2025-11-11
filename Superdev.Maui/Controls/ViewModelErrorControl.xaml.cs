using Superdev.Maui.Mvvm;

namespace Superdev.Maui.Controls
{
    public partial class ViewModelErrorControl : Grid
    {
        public ViewModelErrorControl()
        {
            this.InitializeComponent();
        }

        public static readonly BindableProperty ViewModelErrorProperty = BindableProperty.Create(
                nameof(ViewModelError),
                typeof(ViewModelError),
                typeof(ViewModelErrorControl),
                ViewModelError.None);

        public ViewModelError ViewModelError
        {
            get => (ViewModelError)this.GetValue(ViewModelErrorProperty);
            set => this.SetValue(ViewModelErrorProperty, value);
        }

        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(ViewModelErrorControl));

        public ImageSource ImageSource
        {
            get => (ImageSource)this.GetValue(ImageSourceProperty);
            set => this.SetValue(ImageSourceProperty, value);
        }

        public static readonly BindableProperty TitleLabelStyleProperty =
            BindableProperty.Create(
                nameof(TitleLabelStyle),
                typeof(Style),
                typeof(ViewModelErrorControl));

        public Style TitleLabelStyle
        {
            get => (Style)this.GetValue(TitleLabelStyleProperty);
            set => this.SetValue(TitleLabelStyleProperty, value);
        }

        public static readonly BindableProperty TextLabelStyleProperty =
            BindableProperty.Create(
                nameof(TextLabelStyle),
                typeof(Style),
                typeof(ViewModelErrorControl));

        public Style TextLabelStyle
        {
            get => (Style)this.GetValue(TextLabelStyleProperty);
            set => this.SetValue(TextLabelStyleProperty, value);
        }
    }
}