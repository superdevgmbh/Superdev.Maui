namespace TodoApp.ViewModels.Todos
{
    /// <summary>
    /// A single row of the todo list. The row reports back to <see cref="MainViewModel"/> through the
    /// callbacks it is created with, so that saving and navigating stay in one place.
    /// </summary>
    public class TodoItemViewModel : BindableBase
    {
        private readonly TodoItem todoItem;
        private readonly Action<TodoItemViewModel> completedChanged;
        private readonly Func<TodoItemViewModel, Task> open;

        private ICommand? openCommand;
        private bool isCompleted;

        public TodoItemViewModel(
            TodoItem todoItem,
            Action<TodoItemViewModel> completedChanged,
            Func<TodoItemViewModel, Task> open)
        {
            this.todoItem = todoItem;
            this.completedChanged = completedChanged;
            this.open = open;
            this.isCompleted = todoItem.IsCompleted;
        }

        public Guid Id => this.todoItem.Id;

        public string Title => this.todoItem.Title;

        public string? Notes => this.todoItem.Notes;

        public bool HasNotes => !string.IsNullOrWhiteSpace(this.todoItem.Notes);

        public bool IsCompleted
        {
            get => this.isCompleted;
            set
            {
                if (this.SetProperty(ref this.isCompleted, value))
                {
                    this.todoItem.IsCompleted = value;
                    this.completedChanged(this);
                }
            }
        }

        public ICommand OpenCommand
        {
            get => this.openCommand ??= new AsyncRelayCommand(() => this.open(this));
        }

        public TodoItem TodoItem => this.todoItem;
    }
}
