using Superdev.Maui.Extensions;

namespace Superdev.Maui.Tests.Extensions
{
    public partial class EnumerableExtensionsTests
    {
        [Fact]
        public void SingleOrNone_WithNullSource_ReturnsDefault()
        {
            // Arrange
            IEnumerable<string>? source = null;

            // Act
            var result = source.SingleOrNone();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void SingleOrNone_WithEmptySource_ReturnsDefault()
        {
            // Arrange
            var source = Enumerable.Empty<string>();

            // Act
            var result = source.SingleOrNone();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void SingleOrNone_WithSingleElement_ReturnsElement()
        {
            // Arrange
            var source = new[] { "single" };

            // Act
            var result = source.SingleOrNone();

            // Assert
            result.Should().Be("single");
        }

        [Fact]
        public void SingleOrNone_WithMultipleElements_ReturnsDefault()
        {
            // Arrange
            var source = new[] { "first", "second" };

            // Act
            var result = source.SingleOrNone();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void SingleOrNone_WithSingleValueTypeElement_ReturnsElement()
        {
            // Arrange
            var source = new[] { 42 };

            // Act
            var result = source.SingleOrNone();

            // Assert
            result.Should().Be(42);
        }

        [Fact]
        public void SingleOrNone_WithMultipleValueTypeElements_ReturnsDefault()
        {
            // Arrange
            var source = new[] { 1, 2 };

            // Act
            var result = source.SingleOrNone();

            // Assert
            result.Should().Be(default(int));
        }
    }
}
