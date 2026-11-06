using System.Globalization;
using Superdev.Maui.Converters;

namespace Superdev.Maui.Tests.Converters
{
    public class NullToBoolConverterTests
    {
        [Theory]
        [InlineData(false, null, true)]
        [InlineData(false, "value", false)]
        [InlineData(true, null, false)]
        [InlineData(true, "value", true)]
        public void ShouldConvertNullToBool(bool inverted, object? value, bool expectedResult)
        {
            // Arrange
            var converter = new NullToBoolConverter { Inverted = inverted };

            // Act
            var result = converter.Convert(value, typeof(bool), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(
                expectedResult,
                "the inversion applies to both branches; before this was fixed the inverted converter " +
                "returned false for every input, and it ships as the NullToFalseConverter resource");
        }
    }
}
