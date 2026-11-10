namespace TodoApp.Services
{
    /// <summary>
    /// Stores the todo items of this app.
    /// </summary>
    public interface ITodoService
    {
        Task<IReadOnlyCollection<TodoItem>> GetTodoItemsAsync();

        Task<TodoItem?> GetTodoItemAsync(Guid id);

        Task AddOrUpdateTodoItemAsync(TodoItem todoItem);

        Task RemoveTodoItemAsync(Guid id);
    }
}
