using System.Diagnostics;

namespace Superdev.Maui.Utils.Threading
{
    public class SyncHelper
    {
        private const int NotRunning = 0;
        private const int Running = 1;
        private int currentState;

        public async Task RunOnceAsync(Func<Task> task)
        {
            if (Interlocked.CompareExchange(ref this.currentState, Running, NotRunning) == NotRunning)
            {
                // The given task is only executed if we pass this atomic CompareExchange call,
                // which switches the current state flag from 'not running' to 'running'.

                var id = $"{Guid.NewGuid():N}".Substring(0, 5).ToUpperInvariant();
                Debug.WriteLine($"RunOnceAsync: Task {id} started");

                try
                {
                    await task();
                }
                finally
                {
                    Debug.WriteLine($"RunOnceAsync: Task {id} finished");
                    Interlocked.Exchange(ref this.currentState, NotRunning);
                }
            }

            // All other method calls which can't make it into the critical section
            // are just returned immediately.
        }
    }
}