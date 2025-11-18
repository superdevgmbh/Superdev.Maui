using System.Diagnostics;
using System.Windows.Input;
using Superdev.Maui.Utils.Reflection;
using Superdev.Maui.Utils.Threading;

namespace Superdev.Maui.Mvvm
{
    /// <summary>
    /// CommandGroup uses a "first-wins" strategy to serialize the execution of commands.
    /// User interfaces often have the need to lock commands against each other in order
    /// to avoid parallel execution.
    /// </summary>
    public class CommandGroup : BindableObject, ICommandGroup
    {
        private const long NotRunning = 0;
        private const long Running = 1;

        private readonly string name;

        private long currentState;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandGroup"/> class.
        /// </summary>
        public CommandGroup() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandGroup"/> class
        /// with <param name="name">the name of the instance</param> for debugging purposes.
        /// </summary>
        public CommandGroup(string name)
        {
            this.name = name ?? Guid.NewGuid().ToString().Substring(0, 5).ToUpperInvariant();
        }

        public bool IsAnyRunning => Interlocked.Read(ref this.currentState) == Running;

        #region Create methods for Microsoft.Maui.Controls.Command

        public Command CreateCommand(ICommand command)
        {
            return new Command(command.Execute, command.CanExecute);
        }

        public Command CreateCommand(Action execute)
        {
            return this.CreateCommand(
                execute,
                () => true);
        }

        public Command CreateCommand(Action execute, Func<bool> canExecute)
        {
            return this.CreateCommand<object>(
                _ => execute(),
                _ => canExecute());
        }

        public Command CreateCommand<TParameter>(Action<TParameter> execute)
        {
            return this.CreateCommand(
                execute,
                _ => true);
        }

        public Command CreateCommand<TParameter>(Action<TParameter> execute, Func<TParameter, bool> canExecute)
        {
            return this.CreateCommandWithFactory<Command, TParameter>(
                p => AsyncHelper.RunAsync(() => execute(p)),
                p => canExecute(p),
                (e, ce) => new Command<TParameter>(async p => await e(p), ce));
        }

        public Command CreateCommand<TParameter>(Func<TParameter, Task> execute, Func<TParameter, bool> canExecute)
        {
            return this.CreateCommandWithFactory<Command, TParameter>(
                execute,
                canExecute,
                (e, ce) => new Command<TParameter>(async p => await e(p), ce));
        }

        #endregion

        #region Create methods for generic commands

        public T Create<T>(Func<Task> execute) where T : ICommand
        {
            return this.CreateCommandWithFactory(
               execute,
               () => true,
               (e, ce) => ActivatorHelper.CreateInstance<T>(new object[] { e, ce }));
        }

        public T Create<T>(Func<Task> execute, Func<bool> canExecute) where T : ICommand
        {
            return this.CreateCommandWithFactory(
               execute,
               canExecute,
               (e, ce) => ActivatorHelper.CreateInstance<T>(new object[] { e, ce }));
        }

        public T Create<T, TParameter>(Func<TParameter, Task> execute) where T : ICommand
        {
            return this.Create<T, TParameter>(
                execute,
                () => true);
        }

        public T Create<T, TParameter>(Func<TParameter, Task> execute, Func<bool> canExecute) where T : ICommand
        {
            return this.Create<T, TParameter>(
                execute,
                _ => canExecute());
        }

        public T Create<T, TParameter>(
            Func<TParameter, Task> execute,
            Func<TParameter, bool> canExecute) where T : ICommand
        {
            return this.CreateCommandWithFactory(
                execute,
                canExecute,
                (e, ce) => ActivatorHelper.CreateInstance<T>(new object[] { e, new Predicate<TParameter>(p => ce(p)) }));
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
            Func<TParameter, Task> execute,
            Func<TParameter, bool> canExecute,
            Func<Func<TParameter, Task>, Func<TParameter, bool>, TCommand> factory)
            where TCommand : ICommand
        {
            var command = factory(
                async p =>
                {
                    if (Interlocked.CompareExchange(ref this.currentState, Running, NotRunning) == NotRunning)
                    {
                        this.OnPropertyChanged(nameof(this.IsAnyRunning));
                        Debug.WriteLine($"CommandGroup {this.name}: Command execution started");

                        try
                        {
                            await execute(p);
                        }
                        finally
                        {
                            Debug.WriteLine($"CommandGroup {this.name}: Command execution finished");
                            Interlocked.Exchange(ref this.currentState, NotRunning);
                            this.OnPropertyChanged(nameof(this.IsAnyRunning));
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
    }
}