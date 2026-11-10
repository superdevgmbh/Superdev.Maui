using Superdev.Maui.Utils;

namespace Superdev.Maui.Tests.Utils
{
    public class TaskDelayerTests
    {
        [Fact]
        public async Task ShouldRunTheCallback_WhenThereIsNoSynchronizationContext()
        {
            // Arrange + Act - before this was fixed the callback was scheduled onto the current
            // synchronization context, which throws when there is none, and the throw was swallowed.
            var executed = await Task.Run(async () =>
            {
                SynchronizationContext.SetSynchronizationContext(null);

                var taskDelayer = new TaskDelayer();
                var wasExecuted = false;

                await taskDelayer.RunWithDelay(TimeSpan.FromMilliseconds(1), () =>
                {
                    wasExecuted = true;
                    return Task.CompletedTask;
                });

                return wasExecuted;
            });

            // Assert
            executed.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldPropagateTheException_WhichTheCallbackThrew()
        {
            // Arrange
            var taskDelayer = new TaskDelayer();

            // Act
            Func<Task> action = () => taskDelayer.RunWithDelay(
                TimeSpan.FromMilliseconds(1), () => throw new InvalidOperationException("boom"));

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("boom", "the returned task must complete when the callback does, not when it starts");
        }

        [Fact]
        public async Task ShouldRunOnlyTheMostRecentCall()
        {
            // Arrange
            var taskDelayer = new TaskDelayer();
            var executions = 0;

            // Act
            var superseded = taskDelayer.RunWithDelay(TimeSpan.FromMilliseconds(500), () =>
            {
                Interlocked.Increment(ref executions);
                return Task.CompletedTask;
            });

            var current = taskDelayer.RunWithDelay(TimeSpan.FromMilliseconds(1), () =>
            {
                Interlocked.Increment(ref executions);
                return Task.CompletedTask;
            });

            await Task.WhenAll(superseded, current).WaitAsync(TimeSpan.FromSeconds(10));

            // Assert
            executions.Should().Be(1, "a later call supersedes the pending one");
        }
    }
}
