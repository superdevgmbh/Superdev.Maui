namespace Superdev.Maui.Validation
{
    /// <summary>
    /// Keeps the validation runs of one field in order. Each <see cref="Begin"/> supersedes the run before it: the
    /// earlier run's token is cancelled, so a debounce delay still pending is abandoned, an asynchronous rule in
    /// flight is cancelled, and a result which arrives late is recognised as stale and discarded — it is never
    /// reported as "clean".
    /// </summary>
    internal sealed class RunSequencer
    {
        private readonly Lock syncRoot = new Lock();
        private CancellationTokenSource? current;

        /// <summary>
        /// Starts a run, superseding any run in progress, and returns the token of the new run. The token is
        /// cancelled when a later run begins, when <see cref="Cancel"/> is called, or when <paramref name="linkedTo"/> is cancelled.
        /// </summary>
        public CancellationToken Begin(CancellationToken linkedTo = default)
        {
            var next = linkedTo.CanBeCanceled
                ? CancellationTokenSource.CreateLinkedTokenSource(linkedTo)
                : new CancellationTokenSource();

            CancellationTokenSource? previous;
            lock (this.syncRoot)
            {
                previous = this.current;
                this.current = next;
            }

            previous?.Cancel();
            previous?.Dispose();

            return next.Token;
        }

        /// <summary>Whether <paramref name="runToken"/> still belongs to the latest run.</summary>
        public bool IsCurrent(CancellationToken runToken)
        {
            return !runToken.IsCancellationRequested;
        }

        /// <summary>Abandons the run in progress, if any.</summary>
        public void Cancel()
        {
            CancellationTokenSource? previous;
            lock (this.syncRoot)
            {
                previous = this.current;
                this.current = null;
            }

            previous?.Cancel();
            previous?.Dispose();
        }
    }
}
