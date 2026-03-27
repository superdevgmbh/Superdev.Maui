using CommunityToolkit.Mvvm.Input;
using FluentAssertions;
using Superdev.Maui.Mvvm;
using Xunit.Abstractions;

namespace Superdev.Maui.Tests.Mvvm
{
    public class CommandGroupTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public CommandGroupTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ShouldReturnIsAnyRunningFalse()
        {
            // Arrange
            var commandGroup = new CommandGroup();

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
            var commandGroup = new CommandGroup();
            commandGroup.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(CommandGroup.IsAnyRunning))
                {
                    isAnyRunningChanges.Add(commandGroup.IsAnyRunning);
                }
            };

            IAsyncRelayCommand command = commandGroup.Create<AsyncRelayCommand>(() =>
            {
                // Simulate a long running task here
                return Task.Delay(1000);
            });

            // Act
            await command.ExecuteAsync(null);

            // Assert
            Assert.Equal(2, isAnyRunningChanges.Count);
            Assert.True(isAnyRunningChanges.ElementAt(0));
            Assert.False(isAnyRunningChanges.ElementAt(1));
        }

        [Fact]
        public async Task ShouldCreateCommand_WithoutParameter()
        {
            // Arrange
            const int numberOfParallelCalls = 1000;
            var commandGroup = new CommandGroup();
            var counter = 0;

            Task task(int id)
            {
                return Task.Run(async () =>
                {
                    // Simulate a long running task here
                    await Task.Delay(200);

                    // Access a shared resource, variable counter
                    Interlocked.Increment(ref counter);
                    this.testOutputHelper.WriteLine($"Run #{id}: \t\tcounter={counter}");
                });
            }

            var commands = Enumerable.Range(1, numberOfParallelCalls)
                .Select(id => commandGroup.CreateCommand(() => task(id).Wait()))
                .ToArray();

            // Act
            var tasks = commands
                .Select(c => Task.Run(() => c.Execute(c)))
                .ToArray();
            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(numberOfParallelCalls, commands.Length);
            Assert.Equal(numberOfParallelCalls, tasks.Length);
            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task ShouldCreateCommand_WithoutParameter_WithCanExecute()
        {
            // Arrange
            const int numberOfParallelCalls = 1000;
            var commandGroup = new CommandGroup();
            var counter = 0;

            Task task(int id)
            {
                return Task.Run(async () =>
                {
                    // Simulate a long running task here
                    await Task.Delay(200);

                    // Access a shared resource, variable counter
                    Interlocked.Increment(ref counter);
                    this.testOutputHelper.WriteLine($"Run #{id}: \t\tcounter={counter}");
                });
            }

            var commands = Enumerable.Range(1, numberOfParallelCalls)
                .Select(id => commandGroup.CreateCommand(() => task(id).Wait(), () => id % 2 == 0))
                .ToArray();

            // Act
            var tasks = commands
                .Select(c => Task.Run(() =>
                {
                    var canExecute = c.CanExecute(null);
                    if (canExecute)
                    {
                        c.Execute(null);
                    }
                    return canExecute;
                }))
                .ToArray();
            var canExecutes = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(numberOfParallelCalls, commands.Length);
            Assert.Equal(numberOfParallelCalls, tasks.Length);
            Assert.Equal(numberOfParallelCalls, canExecutes.Length);
            Assert.Equal(1, counter);

            Assert.Equal(canExecutes.Length / 2, canExecutes.Count(ce => ce == true));
            Assert.Equal(canExecutes.Length / 2, canExecutes.Count(ce => ce == false));
        }

        [Fact]
        public async Task ShouldCreateAsyncRelayCommand_WithoutParameter()
        {
            // Arrange
            const int numberOfParallelCalls = 1000;
            var commandGroup = new CommandGroup();
            var counter = 0;

            Task task(int id)
            {
                return Task.Run(async () =>
                {
                    // Simulate a long running task here
                    await Task.Delay(200);

                    // Access a shared resource, variable counter
                    Interlocked.Increment(ref counter);
                    this.testOutputHelper.WriteLine($"Run #{id}: \t\tcounter={counter}");
                    return counter;
                });
            }

            var commands = Enumerable.Range(1, numberOfParallelCalls)
                .Select(i => commandGroup.Create<AsyncRelayCommand>(() => task(i)))
                .ToArray();

            // Act
            var tasks = commands
                .Select(command => command.ExecuteAsync(null))
                .ToArray();
            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(numberOfParallelCalls, commands.Length);
            Assert.Equal(numberOfParallelCalls, tasks.Length);
            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task ShouldCreateAsyncRelayCommand_WithParameter_WithCanExecute()
        {
            // Arrange
            const int numberOfParallelCalls = 1000;
            var commandGroup = new CommandGroup();
            var counter = 0;

            Task task(int id)
            {
                return Task.Run(async () =>
                {
                    // Simulate a long running task here
                    await Task.Delay(1000);

                    // Access a shared resource, variable counter
                    Interlocked.Increment(ref counter);
                    this.testOutputHelper.WriteLine($"Run #{id}: \t\tcounter={counter}");
                    return counter;
                });
            }

            var commands = Enumerable.Range(1, numberOfParallelCalls)
                .Select(id => (Id: id, Command: commandGroup.Create<AsyncRelayCommand<int>, int>(task, i => i % 2 == 0)))
                .ToArray();

            // Act
            var tasks = commands
                .Select(c =>
                {
                    var canExecute = c.Command.CanExecute(c.Id);
                    var executionTask = canExecute ? c.Command.ExecuteAsync(c.Id) : Task.CompletedTask;
                    return (CanExecute: canExecute, ExecutionTask: executionTask);
                })
                .ToArray();
            await Task.WhenAll(tasks.Select(t => t.ExecutionTask));

            // Assert
            Assert.Equal(numberOfParallelCalls, commands.Length);
            Assert.Equal(numberOfParallelCalls, tasks.Length);
            Assert.Equal(1, counter);

            var canExecutes = tasks.Select(t => t.CanExecute).ToArray();
            Assert.Equal(canExecutes.Length / 2, canExecutes.Count(ce => ce == true));
            Assert.Equal(canExecutes.Length / 2, canExecutes.Count(ce => ce == false));
        }
    }
}