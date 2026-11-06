using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    /// <summary>
    /// Demonstrates the popup pages: a bottom sheet, a centered card and a sheet with a text input.
    /// </summary>
    public class PopupDemoViewModel : BaseViewModel
    {
        private readonly INavigationService navigationService;

        private IAsyncRelayCommand? showContextMenuCommand;
        private IAsyncRelayCommand? showTooltipCommand;
        private IAsyncRelayCommand? showFormCommand;
        private string? lastResult;

        public PopupDemoViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.Title = "Popups";
        }

        public string? LastResult
        {
            get => this.lastResult;
            private set => this.SetProperty(ref this.lastResult, value);
        }

        public IAsyncRelayCommand ShowContextMenuCommand
        {
            get => this.showContextMenuCommand ??= new AsyncRelayCommand(this.ShowContextMenuAsync);
        }

        public IAsyncRelayCommand ShowTooltipCommand
        {
            get => this.showTooltipCommand ??= new AsyncRelayCommand(
                () => this.navigationService.PushAsync("TooltipPopupPage"));
        }

        public IAsyncRelayCommand ShowFormCommand
        {
            get => this.showFormCommand ??= new AsyncRelayCommand(this.ShowFormAsync);
        }

        private async Task ShowContextMenuAsync()
        {
            var result = await this.navigationService.PushForResultAsync<string>("ContextMenuPopupPage");
            this.LastResult = result ?? "(dismissed)";
        }

        private async Task ShowFormAsync()
        {
            var result = await this.navigationService.PushForResultAsync<string>("FormPopupPage");
            this.LastResult = result ?? "(dismissed)";
        }
    }
}
