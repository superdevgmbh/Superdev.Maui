using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    /// <summary>
    /// Demonstrates selecting a statically declared tab programmatically.
    /// </summary>
    public class TabsDemoViewModel : BaseViewModel
    {
        private readonly INavigationService navigationService;
        private IAsyncRelayCommand<string>? selectTabCommand;

        public TabsDemoViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.Title = "Tabs";
        }

        public IAsyncRelayCommand<string> SelectTabCommand
        {
            get => this.selectTabCommand ??= new AsyncRelayCommand<string>(
                tabName => this.navigationService.SelectTabAsync(tabName!));
        }
    }
}
