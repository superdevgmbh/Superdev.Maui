using Superdev.Maui.Controls;

namespace Superdev.Maui.Tests.Controls
{
    public class CustomScrollViewTests
    {
        [Fact]
        public void IsScrollEnabled_Default_IsTrue()
        {
            // Act
            var customScrollView = new CustomScrollView();

            // Assert
            customScrollView.IsScrollEnabled.Should().BeTrue("a scroll view scrolls unless told not to, like MAUI's own");
        }

        [Fact]
        public void IsBounceEnabled_Default_IsFalse()
        {
            // Act
            var customScrollView = new CustomScrollView();

            // Assert
            customScrollView.IsBounceEnabled.Should().BeFalse();
        }
    }
}
