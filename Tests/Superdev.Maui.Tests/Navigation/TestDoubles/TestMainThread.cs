using Superdev.Maui.Services;

namespace Superdev.Maui.Tests.Navigation.TestDoubles
{
    /// <summary>
    /// Reports that the caller is not on the main thread, so that the marshalling of the navigation
    /// pipeline is exercised. The work itself still runs inline, which keeps the tests deterministic.
    /// </summary>
    internal sealed class TestMainThread : IMainThread
    {
        public bool IsMainThread { get; set; }

        public int InvokeCount { get; private set; }

        public void BeginInvokeOnMainThread(Action action)
        {
            this.InvokeCount++;
            action();
        }

        public Task<SynchronizationContext> GetMainThreadSynchronizationContextAsync()
        {
            return Task.FromResult(new SynchronizationContext());
        }

        public Task InvokeOnMainThreadAsync(Action action)
        {
            this.InvokeCount++;
            action();
            return Task.CompletedTask;
        }

        public Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
        {
            this.InvokeCount++;
            return Task.FromResult(func());
        }

        public async Task InvokeOnMainThreadAsync(Func<Task> funcTask)
        {
            this.InvokeCount++;

            // Once the pipeline marshalled, it must see itself on the main thread.
            this.IsMainThread = true;
            await funcTask();
        }

        public async Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask)
        {
            this.InvokeCount++;
            this.IsMainThread = true;
            return await funcTask();
        }
    }
}
