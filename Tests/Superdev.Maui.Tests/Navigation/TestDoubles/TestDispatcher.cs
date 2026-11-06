using Microsoft.Maui.Dispatching;

namespace Superdev.Maui.Tests.Navigation.TestDoubles
{
    /// <summary>
    /// A dispatcher which runs everything inline.
    /// Some MAUI page types, e.g. <see cref="TabbedPage"/>, require a dispatcher to be present,
    /// which a unit test process does not have.
    /// </summary>
    internal sealed class TestDispatcher : IDispatcher
    {
        public bool IsDispatchRequired => false;

        public bool Dispatch(Action action)
        {
            action();
            return true;
        }

        public bool DispatchDelayed(TimeSpan delay, Action action)
        {
            action();
            return true;
        }

        public IDispatcherTimer CreateTimer()
        {
            return new TestDispatcherTimer();
        }

        private sealed class TestDispatcherTimer : IDispatcherTimer
        {
            public TimeSpan Interval { get; set; }

            public bool IsRepeating { get; set; }

            public bool IsRunning { get; private set; }

            public event EventHandler? Tick;

            public void Start()
            {
                this.IsRunning = true;
                this.Tick?.Invoke(this, EventArgs.Empty);
            }

            public void Stop()
            {
                this.IsRunning = false;
            }
        }
    }

    /// <summary>
    /// Provides the <see cref="TestDispatcher"/> to the MAUI infrastructure.
    /// </summary>
    internal sealed class TestDispatcherProvider : IDispatcherProvider
    {
        private static readonly TestDispatcher Dispatcher = new TestDispatcher();

        /// <summary>
        /// Installs this provider once per test run.
        /// </summary>
        public static void EnsureInstalled()
        {
            DispatcherProvider.SetCurrent(new TestDispatcherProvider());
        }

        public IDispatcher? GetForCurrentThread()
        {
            return Dispatcher;
        }
    }
}
