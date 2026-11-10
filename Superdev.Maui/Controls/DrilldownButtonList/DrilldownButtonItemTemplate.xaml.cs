using System.Diagnostics;
using System.Windows.Input;
using Superdev.Maui.Resources.Styles;

namespace Superdev.Maui.Controls
{
    public partial class DrilldownButtonItemTemplate : ContentView
    {
        public DrilldownButtonItemTemplate()
        {
            this.InitializeComponent();

            // Hack: OnPlatform lacks of support for DynamicResource bindings!
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                this.ActivityIndicator.SetDynamicResource(ActivityIndicator.ColorProperty, ThemeConstants.Color.Secondary);
            }
        }

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(
                nameof(Text),
                typeof(string),
                typeof(DrilldownButtonItemTemplate));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(
                nameof(Command),
                typeof(ICommand),
                typeof(DrilldownButtonItemTemplate));

        public ICommand Command
        {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(
                nameof(CommandParameter),
                typeof(object),
                typeof(DrilldownButtonItemTemplate));

        public object CommandParameter
        {
            get => this.GetValue(CommandParameterProperty);
            set => this.SetValue(CommandParameterProperty, value);
        }

        public new static readonly BindableProperty IsEnabledProperty =
            BindableProperty.Create(
                nameof(IsEnabled),
                typeof(bool),
                typeof(DrilldownButtonItemTemplate),
                true);

        public new bool IsEnabled
        {
            get => (bool)this.GetValue(IsEnabledProperty);
            set => this.SetValue(IsEnabledProperty, value);
        }

        public static readonly BindableProperty ImageSourceProperty =
            BindableProperty.Create(
                nameof(ImageSource),
                typeof(ImageSource),
                typeof(DrilldownButtonItemTemplate));

        public ImageSource ImageSource
        {
            get => (ImageSource)this.GetValue(ImageSourceProperty);
            set => this.SetValue(ImageSourceProperty, value);
        }

        public static readonly BindableProperty IsBusyProperty =
            BindableProperty.Create(
                nameof(IsBusy),
                typeof(bool),
                typeof(DrilldownButtonItemTemplate),
                false);

        public bool IsBusy
        {
            get => (bool)this.GetValue(IsBusyProperty);
            set => this.SetValue(IsBusyProperty, value);
        }
    }
}