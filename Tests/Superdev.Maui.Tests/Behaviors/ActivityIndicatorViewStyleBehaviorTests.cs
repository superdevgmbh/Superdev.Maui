using Superdev.Maui.Behaviors;
using Superdev.Maui.Platforms.Behaviors;

namespace Superdev.Maui.Tests.Behaviors
{
    public class ActivityIndicatorViewStyleBehaviorTests
    {
        [Fact]
        public void SetViewStyle_ActivityIndicator_AddsPlatformBehaviorWithViewStyle()
        {
            // Arrange
            var activityIndicator = new ActivityIndicator();

            // Act
            ActivityIndicatorViewStyleBehavior.SetViewStyle(activityIndicator, ActivityIndicatorViewStyle.Large);

            // Assert
            activityIndicator.Behaviors.OfType<ActivityIndicatorViewStylePlatformBehavior>()
                .Should().ContainSingle()
                .Which.ViewStyle.Should().Be(ActivityIndicatorViewStyle.Large);
        }

        [Fact]
        public void SetViewStyle_Again_UpdatesTheExistingPlatformBehavior()
        {
            // Arrange
            var activityIndicator = new ActivityIndicator();
            ActivityIndicatorViewStyleBehavior.SetViewStyle(activityIndicator, ActivityIndicatorViewStyle.Large);

            // Act
            ActivityIndicatorViewStyleBehavior.SetViewStyle(activityIndicator, ActivityIndicatorViewStyle.Medium);

            // Assert
            activityIndicator.Behaviors.OfType<ActivityIndicatorViewStylePlatformBehavior>()
                .Should().ContainSingle()
                .Which.ViewStyle.Should().Be(ActivityIndicatorViewStyle.Medium);
        }

        [Fact]
        public void SetViewStyle_ElementWhichIsNoActivityIndicator_AddsNothing()
        {
            // Arrange
            var label = new Label();

            // Act
            ActivityIndicatorViewStyleBehavior.SetViewStyle(label, ActivityIndicatorViewStyle.Large);

            // Assert
            label.Behaviors.Should().BeEmpty();
        }
    }
}
