namespace Superdev.Maui.Utils.Threading
{
    /// <summary>
    /// An asynchronous mutual exclusion lock: <c>using (await asyncLock.LockAsync()) { ... }</c>.
    /// </summary>
    /// <remarks>
    /// Built on <see cref="SemaphoreSlim"/>, so the continuation of a waiter never runs inside the
    /// <see cref="Releaser.Dispose"/> which released it, and a <see cref="Releaser"/> releases the lock
    /// once, no matter how many copies of it are disposed.
    /// </remarks>
    public class AsyncLock
    {
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public Task<Releaser> LockAsync()
        {
            return this.LockAsync(CancellationToken.None);
        }

        /// <summary>
        /// Acquires the lock, or gives up when <paramref name="cancellationToken"/> is cancelled.
        /// </summary>
        /// <exception cref="OperationCanceledException">If the wait was cancelled. The lock is not held.</exception>
        public async Task<Releaser> LockAsync(CancellationToken cancellationToken)
        {
            await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new Releaser(this.semaphore);
        }

        /// <summary>
        /// Releases the lock when disposed. The default value releases nothing.
        /// </summary>
        public readonly struct Releaser : IDisposable
        {
            private readonly ReleaseOnce? release;

            internal Releaser(SemaphoreSlim semaphore)
            {
                this.release = new ReleaseOnce(semaphore);
            }

            public void Dispose()
            {
                this.release?.Release();
            }
        }

        /// <summary>
        /// Shared by every copy of a <see cref="Releaser"/>, so that the lock is released exactly once.
        /// </summary>
        private sealed class ReleaseOnce
        {
            private SemaphoreSlim? semaphore;

            public ReleaseOnce(SemaphoreSlim semaphore)
            {
                this.semaphore = semaphore;
            }

            public void Release()
            {
                Interlocked.Exchange(ref this.semaphore, null)?.Release();
            }
        }
    }
}
