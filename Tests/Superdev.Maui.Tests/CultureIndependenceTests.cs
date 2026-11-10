using Superdev.Maui.Extensions;
using Superdev.Maui.Utils;

namespace Superdev.Maui.Tests
{
    /// <summary>
    /// The formatted output of these helpers must not depend on the culture of the device.
    /// Before this was fixed, every case here failed on any culture with a comma decimal separator.
    /// </summary>
    public class CultureIndependenceTests
    {
        [Theory]
        [InlineData("de-DE")]
        [InlineData("fr-FR")]
        [InlineData("en-US")]
        public void ShouldFormatByteSizeIndependentlyOfCulture(string culture)
        {
            // Arrange
            using var cultureScope = new CultureScope(culture);

            // Act
            var namedSize = ByteFormatter.GetNamedSize(1441792);

            // Assert
            namedSize.Should().Be("1.4 MB");
        }

        [Theory]
        [InlineData("de-DE")]
        [InlineData("fr-FR")]
        [InlineData("en-US")]
        public void ShouldFormatSecondsIndependentlyOfCulture(string culture)
        {
            // Arrange
            using var cultureScope = new CultureScope(culture);

            // Act
            var seconds = TimeSpan.FromMilliseconds(1).ToSecondsString();

            // Assert
            seconds.Should().Be("0.001s");
        }

        [Fact]
        public void ShouldKeepDecimalPlacesForNegativeByteSizes()
        {
            // Act
            var namedSize = ByteFormatter.GetNamedSize(-1024, decimalPlaces: 0);

            // Assert
            namedSize.Should().Be("-1 KB", "the decimal places must survive the negative branch");
        }
    }
}
