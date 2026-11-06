using Superdev.Maui.Resources.Styles;
using Superdev.Maui.Services;

namespace Superdev.Maui.Tests.Resources.Styles
{
    public class FontConfigurationTests
    {
        [Fact]
        public void Initialize_WithScalingFontConverter_ScalesEachFontSizeOnce()
        {
            // Arrange
            var fontConverterMock = new Mock<IFontConverter>();
            fontConverterMock.Setup(c => c.GetScaledFontSize(It.IsAny<double>()))
                .Returns<double>(fontSize => fontSize * 2);
            using var fontConfiguration = new FontConfiguration(fontConverterMock.Object);

            // Act
            fontConfiguration.Initialize();

            // Assert
            fontConfiguration.Resources[ThemeConstants.FontSize.Micro].Should().Be(fontConfiguration.FontSizes.Micro * 2);
            fontConfiguration.Resources[ThemeConstants.FontSize.XXXLarge].Should().Be(fontConfiguration.FontSizes.XXXLarge * 2);
        }
    }
}
