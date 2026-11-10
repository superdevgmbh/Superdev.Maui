namespace TodoApp.ViewModels.Todos
{
    /// <summary>
    /// The detail page of a todo item. It receives the identifier of the todo item as a navigation
    /// parameter, and it asks what to do with unsaved changes before it is left.
    /// </summary>
    public class TodoDetailViewModel : BaseViewModel, IInitialize<TodoDetailViewModel.NavigationParameter>, IConfirmNavigation
    {
        private const string SaveOption = "Save";
        private const string DiscardOption = "Discard";
        private const string CancelOption = "Cancel";

        private readonly ILogger logger;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly ITodoService todoService;

        private IAsyncRelayCommand? saveCommand;
        private IAsyncRelayCommand? deleteCommand;
        private TodoItem? todoItem;
        private string? todoTitle;
        private string? notes;
        private bool isCompleted;
        private DateTime createdDate;
        private bool isClosing;

        public TodoDetailViewModel(
            ILogger<TodoDetailViewModel> logger,
            INavigationService navigationService,
            IDialogService dialogService,
            ITodoService todoService)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            this.todoService = todoService;

            this.Title = "Todo";
        }

        public async Task InitializeAsync(NavigationParameter parameter)
        {
            try
            {
                this.todoItem = await this.todoService.GetTodoItemAsync(parameter.TodoItemId);

                if (this.todoItem == null)
                {
                    this.logger.LogWarning("Todo item {TodoItemId} was not found", parameter.TodoItemId);
                    return;
                }

                this.TodoTitle = this.todoItem.Title;
                this.Notes = this.todoItem.Notes;
                this.IsCompleted = this.todoItem.IsCompleted;
                this.CreatedDate = this.todoItem.CreatedDate;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "InitializeAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Todo", "Failed to load the todo.", "OK");
            }
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

        public bool IsCompleted
        {
            get => this.isCompleted;
            set => this.SetProperty(ref this.isCompleted, value);
        }

        public DateTime CreatedDate
        {
            get => this.createdDate;
            private set => this.SetProperty(ref this.createdDate, value);
        }

        public bool CanSave => this.todoItem != null && !string.IsNullOrWhiteSpace(this.TodoTitle);

        /// <summary>
        /// Compares the input with the stored todo the same way saving would store it, so that trimming a
        /// title or clearing the notes to blanks does not count as a change.
        /// </summary>
        private bool HasChanges =>
            this.todoItem != null &&
            (NormalizeTitle(this.TodoTitle) != this.todoItem.Title ||
             NormalizeNotes(this.Notes) != this.todoItem.Notes ||
             this.IsCompleted != this.todoItem.IsCompleted);

        public IAsyncRelayCommand SaveCommand
        {
            get => this.saveCommand ??= new AsyncRelayCommand(this.SaveAsync);
        }

        private async Task SaveAsync()
        {
            if (!await this.TrySaveAsync())
            {
                return;
            }

            // The navigation guard must not ask again when the page closes itself after a successful save.
            this.isClosing = true;

            try
            {
                await this.navigationService.PopAsync();
            }
            catch (Exception ex)
            {
                this.isClosing = false;
                this.logger.LogError(ex, "Navigating back after saving failed with exception");
            }
        }

        private async Task<bool> TrySaveAsync()
        {
            this.IsBusy = true;

            try
            {
                this.todoItem!.Title = NormalizeTitle(this.TodoTitle);
                this.todoItem.Notes = NormalizeNotes(this.Notes);
                this.todoItem.IsCompleted = this.IsCompleted;

                await this.todoService.AddOrUpdateTodoItemAsync(this.todoItem);
                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "SaveAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Todo", "Failed to save the todo.", "OK");
                return false;
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        public IAsyncRelayCommand DeleteCommand
        {
            get => this.deleteCommand ??= new AsyncRelayCommand(this.DeleteAsync);
        }

        private async Task DeleteAsync()
        {
            if (this.todoItem == null)
            {
                return;
            }

            var userConsent = await this.dialogService.DisplayDestructiveAlertAsync(
                "Delete todo", $"Do you want to delete \"{this.todoItem.Title}\"?", "Delete", "Cancel");

            if (!userConsent)
            {
                return;
            }

            try
            {
                await this.todoService.RemoveTodoItemAsync(this.todoItem.Id);

                // Whatever was typed belongs to a todo which no longer exists, so the guard has nothing to ask.
                this.isClosing = true;

                await this.navigationService.PopAsync();
            }
            catch (Exception ex)
            {
                this.isClosing = false;
                this.logger.LogError(ex, "DeleteAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Todo", "Failed to delete the todo.", "OK");
            }
        }

        /// <summary>
        /// Asks what to do with unsaved changes before this page is left. The navigation service asks this
        /// guard whichever way the page is about to go: the back button of the navigation bar, the hardware
        /// back button on Android, a pop from code, or a deep link which replaces the page tree. Choosing "Save" stores the todo and
        /// then lets the navigation proceed; a failed save keeps the page, so nothing is lost.
        /// </summary>
        public async Task<bool> CanNavigateAsync()
        {
            this.logger.LogDebug(
                "CanNavigateAsync: isClosing={IsClosing}, hasChanges={HasChanges}, title=\"{Title}\"",
                this.isClosing, this.HasChanges, this.TodoTitle);

            if (this.isClosing || !this.HasChanges)
            {
                return true;
            }

            // A todo without a title cannot be saved, so then the only choices are to discard or to stay.
            var options = this.CanSave ? new[] { SaveOption } : Array.Empty<string>();

            var choice = await this.dialogService.DisplayActionSheetAsync(
                "Unsaved changes", CancelOption, DiscardOption, options);

            return choice switch
            {
                SaveOption => await this.TrySaveAsync(),
                DiscardOption => true,
                _ => false
            };
        }

        private static string NormalizeTitle(string? title)
        {
            return (title ?? string.Empty).Trim();
        }

        private static string? NormalizeNotes(string? notes)
        {
            return string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        }

        public class NavigationParameter
        {
            public required Guid TodoItemId { get; init; }
        }
    }
}
