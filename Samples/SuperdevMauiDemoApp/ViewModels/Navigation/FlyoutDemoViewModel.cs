using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    /// <summary>
    /// Menu of the flyout shell. Every entry replaces the detail page through a relative navigation URI,
    /// which is the navigation shape a flyout-based application is built on.
    /// </summary>
    public class FlyoutDemoViewModel : BaseViewModel
    {
        private readonly INavigationService navigationService;
        private IAsyncRelayCommand<string>? navigateCommand;
        private IAsyncRelayCommand? leaveCommand;

        public FlyoutDemoViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.Title = "Flyout";
        }

        public IAsyncRelayCommand<string> NavigateCommand
        {
            get => this.navigateCommand ??= new AsyncRelayCommand<string>(
                uri => this.navigationService.NavigateAsync(uri!));
        }

        public IAsyncRelayCommand LeaveCommand
        {
            get => this.leaveCommand ??= new AsyncRelayCommand(
                () => this.navigationService.NavigateAsync("/NavigationPage/MainPage"));
        }
    }
}
