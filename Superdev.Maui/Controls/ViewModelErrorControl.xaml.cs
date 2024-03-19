using Superdev.Maui.Mvvm;

namespace Superdev.Maui.Controls
{
    public partial class ViewModelErrorControl : ContentView
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
            typeof(ViewModelErrorControl),
            ImageSource.FromResource("Superdev.Maui.Resources.Images.reload_192x192.png"));

        public ImageSource ImageSource
        {
            get => (ImageSource)this.GetValue(ImageSourceProperty);
            set => this.SetValue(ImageSourceProperty, value);
        }

        public static readonly BindableProperty LabelStyleProperty =
            BindableProperty.Create(
                nameof(LabelStyle),
                typeof(Style),
                typeof(ViewModelErrorControl),
                default(Style),
                BindingMode.OneWay);

        public Style LabelStyle
        {
            get => (Style)this.GetValue(LabelStyleProperty);
            set => this.SetValue(LabelStyleProperty, value);
        }
    }
}