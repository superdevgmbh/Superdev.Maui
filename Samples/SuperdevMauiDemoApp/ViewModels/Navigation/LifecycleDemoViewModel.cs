using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    /// <summary>
    /// Demonstrates the navigation lifecycle callbacks and the navigation events of the navigation service.
    /// </summary>
    public class LifecycleDemoViewModel : BaseViewModel,
        INavigatedTo, INavigatedFrom, IApplicationResumed, IApplicationStopped, IDisposable
    {
        private readonly INavigationService navigationService;
        private IAsyncRelayCommand? pushChildCommand;
        private IRelayCommand? clearLogCommand;

        public LifecycleDemoViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.Title = "Lifecycle";

            this.navigationService.Navigating += this.OnNavigating;
            this.navigationService.Navigated += this.OnNavigated;
        }

        public ObservableCollection<string> Log { get; } = new ObservableCollection<string>();

        public IAsyncRelayCommand PushChildCommand
        {
            get => this.pushChildCommand ??= new AsyncRelayCommand(
                () => this.navigationService.PushAsync("LifecycleChildPage"));
        }

        public IRelayCommand ClearLogCommand
        {
            get => this.clearLogCommand ??= new RelayCommand(this.Log.Clear);
        }

        protected override Task InitializeAsync()
        {
            this.Add("InitializeAsync (runs once, before the page is visible)");
            return Task.CompletedTask;
        }

        public Task NavigatedToAsync(NavigationDirection navigationDirection)
        {
            this.Add($"NavigatedToAsync({navigationDirection})");
            return Task.CompletedTask;
        }

        public Task NavigatedFromAsync(NavigationDirection navigationDirection)
        {
            this.Add($"NavigatedFromAsync({navigationDirection})");
            return Task.CompletedTask;
        }

        public Task OnResumedAsync()
        {
            this.Add("OnResumedAsync (application resumed)");
            return Task.CompletedTask;
        }

        public Task OnStoppedAsync()
        {
            this.Add("OnStoppedAsync (application sent to background)");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // A viewmodel which subscribes to the navigation events has to unsubscribe when it is disposed.
            this.navigationService.Navigating -= this.OnNavigating;
            this.navigationService.Navigated -= this.OnNavigated;
        }

        private void OnNavigating(object? sender, NavigatingEventArgs e)
        {
            this.Add($"Navigating: {e.Request}");
        }

        private void OnNavigated(object? sender, NavigatedEventArgs e)
        {
            this.Add($"Navigated: {e.Request} ({e.Duration.TotalMilliseconds:F0} ms)");
        }

        private void Add(string entry)
        {
            this.Log.Insert(0, $"{DateTime.Now:HH:mm:ss.fff}  {entry}");
        }
    }
}
