using Superdev.Maui.Utils.Threading;

namespace Superdev.Maui.Tests.Utils.Threading
{
    public class AsyncSemaphoreTests
    {
        [ThreadStatic]
        private static bool isReleasing;

        [Fact]
        public void ShouldThrow_WhenInitialCountIsNegative()
        {
            // Act
            Action action = () => new AsyncSemaphore(-1);

            // Assert
            action.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("initialCount");
        }

        [Fact]
        public async Task ShouldAdmitInitialCount_ThenQueue()
        {
            // Arrange
            var semaphore = new AsyncSemaphore(2);

            // Act
            await semaphore.WaitAsync();
            await semaphore.WaitAsync();
            var third = semaphore.WaitAsync();

            // Assert
            third.IsCompleted.Should().BeFalse("both slots are taken");

            semaphore.Release();
            await third;
        }

        [Fact]
        public async Task ShouldNotRunTheWaiterInsideTheRelease()
        {
            // Arrange
            var semaphore = new AsyncSemaphore(0);

            var waiterRanInsideRelease = false;
            var waiter = WaitAndReportAsync(semaphore, () => waiterRanInsideRelease = isReleasing);

            // Act
            isReleasing = true;
            semaphore.Release();
            isReleasing = false;
            await waiter;

            // Assert
            waiterRanInsideRelease.Should().BeFalse("the continuation of a waiter runs asynchronously, not inside Release");
        }

        [Fact]
        public async Task ShouldGiveUpTheWait_WhenCancelled()
        {
            // Arrange
            var semaphore = new AsyncSemaphore(0);
            using var cancellation = new CancellationTokenSource();

            // Act
            var waiter = semaphore.WaitAsync(cancellation.Token);
            cancellation.Cancel();

            // Assert
            Func<Task> awaiting = () => waiter;
            await awaiting.Should().ThrowAsync<OperationCanceledException>();

            semaphore.Release();
            await semaphore.WaitAsync();
        }

        private static async Task WaitAndReportAsync(AsyncSemaphore semaphore, Action report)
        {
            await semaphore.WaitAsync();
            report();
        }
    }
}
