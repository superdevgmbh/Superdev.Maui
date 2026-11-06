using Superdev.Maui.Utils;

namespace Superdev.Maui.Tests.Utils
{
    public class PeriodicTaskFactoryTests
    {
        [Fact]
        public async Task ShouldCompleteNormally_WhenCancelledDuringTheInitialDelay()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();

            // Act - before this was fixed the loop was an async void method, so the cancellation of the
            // initial delay escaped as an unhandled exception on the thread pool and killed the process.
            var periodicTask = PeriodicTaskFactory.Start(
                () => { },
                intervalInMilliseconds: 50,
                delayInMilliseconds: 30_000,
                cancelToken: cancellationTokenSource.Token);

            await cancellationTokenSource.CancelAsync();

            // Assert
            await periodicTask.WaitAsync(TimeSpan.FromSeconds(10));
            periodicTask.IsCompletedSuccessfully.Should().BeTrue("cancellation ends the task, it does not fault it");
        }

        [Fact]
        public async Task ShouldRunTheRequestedNumberOfIterations_AndCompleteTheReturnedTask()
        {
            // Arrange
            var iterations = 0;

            // Act
            var periodicTask = PeriodicTaskFactory.Start(
                () => Interlocked.Increment(ref iterations),
                intervalInMilliseconds: 1,
                maxIterations: 3,
                synchronous: true);

            await periodicTask.WaitAsync(TimeSpan.FromSeconds(10));

            // Assert
            iterations.Should().Be(3, "the returned task must complete only after the loop ended");
        }

        [Fact]
        public async Task ShouldNotFault_WhenAnIterationThrows()
        {
            // Act
            var periodicTask = PeriodicTaskFactory.Start(
                () => throw new InvalidOperationException("boom"),
                intervalInMilliseconds: 1,
                maxIterations: 2,
                synchronous: true);

            // Assert
            await periodicTask.WaitAsync(TimeSpan.FromSeconds(10));
            periodicTask.IsCompletedSuccessfully.Should().BeTrue("an errant iteration must not kill the periodic task");
        }
    }
}
