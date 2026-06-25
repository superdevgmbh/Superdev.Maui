using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Windows.Input;
using Superdev.Maui.Services;
using Superdev.Maui.Utils.Reflection;
using Superdev.Maui.Utils.Threading;

namespace Superdev.Maui.Mvvm
{
    /// <summary>
    /// CommandGroup uses a "first-wins" strategy to serialize the execution of commands.
    /// User interfaces often have the need to lock commands against each other in order
    /// to avoid parallel execution. While a command of the group is running, concurrent
    /// executions of any command in the same group are skipped. Bind <see cref="IsAnyRunning"/>
    /// to reflect the busy state in the UI (e.g. to disable buttons or show an activity indicator).
    /// </summary>
    public class CommandGroup : BindableObject, ICommandGroup
    {
        private const long NotRunning = 0;
        private const long Running = 1;

        private readonly string name;
        private readonly IMainThread mainThread;

        private long currentState;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandGroup"/> class.
        /// </summary>
        public CommandGroup() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandGroup"/> class
        /// with <paramref name="name"/> for debugging purposes.
        /// </summary>
        public CommandGroup(string? name) : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandGroup"/> class
        /// with <paramref name="name"/> for debugging purposes and <paramref name="mainThread"/>
        /// used to marshal change notifications onto the UI thread.
        /// </summary>
        public CommandGroup(string? name, IMainThread? mainThread)
        {
            this.name = name ?? Guid.NewGuid().ToString().Substring(0, 5).ToUpperInvariant();
            this.mainThread = mainThread ?? IMainThread.Current;
        }

        /// <summary>
        /// Gets a value indicating whether any command of this group is currently running.
        /// </summary>
        public bool IsAnyRunning => Interlocked.Read(ref this.currentState) == Running;

        #region Create methods for Microsoft.Maui.Controls.Command

        public Command CreateCommand(ICommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            return this.CreateCommand<object>(
                _ => command.Execute(null),
                _ => command.CanExecute(null));
        }

        public Command<TParameter?> CreateCommand<TParameter>(ICommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            return this.CreateCommand<TParameter>(
                p => command.Execute(p),
                p => command.CanExecute(p));
        }

        public Command CreateCommand(Action execute)
        {
            return this.CreateCommand(
                execute,
                () => true);
        }

        public Command CreateCommand(Action execute, Func<bool> canExecute)
        {
            ArgumentNullException.ThrowIfNull(execute);
            ArgumentNullException.ThrowIfNull(canExecute);

            return this.CreateCommand<object>(
                _ => execute(),
                _ => canExecute());
        }

        public Command<TParameter?> CreateCommand<TParameter>(Action<TParameter?> execute)
        {
            return this.CreateCommand(
                execute,
                _ => true);
        }

        public Command<TParameter?> CreateCommand<TParameter>(Action<TParameter?> execute, Func<TParameter?, bool> canExecute)
        {
            ArgumentNullException.ThrowIfNull(execute);
            ArgumentNullException.ThrowIfNull(canExecute);

            return this.CreateCommandWithFactory<Command<TParameter?>, TParameter>(
                p => AsyncHelper.RunAsync(() => execute(p)),
                p => canExecute(p),
                (e, ce) => new Command<TParameter?>(p => this.SafeFireAndForget(e(p)), ce));
        }

        public Command<TParameter?> CreateCommand<TParameter>(Func<TParameter?, Task> execute, Func<TParameter?, bool> canExecute)
        {
            ArgumentNullException.ThrowIfNull(execute);
            ArgumentNullException.ThrowIfNull(canExecute);

            return this.CreateCommandWithFactory<Command<TParameter?>, TParameter>(
                execute,
                canExecute,
                (e, ce) => new Command<TParameter?>(p => this.SafeFireAndForget(e(p)), ce));
        }

        #endregion

        #region Create methods for generic commands

        /// <summary>
        /// Creates a command of type <typeparamref name="TCommand"/> whose execution is serialized by this group.
        /// <typeparamref name="TCommand"/> must provide a constructor accepting an execute and a can-execute delegate
        /// (<c>Func&lt;Task&gt;</c>, <c>Func&lt;bool&gt;</c>) — for example CommunityToolkit's <c>AsyncRelayCommand</c>.
        /// </summary>
        public TCommand Create<TCommand>(Func<Task> execute) where TCommand : ICommand
        {
            ArgumentNullException.ThrowIfNull(execute);

            return this.CreateCommandWithFactory(
                execute,
                () => true,
                (e, ce) => ActivatorHelper.CreateInstance<TCommand>(new object[] { e, ce }));
        }

        /// <summary>
        /// Creates a command of type <typeparamref name="TCommand"/> whose execution is serialized by this group.
        /// <typeparamref name="TCommand"/> must provide a constructor accepting an execute and a can-execute delegate
        /// (<c>Func&lt;Task&gt;</c>, <c>Func&lt;bool&gt;</c>) — for example CommunityToolkit's <c>AsyncRelayCommand</c>.
        /// </summary>
        public TCommand Create<TCommand>(Func<Task> execute, Func<bool> canExecute) where TCommand : ICommand
        {
            ArgumentNullException.ThrowIfNull(execute);
            ArgumentNullException.ThrowIfNull(canExecute);

            return this.CreateCommandWithFactory(
                execute,
                canExecute,
                (e, ce) => ActivatorHelper.CreateInstance<TCommand>(new object[] { e, ce }));
        }

        /// <summary>
        /// Creates a parameterized command of type <typeparamref name="TCommand"/> whose execution is serialized by this group.
        /// <typeparamref name="TCommand"/> must provide a constructor accepting an execute delegate and a can-execute predicate
        /// (<c>Func&lt;TParameter, Task&gt;</c>, <c>Predicate&lt;TParameter&gt;</c>) — for example CommunityToolkit's <c>AsyncRelayCommand&lt;T&gt;</c>.
        /// </summary>
        public TCommand Create<TCommand, TParameter>(Func<TParameter?, Task> execute) where TCommand : ICommand
        {
            return this.Create<TCommand, TParameter>(
                execute,
                () => true);
        }

        /// <summary>
        /// Creates a parameterized command of type <typeparamref name="TCommand"/> whose execution is serialized by this group.
        /// <typeparamref name="TCommand"/> must provide a constructor accepting an execute delegate and a can-execute predicate
        /// (<c>Func&lt;TParameter, Task&gt;</c>, <c>Predicate&lt;TParameter&gt;</c>) — for example CommunityToolkit's <c>AsyncRelayCommand&lt;T&gt;</c>.
        /// </summary>
        public TCommand Create<TCommand, TParameter>(Func<TParameter?, Task> execute, Func<bool> canExecute) where TCommand : ICommand
        {
            ArgumentNullException.ThrowIfNull(execute);
            ArgumentNullException.ThrowIfNull(canExecute);

            return this.Create<TCommand, TParameter>(
                execute,
                _ => canExecute());
        }

        /// <summary>
        /// Creates a parameterized command of type <typeparamref name="TCommand"/> whose execution is serialized by this group.
        /// <typeparamref name="TCommand"/> must provide a constructor accepting an execute delegate and a can-execute predicate
        /// (<c>Func&lt;TParameter, Task&gt;</c>, <c>Predicate&lt;TParameter&gt;</c>) — for example CommunityToolkit's <c>AsyncRelayCommand&lt;T&gt;</c>.
        /// </summary>
        public TCommand Create<TCommand, TParameter>(
            Func<TParameter?, Task> execute,
            Func<TParameter?, bool> canExecute) where TCommand : ICommand
        {
            ArgumentNullException.ThrowIfNull(execute);
            ArgumentNullException.ThrowIfNull(canExecute);

            return this.CreateCommandWithFactory(
                execute,
                canExecute,
                (e, ce) => ActivatorHelper.CreateInstance<TCommand>(new object[] { e, new Predicate<TParameter?>(p => ce(p)) }));
        }

        #endregion

        private TCommand CreateCommandWithFactory<TCommand>(
            Func<Task> execute,
            Func<bool> canExecute,
            Func<Func<Task>, Func<bool>, TCommand> factory)
            where TCommand : ICommand
        {
            return this.CreateCommandWithFactory<TCommand, object>(
                _ => execute(),
                _ => canExecute(),
                (e, ce) => factory(() => e(null), () => ce(null)));
        }

        private TCommand CreateCommandWithFactory<TCommand, TParameter>(
            Func<TParameter?, Task> execute,
            Func<TParameter?, bool> canExecute,
            Func<Func<TParameter?, Task>, Func<TParameter?, bool>, TCommand> factory)
            where TCommand : ICommand
        {
            var command = factory(
                async p =>
                {
                    if (Interlocked.CompareExchange(ref this.currentState, Running, NotRunning) == NotRunning)
                    {
                        this.RaiseIsAnyRunningChanged();
                        Debug.WriteLine($"CommandGroup {this.name}: Command execution started");

                        try
                        {
                            await execute(p);
                        }
                        finally
                        {
                            Debug.WriteLine($"CommandGroup {this.name}: Command execution finished");
                            Interlocked.Exchange(ref this.currentState, NotRunning);
                            this.RaiseIsAnyRunningChanged();
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"CommandGroup {this.name}: Command execution skipped");
                    }
                },
                canExecute);

            return command;
        }

        private void RaiseIsAnyRunningChanged()
        {
            if (this.mainThread.IsMainThread)
            {
                this.OnPropertyChanged(nameof(this.IsAnyRunning));
            }
            else
            {
                this.mainThread.BeginInvokeOnMainThread(() => this.OnPropertyChanged(nameof(this.IsAnyRunning)));
            }
        }

        private void SafeFireAndForget(Task task)
        {
            task.ContinueWith(
                t =>
                {
                    var exception = t.Exception?.InnerException ?? t.Exception;
                    if (exception != null)
                    {
                        Debug.WriteLine($"CommandGroup {this.name}: Command execution failed with an unhandled exception");

                        // The synchronous Command path is fire-and-forget and has no place to surface an
                        // exception. Instead of silently swallowing it, rethrow on the main thread so an
                        // uncaught command exception reaches the app's global handler, just like a plain
                        // Command would. Catching exceptions remains the caller's responsibility; use an
                        // async ICommand (e.g. AsyncRelayCommand) for asynchronous work.
                        this.mainThread.BeginInvokeOnMainThread(() => ExceptionDispatchInfo.Capture(exception).Throw());
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
        }
    }
}
