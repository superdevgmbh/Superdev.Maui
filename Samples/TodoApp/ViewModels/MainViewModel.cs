using TodoApp.ViewModels.Todos;

namespace TodoApp.ViewModels
{
    /// <summary>
    /// The todo list, which is the first tab of the <see cref="Views.TabbedMainPage"/>.
    /// </summary>
    public class MainViewModel : BaseViewModel, INavigatedTo
    {
        private readonly ILogger logger;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly ITodoService todoService;

        private IAsyncRelayCommand? addTodoCommand;
        private IAsyncRelayCommand? showAboutCommand;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            INavigationService navigationService,
            IDialogService dialogService,
            ITodoService todoService)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            this.todoService = todoService;

            this.Title = "Todos";
        }

        public ObservableCollection<TodoItemViewModel> TodoItems { get; } = new ObservableCollection<TodoItemViewModel>();

        public bool HasNoTodoItems => this.TodoItems.Count == 0;

        /// <summary>
        /// Called once, before this page becomes visible for the first time. <see cref="BaseViewModel"/>
        /// reports <see cref="BaseViewModel.IsBusy"/> until it returned.
        /// </summary>
        protected override Task InitializeAsync()
        {
            return this.LoadTodoItemsAsync();
        }

        /// <summary>
        /// Called every time this page becomes the active page. Coming back from the detail page or from
        /// the modal add page is a backward navigation, which is when the list is reloaded.
        /// </summary>
        public Task NavigatedToAsync(NavigationDirection navigationDirection)
        {
            return navigationDirection == NavigationDirection.Back
                ? this.LoadTodoItemsAsync()
                : Task.CompletedTask;
        }

        private async Task LoadTodoItemsAsync()
        {
            this.IsBusy = true;

            try
            {
                this.logger.LogDebug("LoadTodoItemsAsync");

                var todoItems = await this.todoService.GetTodoItemsAsync();

                this.TodoItems.Clear();

                foreach (var todoItem in todoItems)
                {
                    this.TodoItems.Add(
                        new TodoItemViewModel(todoItem, this.OnTodoItemCompletedChanged, this.OpenTodoAsync));
                }

                this.RaisePropertyChanged(nameof(this.HasNoTodoItems));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "LoadTodoItemsAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Todos", "Failed to load the todo list.", "OK");
            }

            this.IsBusy = false;
        }

        private async void OnTodoItemCompletedChanged(TodoItemViewModel todoItemViewModel)
        {
            try
            {
                await this.todoService.AddOrUpdateTodoItemAsync(todoItemViewModel.TodoItem);
            }
            catch (Exception ex)
            {
                // A property setter has no caller which could observe this exception.
                this.logger.LogError(ex, "OnTodoItemCompletedChanged failed with exception");
            }
        }

        private Task OpenTodoAsync(TodoItemViewModel todoItemViewModel)
        {
            // Navigation parameters are plain objects. The target view model receives this one by
            // implementing IInitialize<TodoDetailViewModel.NavigationParameter>.
            var navigationParameter = new TodoDetailViewModel.NavigationParameter
            {
                TodoItemId = todoItemViewModel.Id
            };

            return this.navigationService.PushAsync(App.Pages.TodoDetailPage, navigationParameter);
        }

        public IAsyncRelayCommand AddTodoCommand
        {
            get => this.addTodoCommand ??= new AsyncRelayCommand(this.AddTodoAsync);
        }

        private Task AddTodoAsync()
        {
            // The modal page is wrapped in a navigation page, so that it has a navigation bar which can
            // host the close button.
            return this.navigationService.PushModalAsync(App.Pages.AddTodoPage, wrapInNavigationPage: true);
        }

        public IAsyncRelayCommand ShowAboutCommand
        {
            get => this.showAboutCommand ??= new AsyncRelayCommand(this.ShowAboutAsync);
        }

        /// <summary>
        /// Selects the about tab by the view model type of the page it hosts, as an alternative to the
        /// navigation name. The tab is initialized before it is shown, if it was never shown before.
        /// </summary>
        private Task ShowAboutAsync()
        {
            return this.navigationService.SelectTabAsync<AboutViewModel>();
        }
    }
}
