using Superdev.Maui.Extensions;

namespace Superdev.Maui.Tests.Extensions
{
    public class IntegerExtensionsTests
    {
        [Fact]
        public void IntegerIsOddTest()
        {
            // Arrange.
            const int value = 1;

            // Act.
            var isOdd = value.IsOdd();

            // Assert.
            isOdd.Should().BeTrue();
        }

        [Fact]
        public void IntegerIsEvenTest()
        {
            // Arrange.
            const int value = 2;

            // Act.
            var isOdd = value.IsOdd();

            // Assert.
            isOdd.Should().BeFalse();
        }
    }
}