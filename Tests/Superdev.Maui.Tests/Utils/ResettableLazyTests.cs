using Superdev.Maui.Utils;

namespace Superdev.Maui.Tests.Utils
{
    public class ResettableLazyTests
    {
        [Fact]
        public void ShouldReturnLazyValue()
        {
            // Assert
            var obj = new object();
            var lazy = new ResettableLazy<object>(() => obj);

            // Act
            var lazyValue = lazy.Value;

            // Arrange
            lazyValue.Should().Be(obj);
        }

        [Fact]
        public void ShouldReturnSameValueMultipleTimes()
        {
            // Assert
            var i = 0;
            var lazy = new ResettableLazy<int>(() => ++i);

            // Act
            var lazyValue1 = lazy.Value;
            var lazyValue2 = lazy.Value;

            // Arrange
            lazyValue1.Should().Be(1);
            lazyValue2.Should().Be(1);
        }

        [Fact]
        public void ShouldReturnDifferentValueAfterReset()
        {
            // Assert
            var i = 0;
            var lazy = new ResettableLazy<int>(() => ++i);
            var lazyValue1 = lazy.Value;
            lazy.Reset();

            // Act
            var lazyValue2 = lazy.Value;

            // Arrange
            lazyValue1.Should().Be(1);
            lazyValue2.Should().Be(2);
        }

        [Fact]
        public void ShouldNotCallFuncIfValueIsNotAccessed()
        {
            // Assert
            var i = 0;

            // Act
            _ = new ResettableLazy<int>(() => ++i);

            // Arrange
            i.Should().Be(0);
        }
    }
}