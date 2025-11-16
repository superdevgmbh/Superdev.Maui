using System.ComponentModel;

namespace Superdev.Maui.Services
{
    /// <summary>
    /// Static implementation of <see cref="IMainThread"/>.
    /// For testing purposes only!
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class StaticMainThread : Superdev.Maui.Services.IMainThread
    {
        public bool IsMainThread { get; } = true;

        public void BeginInvokeOnMainThread(Action action)
        {
            action();
        }

        public Task<SynchronizationContext> GetMainThreadSynchronizationContextAsync()
        {
            throw new NotSupportedException();
        }

        public Task InvokeOnMainThreadAsync(Action action)
        {
            action();
            return Task.CompletedTask;
        }

        public Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
        {
            return Task.FromResult(func());
        }

        public async Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask)
        {
            return await funcTask();
        }

        public async Task InvokeOnMainThreadAsync(Func<Task> funcTask)
        {
            await funcTask();
        }
    }
}