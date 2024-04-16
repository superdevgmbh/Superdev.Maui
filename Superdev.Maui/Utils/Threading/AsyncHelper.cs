using System.ComponentModel;

namespace Superdev.Maui.Utils.Threading
{
    /// <summary>
    /// Executes asynchronous tasks synchronously.
    /// </summary>
    /// <remarks>
    /// DO NOT use this class unless absolutely necessary. Calling async code from sync code is an anti-pattern
    /// in most cases. This class is provided to assist in gradual conversion from sync to async code.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class AsyncHelper
    {
        private static readonly TaskFactory TaskFactory = new TaskFactory(
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return TaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            TaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        public static Task RunAsync(Action action)
        {
            action();
            return Task.CompletedTask;
        }
    }
}