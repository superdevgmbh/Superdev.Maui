using SuperdevMauiDemoApp.ViewModels;

namespace SuperdevMauiDemoApp.Views;

public partial class PreferencesDemoPage : ContentPage
{
    public PreferencesDemoPage(PreferencesDemoViewModel preferencesDemoViewModel)
    {
        this.InitializeComponent();
        this.BindingContext = preferencesDemoViewModel;
    }
}