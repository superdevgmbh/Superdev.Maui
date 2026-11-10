using Android.Util;

namespace TodoApp
{
    /// <summary>
    /// Writes the application's log to logcat, where <c>adb logcat -s TodoApp</c> shows it. The debug
    /// logger only reaches an attached debugger, which leaves the navigation service silent on a device
    /// that is not being debugged.
    /// </summary>
    internal sealed class LogcatLoggerProvider : ILoggerProvider
    {
        private const string Tag = "TodoApp";

        public ILogger CreateLogger(string categoryName)
        {
            return new LogcatLogger(categoryName);
        }

        public void Dispose()
        {
        }

        private sealed class LogcatLogger : ILogger
        {
            private readonly string category;

            public LogcatLogger(string category)
            {
                // The namespace adds nothing in logcat; the type name is what identifies the source.
                this.category = category[(category.LastIndexOf('.') + 1)..];
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
                var message = $"[{this.category}] {formatter(state, exception)}";
                if (exception != null)
                {
                    message += Environment.NewLine + exception;
                }

                switch (logLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                        Android.Util.Log.Debug(Tag, message);
                        break;
                    case LogLevel.Information:
                        Android.Util.Log.Info(Tag, message);
                        break;
                    case LogLevel.Warning:
                        Android.Util.Log.Warn(Tag, message);
                        break;
                    default:
                        Android.Util.Log.Error(Tag, message);
                        break;
                }
            }
        }
    }
}
