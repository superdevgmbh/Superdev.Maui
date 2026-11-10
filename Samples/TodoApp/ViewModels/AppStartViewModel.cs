namespace TodoApp.ViewModels
{
    /// <summary>
    /// The first page of the app. It performs the startup work and then replaces itself with the
    /// <see cref="Views.TabbedMainPage"/>.
    /// </summary>
    public class AppStartViewModel : BaseViewModel, INavigatedTo
    {
        private readonly ILogger logger;
        private readonly INavigationService navigationService;

        public AppStartViewModel(
            ILogger<AppStartViewModel> logger,
            INavigationService navigationService)
        {
            this.logger = logger;
            this.navigationService = navigationService;

            this.IsBusy = true;
        }

        public async Task NavigatedToAsync(NavigationDirection navigationDirection)
        {
            // The start navigation must not be triggered from InitializeAsync, because that callback runs
            // while the navigation to this page is still in progress. NavigatedToAsync runs after it
            // completed, which is why a navigation is allowed here.
            try
            {
                // Startup work of the app would go here, e.g. restoring a session or loading a configuration.
                await Task.Delay(1000);

                // The leading slash replaces the root page of the window. This page is removed and its
                // service scope is disposed as part of this navigation.
                await this.navigationService.NavigateAsync($"/{App.Pages.TabbedMainPage}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Navigation to the main page failed with exception");
                this.IsBusy = false;
            }
        }
    }
}
