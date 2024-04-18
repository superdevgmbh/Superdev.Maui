using Superdev.Maui.SampleApp.ViewModels;

namespace Superdev.Maui.SampleApp.Views;

public partial class PreferencesDemoPage : ContentPage
{
    public PreferencesDemoPage(PreferencesDemoViewModel preferencesDemoViewModel)
    {
        this.InitializeComponent();
        this.BindingContext = preferencesDemoViewModel;
    }
}