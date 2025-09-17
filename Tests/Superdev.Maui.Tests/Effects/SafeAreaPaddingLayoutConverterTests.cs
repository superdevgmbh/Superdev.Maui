using FluentAssertions;
using Superdev.Maui.Effects;

namespace Superdev.Maui.Tests.Effects
{
    public class SafeAreaPaddingLayoutConverterTests
    {
        [Fact]
        public void ShouldCheckCanConvertFrom_String()
        {
            // Arrange
            var converter = new SafeAreaPaddingLayoutConverter();

            // Act
            var canConvert = converter.CanConvertFrom(typeof(string));

            // Assert
            canConvert.Should().BeTrue();
        }
        
        [Fact]
        public void ShouldCheckCanConvertTo_String()
        {
            // Arrange
            var converter = new SafeAreaPaddingLayoutConverter();

            // Act
            var canConvert = converter.CanConvertTo(typeof(string));

            // Assert
            canConvert.Should().BeTrue();
        }

        [Fact]
        public void ShouldConvertFrom()
        {
            // Arrange
            var converter = new SafeAreaPaddingLayoutConverter();

            // Act
            var output = converter.ConvertFromInvariantString("Left,Top,Right,Bottom,") as SafeAreaPaddingLayout;

            // Assert
            output.Should().BeEquivalentTo(new SafeAreaPaddingLayout(
                SafeAreaPaddingLayout.PaddingPosition.Left,
                SafeAreaPaddingLayout.PaddingPosition.Top,
                SafeAreaPaddingLayout.PaddingPosition.Right,
                SafeAreaPaddingLayout.PaddingPosition.Bottom));
        }
        
        [Fact]
        public void ShouldConvertFrom_ThrowsException()
        {
            // Arrange
            var converter = new SafeAreaPaddingLayoutConverter();

            // Act
            var action = () => converter.ConvertFromInvariantString("");

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }
        
        [Fact]
        public void ShouldConvertTo()
        {
            // Arrange
            var converter = new SafeAreaPaddingLayoutConverter();
            var safeAreaPaddingLayout = new SafeAreaPaddingLayout(
                SafeAreaPaddingLayout.PaddingPosition.Left,
                SafeAreaPaddingLayout.PaddingPosition.Right);

            // Act
            var output = converter.ConvertTo(safeAreaPaddingLayout, typeof(string)) as string;

            // Assert
            output.Should().BeEquivalentTo("Left,Right");
        }
    }
}
