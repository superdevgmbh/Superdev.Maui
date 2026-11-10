using Microsoft.Extensions.Time.Testing;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    public class RunSequencerTests
    {
        [Fact]
        public void ShouldSupersedeThePreviousRun_WhenANewOneBegins()
        {
            // Arrange
            var sequencer = new RunSequencer();

            // Act
            var first = sequencer.Begin();
            var second = sequencer.Begin();

            // Assert
            first.IsCancellationRequested.Should().BeTrue();
            sequencer.IsCurrent(first).Should().BeFalse();
            sequencer.IsCurrent(second).Should().BeTrue();
        }

        [Fact]
        public void ShouldCancelTheRun_WhenTheOuterTokenIsCancelled()
        {
            // Arrange
            var sequencer = new RunSequencer();
            using var outer = new CancellationTokenSource();

            // Act
            var run = sequencer.Begin(outer.Token);
            outer.Cancel();

            // Assert
            run.IsCancellationRequested.Should().BeTrue();
        }

        [Fact]
        public void ShouldAbandonTheRun_OnCancel()
        {
            // Arrange
            var sequencer = new RunSequencer();
            var run = sequencer.Begin();

            // Act
            sequencer.Cancel();

            // Assert
            sequencer.IsCurrent(run).Should().BeFalse();
            sequencer.Invoking(d => d.Cancel()).Should().NotThrow("cancelling twice is harmless");
        }

        [Fact]
        public async Task ShouldAbandonAPendingDelay_WhenSuperseded()
        {
            // Arrange
            var time = new FakeTimeProvider();
            var sequencer = new RunSequencer();
            var first = sequencer.Begin();
            var pending = Task.Delay(TimeSpan.FromMilliseconds(400), time, first);

            // Act
            sequencer.Begin();

            // Assert
            var act = async () => await pending;
            await act.Should().ThrowAsync<OperationCanceledException>();
        }
    }
}
