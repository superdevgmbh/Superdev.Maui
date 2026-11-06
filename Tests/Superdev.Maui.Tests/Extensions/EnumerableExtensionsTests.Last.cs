using System.Collections;
using Superdev.Maui.Extensions;

namespace Superdev.Maui.Tests.Extensions
{
    public partial class EnumerableExtensionsTests
    {
        [Fact]
        public void Last_ReturnsTheLastElement()
        {
            // Arrange
            IEnumerable source = new[] { "a", "b", "c" };

            // Act
            var last = source.Last();

            // Assert
            last.Should().Be("c");
        }

        [Fact]
        public void Last_ReturnsNull_WhenTheLastElementIsNull()
        {
            // Arrange
            IEnumerable source = new[] { "a", null };

            // Act
            var last = source.Last();

            // Assert
            last.Should().BeNull("a null element is still an element");
        }

        [Fact]
        public void Last_EnumeratesASourceWhichIsNotAList()
        {
            // Arrange
            IEnumerable source = Enumerate(1, 2, 3);

            // Act
            var last = source.Last();

            // Assert
            last.Should().Be(3);
        }

        [Fact]
        public void Last_Throws_WhenTheSourceIsEmpty()
        {
            // Arrange
            IEnumerable source = Enumerate<int>();

            // Act
            Action action = () => source.Last();

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void LastOrDefault_ReturnsNull_WhenTheSourceIsEmpty()
        {
            // Arrange
            IEnumerable source = Array.Empty<string>();

            // Act
            var last = source.LastOrDefault();

            // Assert
            last.Should().BeNull();
        }

        [Fact]
        public void Replace_ReplacesTheFirstMatchingItem()
        {
            // Arrange
            var array = new[] { 1, 2, 3, 2 };

            // Act
            var replaced = array.Replace(i => i == 2, 9);

            // Assert
            replaced.Should().BeTrue();
            array.Should().Equal(1, 9, 3, 2);
        }

        [Fact]
        public void Replace_ReturnsFalse_WhenNothingMatches()
        {
            // Arrange
            var array = new[] { 1, 2, 3 };

            // Act
            var replaced = array.Replace(i => i == 4, 9);

            // Assert
            replaced.Should().BeFalse("nothing matched, which used to fail with an index exception");
            array.Should().Equal(1, 2, 3);
        }

        private static IEnumerable<T> Enumerate<T>(params T[] items)
        {
            foreach (var item in items)
            {
                yield return item;
            }
        }
    }
}
