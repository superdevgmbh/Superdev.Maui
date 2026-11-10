using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;
using Superdev.Maui.Services;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    /// <summary>
    /// Demonstrates a navigation guard which asks the user before the page is left.
    /// The guard covers the back button, the hardware back button and a programmatic navigation alike.
    /// </summary>
    public class GuardDemoViewModel : BaseViewModel, IConfirmNavigation
    {
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;

        private IAsyncRelayCommand? popCommand;
        private bool hasUnsavedChanges = true;

        public GuardDemoViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            this.Title = "Navigation guard";
        }

        public bool HasUnsavedChanges
        {
            get => this.hasUnsavedChanges;
            set => this.SetProperty(ref this.hasUnsavedChanges, value);
        }

        public IAsyncRelayCommand PopCommand
        {
            get => this.popCommand ??= new AsyncRelayCommand(this.PopAsync);
        }

        public async Task<bool> CanNavigateAsync()
        {
            if (!this.HasUnsavedChanges)
            {
                return true;
            }

            return await this.dialogService.DisplayAlertAsync(
                "Unsaved changes",
                "This page has unsaved changes. Do you want to discard them?",
                "Discard",
                "Keep editing");
        }

        private async Task PopAsync()
        {
            try
            {
                // The page is either pushed onto a navigation stack or presented modally
                // (see "PushModalAsync (guarded page)"): it is closed the way it was opened.
                if (this.navigationService.NavigationStack.Count > 1)
                {
                    await this.navigationService.PopAsync();
                }
                else
                {
                    await this.navigationService.PopModalAsync();
                }
            }
            catch (NavigationCancelledException)
            {
                // The guard vetoed the navigation, which is an expected outcome.
            }
        }
    }
}
