using Superdev.Maui.Utils.Threading;

namespace Superdev.Maui.Tests.Utils.Threading
{
    public class AsyncLockTests
    {
        [ThreadStatic]
        private static bool isReleasing;

        [Fact]
        public async Task ShouldSerializeHolders()
        {
            // Arrange
            var asyncLock = new AsyncLock();
            var first = await asyncLock.LockAsync();

            // Act
            var second = asyncLock.LockAsync();

            // Assert
            second.IsCompleted.Should().BeFalse("the lock is held");

            first.Dispose();
            using (await second)
            {
            }
        }

        [Fact]
        public async Task ShouldReleaseOnce_WhenTheReleaserIsDisposedTwice()
        {
            // Arrange
            var asyncLock = new AsyncLock();
            var releaser = await asyncLock.LockAsync();
            var copy = releaser;

            // Act - the struct was copied and both copies are disposed
            releaser.Dispose();
            copy.Dispose();

            // Assert - the lock was released once: the next holder gets in, the one after has to wait
            var second = await asyncLock.LockAsync();
            var third = asyncLock.LockAsync();
            third.IsCompleted.Should().BeFalse("a second dispose must not release the lock a second time");

            second.Dispose();
            (await third).Dispose();
        }

        [Fact]
        public async Task ShouldNotRunTheWaiterInsideTheRelease()
        {
            // Arrange
            var asyncLock = new AsyncLock();
            var holder = await asyncLock.LockAsync();

            var waiterRanInsideRelease = false;
            var waiter = WaitAndReportAsync(asyncLock, () => waiterRanInsideRelease = isReleasing);
            waiter.IsCompleted.Should().BeFalse("the waiter is queued behind the holder");

            // Act
            isReleasing = true;
            holder.Dispose();
            isReleasing = false;
            await waiter;

            // Assert - a waiter which ran inside the release would nest its whole continuation into the
            // holder's dispose, and the stack would grow with every waiter in the queue
            waiterRanInsideRelease.Should().BeFalse("the continuation of a waiter runs asynchronously, not inside Dispose");
        }

        [Fact]
        public async Task ShouldGiveUpTheWait_WhenCancelled()
        {
            // Arrange
            var asyncLock = new AsyncLock();
            var holder = await asyncLock.LockAsync();
            using var cancellation = new CancellationTokenSource();

            // Act
            var waiter = asyncLock.LockAsync(cancellation.Token);
            cancellation.Cancel();

            // Assert
            Func<Task> awaiting = () => waiter;
            await awaiting.Should().ThrowAsync<OperationCanceledException>();

            holder.Dispose();
            using (await asyncLock.LockAsync())
            {
                // The cancelled waiter did not take the lock.
            }
        }

        private static async Task WaitAndReportAsync(AsyncLock asyncLock, Action report)
        {
            using (await asyncLock.LockAsync())
            {
                report();
            }
        }
    }
}
