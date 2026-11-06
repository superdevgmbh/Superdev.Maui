namespace Superdev.Maui.Utils
{
    /// <summary>
    /// Runs a callback after a sliding delay: a call which arrives before the delay elapsed
    /// supersedes the pending one, so only the most recent call runs.
    /// </summary>
    public class TaskDelayer
    {
        private CancellationTokenSource? throttleCts;

        /// <summary>
        /// Runs the given <paramref name="action" /> on a background thread after a sliding <paramref name="delay" />.
        /// </summary>
        /// <returns>
        /// A task which completes when the action completed, or immediately when a later call superseded this one.
        /// Exceptions thrown by the action fault the returned task.
        /// </returns>
        public Task RunWithDelay(TimeSpan delay, Action action)
        {
            ArgumentNullException.ThrowIfNull(action);

            return this.RunWithDelay(delay, () =>
            {
                action();
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Runs the given <paramref name="task" /> on a background thread after a sliding <paramref name="delay" />.
        /// </summary>
        /// <returns>
        /// A task which completes when <paramref name="task" /> completed, or immediately when a later call
        /// superseded this one. Exceptions thrown by <paramref name="task" /> fault the returned task.
        /// </returns>
        public Task RunWithDelay(TimeSpan delay, Func<Task> task)
        {
            ArgumentNullException.ThrowIfNull(task);

            return this.RunWithDelayCoreAsync<object?>(
                delay,
                async () =>
                {
                    await task().ConfigureAwait(false);
                    return null;
                },
                static () => null);
        }

        /// <summary>
        /// Runs the given <paramref name="task" /> on a background thread after a sliding <paramref name="delay" />.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="delay">Sliding delay.</param>
        /// <param name="task">The task to be executed after the delay.</param>
        /// <param name="defaultValue">The value which is returned when a later call superseded this one.</param>
        public Task<T> RunWithDelay<T>(TimeSpan delay, Func<Task<T>> task, Func<T> defaultValue)
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(defaultValue);

            return this.RunWithDelayCoreAsync(delay, task, defaultValue);
        }

        private async Task<T> RunWithDelayCoreAsync<T>(TimeSpan delay, Func<Task<T>> task, Func<T> defaultValue)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            // The token is taken from the source this call owns rather than re-read from the field,
            // so that two concurrent calls cannot both observe the newest source and both run.
            var previous = Interlocked.Exchange(ref this.throttleCts, cancellationTokenSource);
            if (previous != null)
            {
                previous.Cancel();
                previous.Dispose();
            }

            try
            {
                try
                {
                    await Task.Delay(delay, cancellationTokenSource.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // A later call superseded this one.
                    return defaultValue();
                }

                return await task().ConfigureAwait(false);
            }
            finally
            {
                // Only the call which is still the current one owns the source; a superseded call had
                // its source disposed by whoever replaced it.
                if (Interlocked.CompareExchange(ref this.throttleCts, null, cancellationTokenSource) == cancellationTokenSource)
                {
                    cancellationTokenSource.Dispose();
                }
            }
        }
    }
}
