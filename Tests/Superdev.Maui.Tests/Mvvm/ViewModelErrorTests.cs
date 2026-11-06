using Superdev.Maui.Mvvm;

namespace Superdev.Maui.Tests.Mvvm
{
    public class ViewModelErrorTests
    {
        [Fact]
        public void ShouldNotBeEqual_WhenTheRetryDiffers()
        {
            // Arrange - the same text, but another retry closure: a view model replacing its error has to pick up the new one.
            Func<Task> firstRetry = () => Task.CompletedTask;
            Func<Task> secondRetry = () => Task.Delay(1);
            var first = new ViewModelError(null, "title", "text", "retry", firstRetry);
            var second = new ViewModelError(null, "title", "text", "retry", secondRetry);

            // Act & Assert
            first.Equals(second).Should().BeFalse();
            (first == second).Should().BeFalse();
        }

        [Fact]
        public void ShouldBeEqual_WhenTextAndRetryAreTheSame()
        {
            // Arrange
            Func<Task> retry = () => Task.CompletedTask;
            var first = new ViewModelError(null, "title", "text", "retry", retry);
            var second = new ViewModelError(null, "title", "text", "retry", retry);

            // Act & Assert
            first.Equals(second).Should().BeTrue();
            first.GetHashCode().Should().Be(second.GetHashCode());
        }

        [Fact]
        public void ShouldNotAllowModifyingNone()
        {
            // Arrange
            Func<Task> retry = () => Task.CompletedTask;

            // Act
            Action act = () => ViewModelError.None.WithRetry(retry, "retry");

            // Assert
            act.Should().Throw<InvalidOperationException>();
            ViewModelError.None.CanRetry.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldNotThrow_WhenTheRetryFails()
        {
            // Arrange
            Func<Task> failingRetry = () => throw new InvalidOperationException("retry failed");
            var error = new ViewModelError(null, "title", "text", "retry", failingRetry);

            // Act
            var act = () => error.RetryAsync();

            // Assert
            await act.Should().NotThrowAsync();
            error.IsBusy.Should().BeFalse("the busy flag has to be reset even if the retry failed");
        }

        [Fact]
        public async Task ShouldRunOnlyOneRetryAtATime()
        {
            // Arrange
            var retryStarted = new TaskCompletionSource();
            var releaseRetry = new TaskCompletionSource();
            var retries = 0;
            Func<Task> retry = async () =>
            {
                retries++;
                retryStarted.TrySetResult();
                await releaseRetry.Task;
            };
            var error = new ViewModelError(null, "title", "text", "retry", retry);

            // Act
            var firstRetry = error.RetryAsync();
            await retryStarted.Task;
            var canExecuteWhileBusy = error.RetryCommand!.CanExecute(null);
            var secondRetry = error.RetryAsync();
            releaseRetry.SetResult();
            await Task.WhenAll(firstRetry, secondRetry);

            // Assert
            retries.Should().Be(1, "a second tap while the retry runs must not start another one");
            canExecuteWhileBusy.Should().BeFalse();
            error.RetryCommand.CanExecute(null).Should().BeTrue();
            error.IsBusy.Should().BeFalse();
        }
    }
}
