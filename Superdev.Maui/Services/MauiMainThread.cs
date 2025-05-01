namespace Superdev.Maui.Services
{
    public class MauiMainThread : IMainThread
    {
        private static readonly Lazy<IMainThread> Implementation = new Lazy<IMainThread>(CreateMainThread, LazyThreadSafetyMode.PublicationOnly);

        public static IMainThread Current => Implementation.Value;

        private static IMainThread CreateMainThread()
        {
            return new MauiMainThread();
        }

        private MauiMainThread()
        {
        }

        public bool IsMainThread => MainThread.IsMainThread;

        public void BeginInvokeOnMainThread(Action action)
        {
            MainThread.BeginInvokeOnMainThread(action);
        }

        public Task InvokeOnMainThreadAsync(Action action)
        {
            return MainThread.InvokeOnMainThreadAsync(action);
        }

        public Task InvokeOnMainThreadAsync(Func<Task> func)
        {
            return MainThread.InvokeOnMainThreadAsync(func);
        }

        public Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> func)
        {
            return MainThread.InvokeOnMainThreadAsync(func);
        }

        public Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
        {
            return MainThread.InvokeOnMainThreadAsync(func);
        }

        public Task<SynchronizationContext> GetMainThreadSynchronizationContextAsync()
        {
            return MainThread.GetMainThreadSynchronizationContextAsync();
        }
    }
}
