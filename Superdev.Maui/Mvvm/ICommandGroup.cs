using System.Windows.Input;

namespace Superdev.Maui.Mvvm
{
    public interface ICommandGroup
    {
        bool IsAnyRunning { get; }
        
        Command CreateCommand(Action execute);

        Command CreateCommand(Action execute, Func<bool> canExecute);
        
        Command CreateCommand(ICommand command);
        
        Command CreateCommand<TParameter>(Action<TParameter> execute);
        
        Command CreateCommand<TParameter>(Action<TParameter> execute, Func<TParameter, bool> canExecute);
        
        Command CreateCommand<TParameter>(Func<TParameter, Task> execute, Func<TParameter, bool> canExecute);

        TCommand Create<TCommand>(Func<Task> execute) where TCommand : ICommand;

        TCommand Create<TCommand>(Func<Task> execute, Func<bool> canExecute) where TCommand : ICommand;

        TCommand Create<TCommand, TParameter>(Func<TParameter, Task> execute) where TCommand : ICommand;

        TCommand Create<TCommand, TParameter>(Func<TParameter, Task> execute, Func<bool> canExecute) where TCommand : ICommand;

        TCommand Create<TCommand, TParameter>(Func<TParameter, Task> execute, Func<TParameter, bool> canExecute) where TCommand : ICommand;
    }
}