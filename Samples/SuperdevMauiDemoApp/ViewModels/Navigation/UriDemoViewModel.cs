using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    /// <summary>
    /// Demonstrates multi-segment navigation, removal segments and deep links.
    /// </summary>
    public class UriDemoViewModel : BaseViewModel
    {
        private readonly INavigationService navigationService;
        private IAsyncRelayCommand<string>? navigateCommand;

        public UriDemoViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.Title = "Navigation URIs";
        }

        public IAsyncRelayCommand<string> NavigateCommand
        {
            get => this.navigateCommand ??= new AsyncRelayCommand<string>(
                uri => this.navigationService.NavigateAsync(uri!));
        }
    }
}
