namespace Superdev.Maui.Services
{
    public interface IMainThread
    {
        /// <inheritdoc cref="MainThread.IsMainThread"/>
        bool IsMainThread { get; }

        /// <inheritdoc cref="MainThread.BeginInvokeOnMainThread(Action)"/>
        void BeginInvokeOnMainThread(Action action);

        /// <inheritdoc cref="MainThread.InvokeOnMainThreadAsync(Action)"/>
        Task InvokeOnMainThreadAsync(Action action);

        /// <inheritdoc cref="MainThread.InvokeOnMainThreadAsync(Func{Task})"/>
        Task InvokeOnMainThreadAsync(Func<Task> func);

        /// <inheritdoc cref="MainThread.InvokeOnMainThreadAsync{T}(Func{Task{T}})"/>
        Task<T> InvokeOnMainThreadAsync<T>(Func<T> func);

        /// <inheritdoc cref="MainThread.InvokeOnMainThreadAsync{T}(Func{T})"/>
        Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> func);

        /// <inheritdoc cref="MainThread.GetMainThreadSynchronizationContextAsync()"/>
        Task<SynchronizationContext> GetMainThreadSynchronizationContextAsync();
    }
}
