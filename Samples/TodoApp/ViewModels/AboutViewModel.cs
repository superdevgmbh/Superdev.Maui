using TodoApp.ViewModels.Todos;

namespace TodoApp.ViewModels
{
    /// <summary>
    /// The second tab of the <see cref="Views.TabbedMainPage"/>. Besides the version information it shows
    /// how a tab is selected programmatically, and how a page is pushed onto the stack of that tab.
    /// </summary>
    public class AboutViewModel : BaseViewModel
    {
        private readonly ILogger logger;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly ITodoService todoService;
        private readonly IDeviceInfo deviceInfo;
        private readonly IAppInfo appInfo;

        private IAsyncRelayCommand? showTodosCommand;
        private IAsyncRelayCommand? openNewestTodoCommand;
        private string? packageName;
        private string? deviceModel;
        private string? appVersion;

        public AboutViewModel(
            ILogger<AboutViewModel> logger,
            INavigationService navigationService,
            IDialogService dialogService,
            ITodoService todoService,
            IDeviceInfo deviceInfo,
            IAppInfo appInfo)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            this.todoService = todoService;
            this.deviceInfo = deviceInfo;
            this.appInfo = appInfo;

            this.Title = "About";
        }

        protected override Task InitializeAsync()
        {
            this.PackageName = this.appInfo.PackageName;
            this.AppVersion = $"{this.appInfo.VersionString} ({this.appInfo.BuildString})";
            this.DeviceModel = this.deviceInfo.Model;

            return Task.CompletedTask;
        }

        public string? PackageName
        {
            get => this.packageName;
            private set => this.SetProperty(ref this.packageName, value);
        }

        public string? AppVersion
        {
            get => this.appVersion;
            private set => this.SetProperty(ref this.appVersion, value);
        }

        public string? DeviceModel
        {
            get => this.deviceModel;
            private set => this.SetProperty(ref this.deviceModel, value);
        }

        public IAsyncRelayCommand ShowTodosCommand
        {
            get => this.showTodosCommand ??= new AsyncRelayCommand(this.ShowTodosAsync);
        }

        /// <summary>
        /// Selects the todos tab by the navigation name of the page it hosts. The tab child receives its
        /// lifecycle callbacks exactly as if the user had tapped the tab bar.
        /// </summary>
        private Task ShowTodosAsync()
        {
            return this.navigationService.SelectTabAsync(App.Pages.MainPage);
        }

        public IAsyncRelayCommand OpenNewestTodoCommand
        {
            get => this.openNewestTodoCommand ??= new AsyncRelayCommand(this.OpenNewestTodoAsync);
        }

        /// <summary>
        /// Selects the todos tab, this time by the view model type of the page it hosts, and then pushes the
        /// detail page onto the navigation stack of that tab. Once a tab is selected, the ordinary navigation
        /// calls apply to its navigation stack.
        /// </summary>
        private async Task OpenNewestTodoAsync()
        {
            try
            {
                var todoItems = await this.todoService.GetTodoItemsAsync();
                var newestTodoItem = todoItems.MaxBy(t => t.CreatedDate);

                if (newestTodoItem == null)
                {
                    await this.dialogService.DisplayAlertAsync("Todos", "There is no todo yet. Add one on the Todos tab.", "OK");
                    return;
                }

                await this.navigationService.SelectTabAsync<MainViewModel>();

                var navigationParameter = new TodoDetailViewModel.NavigationParameter
                {
                    TodoItemId = newestTodoItem.Id
                };

                await this.navigationService.PushAsync(App.Pages.TodoDetailPage, navigationParameter);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "OpenNewestTodoAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Todos", "Failed to open the newest todo.", "OK");
            }
        }
    }
}
