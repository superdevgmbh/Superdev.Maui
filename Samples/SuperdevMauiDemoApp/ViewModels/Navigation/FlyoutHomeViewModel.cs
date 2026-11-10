using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;
using SuperdevMauiDemoApp.Services;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    public class FlyoutHomeViewModel : BaseViewModel, INavigatedTo, INavigatedFrom, IDisposable
    {
        private const string Source = "FlyoutHome";

        public FlyoutHomeViewModel(NavigationDebugLog navigationDebugLog)
        {
            this.NavigationDebugLog = navigationDebugLog;
            this.Title = "Home";
        }

        public NavigationDebugLog NavigationDebugLog { get; }

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
            // Written to the shared log, because this viewmodel is gone by the time the entry is read.
            this.NavigationDebugLog.Add(Source, "Dispose (the page and its scope were torn down)");
        }
    }
}
