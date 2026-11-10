namespace TodoApp.Services
{
    /// <summary>
    /// Persists the todo items in the preferences of the device.
    /// <see cref="IPreferences"/> and <see cref="IDateTime"/> are registered by <c>UseSuperdevMaui</c>,
    /// so this service needs no additional setup and stays testable.
    /// </summary>
    public class TodoService : ITodoService
    {
        private const string PreferencesKey = "TodoApp_TodoItems";

        private readonly IPreferences preferences;
        private readonly IDateTime dateTime;

        public TodoService(IPreferences preferences, IDateTime dateTime)
        {
            this.preferences = preferences;
            this.dateTime = dateTime;
        }

        public Task<IReadOnlyCollection<TodoItem>> GetTodoItemsAsync()
        {
            var todoItems = this.GetTodoItems()
                .OrderBy(t => t.IsCompleted)
                .ThenByDescending(t => t.CreatedDate)
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<TodoItem>>(todoItems);
        }

        public Task<TodoItem?> GetTodoItemAsync(Guid id)
        {
            var todoItem = this.GetTodoItems().SingleOrDefault(t => t.Id == id);
            return Task.FromResult(todoItem);
        }

        public Task AddOrUpdateTodoItemAsync(TodoItem todoItem)
        {
            ArgumentNullException.ThrowIfNull(todoItem);

            var todoItems = this.GetTodoItems();

            var existingTodoItem = todoItems.SingleOrDefault(t => t.Id == todoItem.Id);
            if (existingTodoItem != null)
            {
                todoItems.Remove(existingTodoItem);
                todoItem.CreatedDate = existingTodoItem.CreatedDate;
            }
            else
            {
                todoItem.CreatedDate = this.dateTime.Now;
            }

            todoItems.Add(todoItem);

            this.SetTodoItems(todoItems);
            return Task.CompletedTask;
        }

        public Task RemoveTodoItemAsync(Guid id)
        {
            var todoItems = this.GetTodoItems();

            var existingTodoItem = todoItems.SingleOrDefault(t => t.Id == id);
            if (existingTodoItem != null)
            {
                todoItems.Remove(existingTodoItem);
                this.SetTodoItems(todoItems);
            }

            return Task.CompletedTask;
        }

        private List<TodoItem> GetTodoItems()
        {
            return this.preferences.Get(PreferencesKey, new List<TodoItem>()) ?? new List<TodoItem>();
        }

        private void SetTodoItems(List<TodoItem> todoItems)
        {
            this.preferences.Set(PreferencesKey, todoItems);
        }
    }
}
