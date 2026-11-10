using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    public class ContextMenuPopupViewModel : BaseViewModel
    {
        private readonly INavigationService navigationService;
        private IAsyncRelayCommand<string>? selectCommand;
        private IAsyncRelayCommand? resetRootCommand;

        public ContextMenuPopupViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public IReadOnlyList<string> MenuItems { get; } = new[] { "Share", "Duplicate", "Delete" };

        public IAsyncRelayCommand<string> SelectCommand
        {
            get => this.selectCommand ??= new AsyncRelayCommand<string>(
                menuItem => this.navigationService.PopAsync(menuItem!));
        }

        /// <summary>
        /// Replaces the root page while this popup is still open, which requires the popup and the modal
        /// stack to be drained before the root is swapped.
        /// </summary>
        public IAsyncRelayCommand ResetRootCommand
        {
            get => this.resetRootCommand ??= new AsyncRelayCommand(
                () => this.navigationService.NavigateAsync("/NavigationPage/MainPage"));
        }
    }
}
