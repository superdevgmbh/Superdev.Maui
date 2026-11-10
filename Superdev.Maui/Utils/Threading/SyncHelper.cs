using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Superdev.Maui.Utils.Threading
{
    /// <summary>
    /// Runs work only once at a time. A call made while an execution is running either skips its own work
    /// or joins the running execution, depending on the overload.
    /// </summary>
    public class SyncHelper
    {
        private static readonly object NoResult = new object();
        private readonly object syncRoot = new object();

        /// <summary>
        /// The execution the current flow of control runs inside, so a call made from inside it can be told apart
        /// from a call that may wait for it.
        /// </summary>
        private readonly AsyncLocal<Task<object?>?> executionInScope = new AsyncLocal<Task<object?>?>();

        private Task<object?>? currentExecutionTask;
        private bool currentExecutionHasResult;
        private bool currentExecutionIsAsync;

        /// <summary>
        /// Indicates if the current instance of <seealso cref="SyncHelper"/> is currently running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.currentExecutionTask != null;
                }
            }
        }

        /// <summary>
        /// Runs the given <paramref name="action"/> only once at a time.
        /// A call made while an execution is running returns without running <paramref name="action"/>.
        /// </summary>
        /// <param name="action">The synchronous action.</param>
        public void RunOnce(Action action)
        {
            if (!this.TryBeginExecution(isAsync: false, out var execution))
            {
                return;
            }

            var id = $"{Guid.NewGuid():N}".Substring(0, 5).ToUpperInvariant();
            Debug.WriteLine($"RunOnce: Task {id} started");

            var previousScope = this.EnterScope(execution.Task);
            try
            {
                action();
                execution.SetResult(NoResult);
            }
            catch (Exception ex)
            {
                execution.SetException(ex);
                throw;
            }
            finally
            {
                this.executionInScope.Value = previousScope;
                Debug.WriteLine($"RunOnce: Task {id} finished");
                this.EndExecution(execution.Task);
            }
        }

        /// <summary>
        /// Runs the given <paramref name="task"/> only once at a time.
        /// A call made while an execution is running returns without running <paramref name="task"/>.
        /// </summary>
        /// <param name="task">The asynchronous task.</param>
        public async Task RunOnceAsync(Func<Task> task)
        {
            if (!this.TryBeginExecution(isAsync: true, out var execution))
            {
                return;
            }

            var id = $"{Guid.NewGuid():N}".Substring(0, 5).ToUpperInvariant();
            Debug.WriteLine($"RunOnceAsync: Task {id} started");

            var previousScope = this.EnterScope(execution.Task);
            try
            {
                await task().ConfigureAwait(false);
                execution.SetResult(NoResult);
            }
            catch (Exception ex)
            {
                execution.SetException(ex);
                throw;
            }
            finally
            {
                this.executionInScope.Value = previousScope;
                Debug.WriteLine($"RunOnceAsync: Task {id} finished");
                this.EndExecution(execution.Task);
            }
        }

        /// <summary>
        /// Runs the given <paramref name="function"/> only once at a time.
        /// A call made while an execution is running waits for it and returns its result when that is of type
        /// <typeparamref name="T"/>; otherwise it runs <paramref name="function"/> once the running execution is finished.
        /// </summary>
        /// <param name="function">The synchronous function which returns a result of type <typeparamref name="T"/>.</param>
        /// <exception cref="InvalidOperationException">
        /// The call is made from inside the running execution, which would wait for itself. Or the running execution
        /// was started by one of the <c>RunOnceAsync</c> overloads and the calling thread has a synchronization context,
        /// such as the UI thread: blocking it can deadlock an execution which needs that thread to continue.
        /// Use <see cref="RunOnceAsync{T}(Func{Task{T}})"/> there.
        /// </exception>
        public T RunOnce<T>(Func<T> function)
        {
            while (true)
            {
                var join = this.JoinOrBeginResultExecution(isAsync: false);
                if (join.Execution != null)
                {
                    return this.Run(join.Execution, function);
                }

                this.ThrowIfJoiningItself(join.ExecutionTask);
                if (join.IsAsync && SynchronizationContext.Current != null)
                {
                    throw new InvalidOperationException(
                        "RunOnce would block a thread with a synchronization context, such as the UI thread, on an execution started by RunOnceAsync, " +
                        "which may need that thread to continue. Use RunOnceAsync instead.");
                }

                var joinedResult = join.ExecutionTask.GetAwaiter().GetResult();
                if (join.HasResult && TryConvertResult<T>(joinedResult, out var typedResult))
                {
                    return typedResult;
                }

                // The running execution had no result, or one of another type: this call runs once it is finished.
                Thread.Yield();
            }
        }

        /// <summary>
        /// Runs the given <paramref name="task"/> only once at a time.
        /// A call made while an execution is running waits for it and returns its result when that is of type
        /// <typeparamref name="T"/>; otherwise it runs <paramref name="task"/> once the running execution is finished.
        /// </summary>
        /// <param name="task">The asynchronous task which returns a result of type <typeparamref name="T"/>.</param>
        /// <exception cref="InvalidOperationException">The call is made from inside the running execution, which would wait for itself.</exception>
        public async Task<T> RunOnceAsync<T>(Func<Task<T>> task)
        {
            while (true)
            {
                var join = this.JoinOrBeginResultExecution(isAsync: true);
                if (join.Execution != null)
                {
                    return await this.RunAsync(join.Execution, task).ConfigureAwait(false);
                }

                this.ThrowIfJoiningItself(join.ExecutionTask);

                var joinedResult = await join.ExecutionTask.ConfigureAwait(false);
                if (join.HasResult && TryConvertResult<T>(joinedResult, out var typedResult))
                {
                    return typedResult;
                }

                // The running execution had no result, or one of another type: this call runs once it is finished.
                await Task.Yield();
            }
        }

        private T Run<T>(TaskCompletionSource<object?> execution, Func<T> function)
        {
            var id = $"{Guid.NewGuid():N}".Substring(0, 5).ToUpperInvariant();
            Debug.WriteLine($"RunOnce: Task {id} started");

            var previousScope = this.EnterScope(execution.Task);
            try
            {
                var result = function();
                execution.SetResult(result);
                return result;
            }
            catch (Exception ex)
            {
                execution.SetException(ex);
                throw;
            }
            finally
            {
                this.executionInScope.Value = previousScope;
                Debug.WriteLine($"RunOnce: Task {id} finished");
                this.EndExecution(execution.Task);
            }
        }

        private async Task<T> RunAsync<T>(TaskCompletionSource<object?> execution, Func<Task<T>> task)
        {
            var id = $"{Guid.NewGuid():N}".Substring(0, 5).ToUpperInvariant();
            Debug.WriteLine($"RunOnceAsync: Task {id} started");

            var previousScope = this.EnterScope(execution.Task);
            try
            {
                var result = await task().ConfigureAwait(false);
                execution.SetResult(result);
                return result;
            }
            catch (Exception ex)
            {
                execution.SetException(ex);
                throw;
            }
            finally
            {
                this.executionInScope.Value = previousScope;
                Debug.WriteLine($"RunOnceAsync: Task {id} finished");
                this.EndExecution(execution.Task);
            }
        }

        private Task<object?>? EnterScope(Task<object?> executionTask)
        {
            var previousScope = this.executionInScope.Value;
            this.executionInScope.Value = executionTask;
            return previousScope;
        }

        private void ThrowIfJoiningItself(Task<object?> executionTask)
        {
            if (ReferenceEquals(this.executionInScope.Value, executionTask))
            {
                throw new InvalidOperationException(
                    "SyncHelper was called from inside its own running execution, which would wait for itself.");
            }
        }

        private bool TryBeginExecution(bool isAsync, [NotNullWhen(true)] out TaskCompletionSource<object?>? execution)
        {
            lock (this.syncRoot)
            {
                if (this.currentExecutionTask != null)
                {
                    execution = null;
                    return false;
                }

                execution = CreateExecutionSource();
                this.currentExecutionTask = execution.Task;
                this.currentExecutionHasResult = false;
                this.currentExecutionIsAsync = isAsync;
                return true;
            }
        }

        private Join JoinOrBeginResultExecution(bool isAsync)
        {
            lock (this.syncRoot)
            {
                if (this.currentExecutionTask is { } runningTask)
                {
                    return new Join(runningTask, Execution: null, this.currentExecutionHasResult, this.currentExecutionIsAsync);
                }

                var execution = CreateExecutionSource();
                this.currentExecutionTask = execution.Task;
                this.currentExecutionHasResult = true;
                this.currentExecutionIsAsync = isAsync;
                return new Join(execution.Task, execution, HasResult: true, isAsync);
            }
        }

        private void EndExecution(Task<object?> executionTask)
        {
            lock (this.syncRoot)
            {
                if (ReferenceEquals(this.currentExecutionTask, executionTask))
                {
                    this.currentExecutionTask = null;
                    this.currentExecutionHasResult = false;
                    this.currentExecutionIsAsync = false;
                }
            }
        }

        /// <summary>
        /// A joiner only takes over a result of its own type; a null result is accepted for any reference type.
        /// </summary>
        private static bool TryConvertResult<T>(object? result, out T typedResult)
        {
            if (result is T typed)
            {
                typedResult = typed;
                return true;
            }

            if (result == null && default(T) == null)
            {
                typedResult = default!;
                return true;
            }

            typedResult = default!;
            return false;
        }

        private static TaskCompletionSource<object?> CreateExecutionSource()
        {
            return new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        /// <summary>
        /// The outcome of asking for a result execution: either a new execution this call runs
        /// (<see cref="Execution"/> is set), or the running execution this call has to wait for.
        /// </summary>
        private readonly record struct Join(Task<object?> ExecutionTask, TaskCompletionSource<object?>? Execution, bool HasResult, bool IsAsync);
    }
}
