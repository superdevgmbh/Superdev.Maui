using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using Superdev.Maui.Extensions;
using Superdev.Maui.Navigation;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class NavigationDemoViewModel : BaseViewModel
    {
        private readonly INavigationService navigationService;
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private readonly IDialogService dialogService;

        private IAsyncRelayCommand navigateToPageCommand;
        private IAsyncRelayCommand popCommand;
        private IAsyncRelayCommand popToRootCommand;
        private IAsyncRelayCommand navigateToPageModalCommand;
        private IAsyncRelayCommand popModalCommand;
        private IRelayCommand toggleHasNavigationBarCommand;
        private bool hasNavigationBar = true;
        private IRelayCommand toggleSwipeBackEnabledCommand;
        private bool swipeBackEnabled = true;

        public NavigationDemoViewModel(
            INavigationService navigationService,
            IViewModelErrorHandler viewModelErrorHandler,
            IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.dialogService = dialogService;

            _ = this.InitializeAsync();
        }
        private async Task InitializeAsync()
        {
            try
            {
                await this.LoadData();
            }
            finally
            {
                this.IsInitialized = true;
            }
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        public IAsyncRelayCommand NavigateToPageCommand
        {
            get => this.navigateToPageCommand ??= new AsyncRelayCommand<string>(this.NavigateToPageAsync);
        }

        private async Task NavigateToPageAsync(string pageName)
        {
            try
            {
                await this.navigationService.PushAsync(pageName);
            }
            catch (Exception ex)
            {
                var viewModelError = this.viewModelErrorHandler.FromException(ex);
                await this.dialogService.DisplayAlertAsync(viewModelError, "OK", "Cancel");
            }
        }

        public IAsyncRelayCommand PopCommand
        {
            get => this.popCommand ??= new AsyncRelayCommand(this.PopAsync);
        }

        private async Task PopAsync()
        {
            try
            {
                await this.navigationService.PopAsync();
            }
            catch (Exception ex)
            {
                var viewModelError = this.viewModelErrorHandler.FromException(ex);
                await this.dialogService.DisplayAlertAsync(viewModelError, "OK", "Cancel");
            }
        }

        public IAsyncRelayCommand PopToRootCommand
        {
            get => this.popToRootCommand ??= new AsyncRelayCommand(this.PopToRootAsync);
        }

        private async Task PopToRootAsync()
        {
            try
            {
                await this.navigationService.PopToRootAsync(animated: false);
            }
            catch (Exception ex)
            {
                var viewModelError = this.viewModelErrorHandler.FromException(ex);
                await this.dialogService.DisplayAlertAsync(viewModelError, "OK", "Cancel");
            }
        }

        public IAsyncRelayCommand NavigateToPageModalCommand
        {
            get => this.navigateToPageModalCommand ??= new AsyncRelayCommand<string>(this.NavigateToPageModalAsync);
        }

        private async Task NavigateToPageModalAsync(string pageName)
        {
            try
            {
                await this.navigationService.PushModalAsync(pageName);
            }
            catch (Exception ex)
            {
                var viewModelError = this.viewModelErrorHandler.FromException(ex);
                await this.dialogService.DisplayAlertAsync(viewModelError, "OK", "Cancel");
            }
        }

        public IAsyncRelayCommand PopModalCommand
        {
            get => this.popModalCommand ??= new AsyncRelayCommand(this.PopModalAsync);
        }

        private async Task PopModalAsync()
        {
            try
            {
                await this.navigationService.PopModalAsync();
            }
            catch (Exception ex)
            {
                var viewModelError = this.viewModelErrorHandler.FromException(ex);
                await this.dialogService.DisplayAlertAsync(viewModelError, "OK", "Cancel");
            }
        }
        public IRelayCommand ToggleHasNavigationBarCommand
        {
            get => this.toggleHasNavigationBarCommand ??= new RelayCommand(this.ToggleHasNavigationBar);
        }

        private void ToggleHasNavigationBar()
        {
            this.HasNavigationBar = !this.HasNavigationBar;
        }

        public bool HasNavigationBar
        {
            get => this.hasNavigationBar;
            private set => this.SetProperty(ref this.hasNavigationBar, value);
        }

        public IRelayCommand ToggleSwipeBackEnabledCommand
        {
            get => this.toggleSwipeBackEnabledCommand ??= new RelayCommand(this.ToggleSwipeBackEnabled);
        }

        private void ToggleSwipeBackEnabled()
        {
            this.SwipeBackEnabled = !this.SwipeBackEnabled;
        }

        public bool SwipeBackEnabled
        {
            get => this.swipeBackEnabled;
            private set => this.SetProperty(ref this.swipeBackEnabled, value);
        }
    }
}