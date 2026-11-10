using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;

namespace Superdev.Maui.Tests.Mvvm
{
    public class CommandGroupTests
    {
        private const int NumberOfCommands = 1000;
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

        [Fact]
        public void ShouldReturnIsAnyRunningFalse()
        {
            // Arrange
            var commandGroup = new CommandGroup(null, new StaticMainThread());

            // Act
            var isAnyRunning = commandGroup.IsAnyRunning;

            // Assert
            isAnyRunning.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldReturnIsAnyRunningTrue()
        {
            // Arrange
            var isAnyRunningChanges = new List<bool>();
            var commandGroup = new CommandGroup(null, new StaticMainThread());
            commandGroup.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(CommandGroup.IsAnyRunning))
                {
                    isAnyRunningChanges.Add(commandGroup.IsAnyRunning);
                }
            };

            IAsyncRelayCommand command = commandGroup.Create<AsyncRelayCommand>(async () => await Task.Yield());

            // Act
            await command.ExecuteAsync(null);

            // Assert
            isAnyRunningChanges.Should().Equal(true, false);
        }

        [Fact]
        public void ShouldForwardCanExecuteChanged_OfAWrappedCommand()
        {
            // Arrange
            var commandGroup = new CommandGroup(null, new StaticMainThread());
            var canExecute = false;
            var innerCommand = new Microsoft.Maui.Controls.Command(() => { }, () => canExecute);
            var wrapper = commandGroup.CreateCommand(innerCommand);
            var canExecuteChangedRaised = 0;
            wrapper.CanExecuteChanged += (_, _) => canExecuteChangedRaised++;

            // Act
            canExecute = true;
            innerCommand.ChangeCanExecute();

            // Assert
            canExecuteChangedRaised.Should().Be(1, "a bound button re-queries the wrapper when the wrapped command changes");
            wrapper.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void ShouldThrowSynchronously_WhenASynchronousCommandThrows()
        {
            // Arrange
            var commandGroup = new CommandGroup(null, new StaticMainThread());
            var command = commandGroup.CreateCommand(() => throw new InvalidOperationException("boom"));

            // Act
            Action act = () => command.Execute(null);

            // Assert - the same as a plain Command: the caller of Execute sees the exception.
            act.Should().Throw<InvalidOperationException>().WithMessage("boom");
            commandGroup.IsAnyRunning.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldCreateCommand_WithoutParameter()
        {
            // Arrange
            var commandGroup = new CommandGroup(null, new StaticMainThread());
            var execution = new GatedExecution();
            var commands = Enumerable.Range(1, NumberOfCommands)
                .Select(_ => commandGroup.CreateCommand(execution.Run))
                .ToArray();

            // Act
            var first = Task.Run(() => commands[0].Execute(null));
            await execution.WaitUntilStartedAsync();
            await Task.WhenAll(commands.Skip(1).Select(c => Task.Run(() => c.Execute(null)))).WaitAsync(Timeout);
            execution.Release();
            await first.WaitAsync(Timeout);

            // Assert
            execution.Count.Should().Be(1, "every command issued while the first one runs is skipped");
            commandGroup.IsAnyRunning.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldCreateCommand_WithoutParameter_WithCanExecute()
        {
            // Arrange
            var commandGroup = new CommandGroup(null, new StaticMainThread());
            var execution = new GatedExecution();
            var commands = Enumerable.Range(1, NumberOfCommands)
                .Select(id => commandGroup.CreateCommand(execution.Run, () => id % 2 == 0))
                .ToArray();

            // Act
            var first = Task.Run(() => commands[1].Execute(null));
            await execution.WaitUntilStartedAsync();
            var canExecutes = await Task.WhenAll(commands.Select(c => Task.Run(() =>
                {
                    var canExecute = c.CanExecute(null);
                    if (canExecute)
                    {
                        c.Execute(null);
                    }

                    return canExecute;
                })))
                .WaitAsync(Timeout);
            execution.Release();
            await first.WaitAsync(Timeout);

            // Assert
            execution.Count.Should().Be(1, "every command issued while the first one runs is skipped");
            canExecutes.Count(ce => ce).Should().Be(NumberOfCommands / 2);
            canExecutes.Count(ce => !ce).Should().Be(NumberOfCommands / 2);
        }

        [Fact]
        public async Task ShouldCreateAsyncRelayCommand_WithoutParameter()
        {
            // Arrange
            var commandGroup = new CommandGroup(null, new StaticMainThread());
            var execution = new GatedExecution();
            var commands = Enumerable.Range(1, NumberOfCommands)
                .Select(_ => commandGroup.Create<AsyncRelayCommand>(() => execution.RunAsync()))
                .ToArray();

            // Act
            var first = commands[0].ExecuteAsync(null);
            await execution.WaitUntilStartedAsync();
            await Task.WhenAll(commands.Skip(1).Select(c => c.ExecuteAsync(null))).WaitAsync(Timeout);
            execution.Release();
            await first.WaitAsync(Timeout);

            // Assert
            execution.Count.Should().Be(1, "every command issued while the first one runs is skipped");
            commandGroup.IsAnyRunning.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldCreateAsyncRelayCommand_WithParameter_WithCanExecute()
        {
            // Arrange
            var commandGroup = new CommandGroup(null, new StaticMainThread());
            var execution = new GatedExecution();
            var commands = Enumerable.Range(1, NumberOfCommands)
                .Select(id => (Id: id, Command: commandGroup.Create<AsyncRelayCommand<int>, int>(_ => execution.RunAsync(), i => i % 2 == 0)))
                .ToArray();

            // Act - an AsyncRelayCommand cannot execute while it runs itself, so every command is asked before the first starts.
            var canExecutes = commands.Select(c => c.Command.CanExecute(c.Id)).ToArray();
            var first = commands[1].Command.ExecuteAsync(commands[1].Id);
            await execution.WaitUntilStartedAsync();
            var others = commands
                .Where((_, i) => i != 1 && canExecutes[i])
                .Select(c => c.Command.ExecuteAsync(c.Id));
            await Task.WhenAll(others).WaitAsync(Timeout);
            execution.Release();
            await first.WaitAsync(Timeout);

            // Assert
            execution.Count.Should().Be(1, "every command issued while the first one runs is skipped");
            canExecutes.Count(ce => ce).Should().Be(NumberOfCommands / 2);
            canExecutes.Count(ce => !ce).Should().Be(NumberOfCommands / 2);
        }

        /// <summary>
        /// Work which counts its runs and holds the first one until <see cref="Release"/>, so the test decides
        /// when it ends instead of relying on how long a delay takes.
        /// </summary>
        private sealed class GatedExecution
        {
            private readonly TaskCompletionSource started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly TaskCompletionSource gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            private int count;

            public int Count => Volatile.Read(ref this.count);

            public void Run()
            {
                this.Start();
                this.gate.Task.Wait(Timeout);
            }

            public async Task RunAsync()
            {
                this.Start();
                await this.gate.Task.WaitAsync(Timeout);
            }

            public Task WaitUntilStartedAsync()
            {
                return this.started.Task.WaitAsync(Timeout);
            }

            public void Release()
            {
                this.gate.TrySetResult();
            }

            private void Start()
            {
                Interlocked.Increment(ref this.count);
                this.started.TrySetResult();
            }
        }
    }
}
