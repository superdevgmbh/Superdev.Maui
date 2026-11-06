using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    /// <summary>
    /// Verifies that a page and its viewmodel are released after they left the navigation stack.
    /// The pushed page holds a large buffer, so a leak is visible in the memory usage as well.
    /// </summary>
    public class LeakDemoViewModel : BaseViewModel
    {
        private static WeakReference? lastPage;
        private static WeakReference? lastViewModel;

        private readonly INavigationService navigationService;
        private IAsyncRelayCommand? pushHeavyPageCommand;
        private IRelayCommand? collectCommand;
        private string status = "Push the heavy page, navigate back and collect.";

        public LeakDemoViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.Title = "Leak playground";
        }

        public string Status
        {
            get => this.status;
            private set => this.SetProperty(ref this.status, value);
        }

        public IAsyncRelayCommand PushHeavyPageCommand
        {
            get => this.pushHeavyPageCommand ??= new AsyncRelayCommand(this.PushHeavyPageAsync);
        }

        public IRelayCommand CollectCommand
        {
            get => this.collectCommand ??= new RelayCommand(this.Collect);
        }

        private async Task PushHeavyPageAsync()
        {
            await this.navigationService.PushAsync("HeavyPage");

            if (this.navigationService.CurrentPage is { } page)
            {
                lastPage = new WeakReference(page);
                lastViewModel = new WeakReference(page.BindingContext);
            }
        }

        private void Collect()
        {
            for (var i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            if (lastPage == null)
            {
                this.Status = "Push the heavy page first.";
                return;
            }

            this.Status =
                $"Page alive: {lastPage.IsAlive}{Environment.NewLine}" +
                $"Viewmodel alive: {lastViewModel?.IsAlive}{Environment.NewLine}" +
                $"Both must report false after navigating back.";
        }
    }
}
