namespace Superdev.Maui.Navigation.Internals
{
    /// <summary>
    /// Carries the result of a page which was pushed with one of the
    /// <c>PushForResultAsync</c> methods back to the awaiting caller.
    /// </summary>
    /// <remarks>
    /// The awaiting task is completed when the page's navigation context is disposed, no matter how the page
    /// was removed. This guarantees that a caller never awaits forever, and that it resumes only after the
    /// page has been torn down completely.
    /// </remarks>
    internal sealed class NavigationResultSource
    {
        private readonly TaskCompletionSource<object?> taskCompletionSource =
            new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Gets the task which completes when the page was removed and its scope was disposed.
        /// </summary>
        public Task<object?> Task => this.taskCompletionSource.Task;

        /// <summary>
        /// Gets or sets the result which is handed to the awaiting caller.
        /// </summary>
        public object? Result { get; set; }

        /// <summary>
        /// The result type the caller of <c>PushForResultAsync</c> awaits, so that a result of a different
        /// type is rejected while the navigation can still be cancelled.
        /// </summary>
        public Type? ExpectedResultType { get; set; }

        /// <summary>
        /// Completes the awaiting caller with <see cref="Result"/>.
        /// </summary>
        public void TryComplete()
        {
            this.taskCompletionSource.TrySetResult(this.Result);
        }

        /// <summary>
        /// Faults the awaiting caller with the given <paramref name="exception"/>.
        /// </summary>
        public void TryCompleteWithException(Exception exception)
        {
            this.taskCompletionSource.TrySetException(exception);
        }
    }
}
