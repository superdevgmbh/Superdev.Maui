using Superdev.Maui.Behaviors;
using Superdev.Maui.Services;

namespace Superdev.Maui.Tests.Behaviors
{
    public class StatusBarBehaviorTests
    {
        [Fact]
        public void OnAttachedTo_StatusBarColorSetBefore_AppliesStatusBarColor()
        {
            // Arrange
            var statusBarServiceMock = new Mock<IStatusBarService>();
            var behavior = new StatusBarBehavior(statusBarServiceMock.Object) { StatusBarColor = Colors.Red };

            // Act
            new ContentPage().Behaviors.Add(behavior);

            // Assert
            statusBarServiceMock.Verify(s => s.SetStatusBarColor(Colors.Red), Times.Once);
        }

        [Fact]
        public void StatusBarStyle_SetWhileAttached_AppliesStyle()
        {
            // Arrange
            var statusBarServiceMock = new Mock<IStatusBarService>();
            var behavior = new StatusBarBehavior(statusBarServiceMock.Object);
            new ContentPage().Behaviors.Add(behavior);

            // Act
            behavior.StatusBarStyle = StatusBarStyle.Light;

            // Assert
            statusBarServiceMock.Verify(s => s.SetStyle(StatusBarStyle.Light), Times.Once);
        }

        [Fact]
        public void NavigationBarColor_SetWhileAttached_AppliesNavigationBarColor()
        {
            // Arrange
            var statusBarServiceMock = new Mock<IStatusBarService>();
            var behavior = new StatusBarBehavior(statusBarServiceMock.Object);
            new ContentPage().Behaviors.Add(behavior);

            // Act
            behavior.NavigationBarColor = Colors.Blue;

            // Assert
            statusBarServiceMock.Verify(s => s.SetNavigationBarColor(Colors.Blue), Times.Once);
        }

        [Fact]
        public void StatusBarColor_SetAfterDetaching_IsNotApplied()
        {
            // Arrange
            var statusBarServiceMock = new Mock<IStatusBarService>();
            var behavior = new StatusBarBehavior(statusBarServiceMock.Object);
            var page = new ContentPage();
            page.Behaviors.Add(behavior);
            page.Behaviors.Remove(behavior);

            // Act
            behavior.StatusBarColor = Colors.Red;

            // Assert
            statusBarServiceMock.Verify(s => s.SetStatusBarColor(It.IsAny<Color?>()), Times.Never);
        }
    }
}
