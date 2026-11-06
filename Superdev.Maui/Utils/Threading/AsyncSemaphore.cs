namespace Superdev.Maui.Utils.Threading
{
    /// <summary>
    /// An asynchronous semaphore.
    /// </summary>
    /// <remarks>
    /// A thin wrapper around <see cref="SemaphoreSlim"/>, kept for compatibility; new code can use
    /// <see cref="SemaphoreSlim"/> directly. The continuation of a waiter never runs inside the
    /// <see cref="Release"/> call which released it.
    /// </remarks>
    public class AsyncSemaphore
    {
        private readonly SemaphoreSlim semaphore;

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCount));
            }

            this.semaphore = new SemaphoreSlim(initialCount);
        }

        public Task WaitAsync()
        {
            return this.semaphore.WaitAsync();
        }

        /// <summary>
        /// Waits to enter the semaphore, or gives up when <paramref name="cancellationToken"/> is cancelled.
        /// </summary>
        /// <exception cref="OperationCanceledException">If the wait was cancelled.</exception>
        public Task WaitAsync(CancellationToken cancellationToken)
        {
            return this.semaphore.WaitAsync(cancellationToken);
        }

        public void Release()
        {
            this.semaphore.Release();
        }
    }
}
