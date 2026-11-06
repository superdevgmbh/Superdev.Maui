namespace TodoApp.Models
{
    /// <summary>
    /// A single entry of the todo list.
    /// </summary>
    public class TodoItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
