using Superdev.Maui.Mvvm;

namespace Superdev.Maui.Tests.Mvvm
{
    public class DeveloperModeTests
    {
        private readonly ManualTimeProvider timeProvider = new ManualTimeProvider();

        [Fact]
        public void ShouldUnlock_AfterTheRequiredNumberOfCalls()
        {
            // Arrange
            var developerMode = new DeveloperMode(this.timeProvider);
            var unlockedEvents = 0;
            developerMode.UnlockedEvent += (_, _) => unlockedEvents++;

            // Act
            for (var i = 0; i < DeveloperMode.RequiredUnlockCount - 1; i++)
            {
                developerMode.Unlock();
                this.timeProvider.Advance(TimeSpan.FromMilliseconds(500));
            }

            var unlockedBeforeLastCall = developerMode.Unlocked;
            developerMode.Unlock();

            // Assert
            unlockedBeforeLastCall.Should().BeFalse();
            developerMode.Unlocked.Should().BeTrue();
            unlockedEvents.Should().Be(1);
        }

        [Fact]
        public void ShouldUnlock_ThroughTheCommand()
        {
            // Arrange
            var developerMode = new DeveloperMode(this.timeProvider);

            // Act
            for (var i = 0; i < DeveloperMode.RequiredUnlockCount; i++)
            {
                developerMode.UnlockCommand.Execute(null);
            }

            // Assert
            developerMode.Unlocked.Should().BeTrue();
        }

        [Fact]
        public void ShouldRaiseTheEventOnlyOnce_WhenCallsContinueAfterUnlocking()
        {
            // Arrange
            var developerMode = new DeveloperMode(this.timeProvider);
            var unlockedEvents = 0;
            developerMode.UnlockedEvent += (_, _) => unlockedEvents++;

            // Act
            for (var i = 0; i < DeveloperMode.RequiredUnlockCount * 3; i++)
            {
                developerMode.Unlock();
            }

            // Assert
            unlockedEvents.Should().Be(1);
        }

        [Fact]
        public void ShouldStartOver_WhenTheTimeoutElapsesBetweenTwoCalls()
        {
            // Arrange
            var developerMode = new DeveloperMode(this.timeProvider);
            for (var i = 0; i < DeveloperMode.RequiredUnlockCount - 1; i++)
            {
                developerMode.Unlock();
            }

            // Act
            this.timeProvider.Advance(DeveloperMode.UnlockTimeout + TimeSpan.FromMilliseconds(1));
            developerMode.Unlock();
            var unlockedAfterLateCall = developerMode.Unlocked;

            for (var i = 0; i < DeveloperMode.RequiredUnlockCount - 1; i++)
            {
                developerMode.Unlock();
            }

            // Assert
            unlockedAfterLateCall.Should().BeFalse("the earlier calls expired, so the late call started a new sequence");
            developerMode.Unlocked.Should().BeTrue();
        }

        [Fact]
        public void ShouldSlideTheWindow_WithEveryCall()
        {
            // Arrange - the calls span more than the timeout in total, but no two of them are further apart than the timeout.
            var developerMode = new DeveloperMode(this.timeProvider);
            var interval = DeveloperMode.UnlockTimeout - TimeSpan.FromMilliseconds(100);

            // Act
            for (var i = 0; i < DeveloperMode.RequiredUnlockCount; i++)
            {
                developerMode.Unlock();
                this.timeProvider.Advance(interval);
            }

            // Assert
            developerMode.Unlocked.Should().BeTrue("a window anchored to the first call would have expired");
        }

        [Fact]
        public void ShouldUnlockAgain_AfterItWasLocked()
        {
            // Arrange
            var developerMode = new DeveloperMode(this.timeProvider);
            var unlockedEvents = 0;
            developerMode.UnlockedEvent += (_, _) => unlockedEvents++;

            for (var i = 0; i < DeveloperMode.RequiredUnlockCount; i++)
            {
                developerMode.Unlock();
            }

            // Act
            developerMode.Unlocked = false;

            for (var i = 0; i < DeveloperMode.RequiredUnlockCount; i++)
            {
                developerMode.Unlock();
            }

            // Assert
            developerMode.Unlocked.Should().BeTrue();
            unlockedEvents.Should().Be(2);
        }

        private sealed class ManualTimeProvider : TimeProvider
        {
            private long timestamp;

            public override long TimestampFrequency => TimeSpan.TicksPerSecond;

            public override long GetTimestamp()
            {
                return this.timestamp;
            }

            public void Advance(TimeSpan duration)
            {
                this.timestamp += duration.Ticks;
            }
        }
    }
}
