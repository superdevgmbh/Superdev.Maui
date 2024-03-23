using Superdev.Maui.SampleApp.ViewModels;

namespace Superdev.Maui.SampleApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel mainViewModel)
        {
            this.InitializeComponent();
            this.BindingContext = mainViewModel;
        }
    }
}
