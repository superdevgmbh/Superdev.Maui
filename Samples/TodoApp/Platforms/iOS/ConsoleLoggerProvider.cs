namespace TodoApp
{
    /// <summary>
    /// Writes the application's log to the console, which the simulator's system log shows:
    /// <c>xcrun simctl spawn booted log stream --predicate 'process == "TodoApp"'</c>. The debug logger
    /// only reaches an attached debugger.
    /// </summary>
    internal sealed class ConsoleLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new ConsoleLogger(categoryName[(categoryName.LastIndexOf('.') + 1)..]);
        }

        public void Dispose()
        {
        }

        private sealed class ConsoleLogger : ILogger
        {
            private readonly string category;

            public ConsoleLogger(string category)
            {
                this.category = category;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return logLevel != LogLevel.None;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Console.WriteLine($"TodoApp [{this.category}] {logLevel}: {formatter(state, exception)}{(exception == null ? string.Empty : Environment.NewLine + exception)}");
            }
        }
    }
}
