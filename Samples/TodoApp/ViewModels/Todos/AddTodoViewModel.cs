namespace TodoApp.ViewModels.Todos
{
    /// <summary>
    /// The modal page which creates a new todo item.
    /// </summary>
    public class AddTodoViewModel : BaseViewModel, IConfirmNavigation
    {
        private readonly ILogger logger;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly ITodoService todoService;

        private IAsyncRelayCommand? saveCommand;
        private IAsyncRelayCommand? closeCommand;
        private string? todoTitle;
        private string? notes;
        private bool isClosing;

        public AddTodoViewModel(
            ILogger<AddTodoViewModel> logger,
            INavigationService navigationService,
            IDialogService dialogService,
            ITodoService todoService)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            this.todoService = todoService;

            this.Title = "New todo";
        }

        public string? TodoTitle
        {
            get => this.todoTitle;
            set
            {
                if (this.SetProperty(ref this.todoTitle, value))
                {
                    this.RaisePropertyChanged(nameof(this.CanSave));
                }
            }
        }

        public string? Notes
        {
            get => this.notes;
            set => this.SetProperty(ref this.notes, value);
        }

        public bool CanSave => !string.IsNullOrWhiteSpace(this.TodoTitle);

        private bool HasChanges => !string.IsNullOrWhiteSpace(this.TodoTitle) || !string.IsNullOrWhiteSpace(this.Notes);

        public IAsyncRelayCommand SaveCommand
        {
            get => this.saveCommand ??= new AsyncRelayCommand(this.SaveAsync);
        }

        private async Task SaveAsync()
        {
            this.IsBusy = true;

            try
            {
                var todoItem = new TodoItem
                {
                    Title = this.TodoTitle!.Trim(),
                    Notes = string.IsNullOrWhiteSpace(this.Notes) ? null : this.Notes.Trim()
                };

                await this.todoService.AddOrUpdateTodoItemAsync(todoItem);

                // The navigation guard must not ask for confirmation when the page closes itself.
                this.isClosing = true;

                await this.navigationService.PopModalAsync();
            }
            catch (Exception ex)
            {
                this.isClosing = false;
                this.logger.LogError(ex, "SaveAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("New todo", "Failed to save the todo.", "OK");
            }

            this.IsBusy = false;
        }

        public IAsyncRelayCommand CloseCommand
        {
            get => this.closeCommand ??= new AsyncRelayCommand(this.CloseAsync);
        }

        private async Task CloseAsync()
        {
            try
            {
                // CanNavigateAsync runs first and asks the user whether the input should be discarded.
                await this.navigationService.PopModalAsync();
            }
            catch (NavigationCancelledException)
            {
                // The user decided to keep editing.
            }
        }

        /// <summary>
        /// Vetoes every navigation which would remove this page while there is unsaved input. This
        /// covers the close button as well as the gestures of the platform, e.g. the swipe-down
        /// dismissal of a modal page on iOS or the hardware back button on Android.
        /// </summary>
        public Task<bool> CanNavigateAsync()
        {
            if (this.isClosing || !this.HasChanges)
            {
                return Task.FromResult(true);
            }

            return this.dialogService.DisplayAlertAsync(
                "Discard todo?", "The todo was not saved yet.", "Discard", "Keep editing");
        }
    }
}
