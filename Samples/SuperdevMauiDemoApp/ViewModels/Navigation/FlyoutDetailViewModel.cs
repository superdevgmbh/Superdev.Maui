using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;
using SuperdevMauiDemoApp.Services;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    public class FlyoutDetailViewModel : BaseViewModel, INavigatedTo, INavigatedFrom, IDisposable
    {
        private const string Source = "FlyoutDetail";

        private readonly INavigationService navigationService;
        private IAsyncRelayCommand? pushCommand;
        private IAsyncRelayCommand? openPopupCommand;

        public FlyoutDetailViewModel(INavigationService navigationService, NavigationDebugLog navigationDebugLog)
        {
            this.navigationService = navigationService;
            this.NavigationDebugLog = navigationDebugLog;
            this.Title = "Detail";
        }

        public NavigationDebugLog NavigationDebugLog { get; }

        public IAsyncRelayCommand PushCommand
        {
            get => this.pushCommand ??= new AsyncRelayCommand(
                () => this.navigationService.PushAsync("FlyoutHomePage"));
        }

        public IAsyncRelayCommand OpenPopupCommand
        {
            get => this.openPopupCommand ??= new AsyncRelayCommand(
                () => this.navigationService.PushAsync("ContextMenuPopupPage"));
        }

        protected override Task InitializeAsync()
        {
            this.NavigationDebugLog.Add(Source, "InitializeAsync");
            return Task.CompletedTask;
        }

        public Task NavigatedToAsync(NavigationDirection navigationDirection)
        {
            this.NavigationDebugLog.Add(Source, $"NavigatedToAsync({navigationDirection})");
            return Task.CompletedTask;
        }

        public Task NavigatedFromAsync(NavigationDirection navigationDirection)
        {
            this.NavigationDebugLog.Add(Source, $"NavigatedFromAsync({navigationDirection})");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.NavigationDebugLog.Add(Source, "Dispose (the page and its scope were torn down)");
        }
    }
}
