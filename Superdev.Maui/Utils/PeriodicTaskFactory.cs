using System.Diagnostics;

namespace Superdev.Maui.Utils
{
    /// <summary>
    ///     Factory class to create a periodic Task to simulate a <see cref="Timer" /> using
    ///     <see cref="Task">Tasks.</see>
    /// </summary>
    public static class PeriodicTaskFactory
    {
        /// <summary>
        ///     Starts the periodic task.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="intervalInMilliseconds">The interval in milliseconds.</param>
        /// <param name="delayInMilliseconds">The delay in milliseconds, i.e. how long it waits to kick off the timer.</param>
        /// <param name="duration">
        ///     The duration.
        ///     <example>If the duration is set to 10 seconds, the maximum time this task is allowed to run is 10 seconds.</example>
        /// </param>
        /// <param name="maxIterations">The max iterations.</param>
        /// <param name="synchronous">
        ///     if set to <c>true</c> executes each period in a blocking fashion and each periodic execution of the task
        ///     is included in the total duration of the Task.
        /// </param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <param name="periodicTaskCreationOptions">
        ///     <see cref="TaskCreationOptions" /> used to create the task for executing the
        ///     <see cref="Action" />.
        /// </param>
        /// <returns>
        ///     A <see cref="Task" /> which completes when the periodic task stopped, i.e. when it ran out of iterations
        ///     or duration, or when <paramref name="cancelToken" /> was cancelled.
        /// </returns>
        /// <remarks>
        ///     Exceptions that occur in the <paramref name="action" /> need to be handled in the action itself. These exceptions
        ///     will not be bubbled up to the periodic task.
        ///     <para>
        ///     Cancelling <paramref name="cancelToken" /> ends the periodic task and completes the returned task normally;
        ///     it does not fault it.
        ///     </para>
        /// </remarks>
        public static Task Start(
            Action action,
            int intervalInMilliseconds = Timeout.Infinite,
            int delayInMilliseconds = 0,
            int duration = Timeout.Infinite,
            int maxIterations = -1,
            bool synchronous = false,
            CancellationToken cancelToken = default,
            TaskCreationOptions periodicTaskCreationOptions = TaskCreationOptions.None)
        {
            ArgumentNullException.ThrowIfNull(action);

            void WrapperAction()
            {
                cancelToken.ThrowIfCancellationRequested();
                action();
            }

            // The loop is returned as a task rather than started as a fire-and-forget void method, so that
            // a caller can await it and a cancellation cannot escape as an unhandled exception.
            return Task.Run(
                () => MainPeriodicTaskActionAsync(
                    intervalInMilliseconds,
                    delayInMilliseconds,
                    duration,
                    maxIterations,
                    cancelToken,
                    synchronous,
                    WrapperAction,
                    periodicTaskCreationOptions),
                CancellationToken.None);
        }

        /// <summary>
        ///     Runs the periodic loop until it runs out of iterations or duration, or until it is cancelled.
        /// </summary>
        private static async Task MainPeriodicTaskActionAsync(
            int intervalInMilliseconds,
            int delayInMilliseconds,
            int duration,
            int maxIterations,
            CancellationToken cancelToken,
            bool synchronous,
            Action wrapperAction,
            TaskCreationOptions periodicTaskCreationOptions)
        {
            var stopWatch = new Stopwatch();

            try
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                if (delayInMilliseconds > 0)
                {
                    await Task.Delay(delayInMilliseconds, cancelToken).ConfigureAwait(false);
                }

                if (maxIterations == 0)
                {
                    return;
                }

                var iteration = 0;

                while (!cancelToken.IsCancellationRequested)
                {
                    var subTask = Task.Factory.StartNew(
                        wrapperAction, cancelToken, periodicTaskCreationOptions, TaskScheduler.Default);

                    if (synchronous)
                    {
                        stopWatch.Start();
                        try
                        {
                            await subTask.ConfigureAwait(false);
                        }
                        catch
                        {
                            // Do not let an errant iteration kill the periodic task.
                        }

                        stopWatch.Stop();
                    }
                    else
                    {
                        // The iteration runs on its own, so its failure is observed here rather than
                        // surfacing later as an unobserved task exception.
                        _ = subTask.ContinueWith(
                            static t => _ = t.Exception,
                            CancellationToken.None,
                            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                            TaskScheduler.Default);
                    }

                    // Use the same Timeout setting as the System.Threading.Timer: an infinite timeout
                    // executes only one iteration.
                    if (intervalInMilliseconds == Timeout.Infinite)
                    {
                        break;
                    }

                    iteration++;

                    if (maxIterations > 0 && iteration >= maxIterations)
                    {
                        break;
                    }

                    stopWatch.Start();
                    await Task.Delay(intervalInMilliseconds, cancelToken).ConfigureAwait(false);
                    stopWatch.Stop();

                    if (duration > 0 && stopWatch.ElapsedMilliseconds >= duration)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Cancellation ends the periodic task; it is an expected outcome rather than a failure.
            }
        }
    }
}
