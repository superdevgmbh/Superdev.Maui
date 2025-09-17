using Superdev.Maui.Extensions;

namespace Superdev.Maui.Tests.Extensions
{
    public class ColorExtensionsTests
    {
        [Theory]
        [ClassData(typeof(DarkenColorTestData))]
        public void ShouldDarkenColor(Color inputColor, float amount, string expectedHexColor)
        {
            // Act
            var output = inputColor.Darken(amount);

            // Assert
            output.ToHex().Should().Be(expectedHexColor);
        }

        private class DarkenColorTestData : TheoryData<Color, float, string>
        {
            public DarkenColorTestData()
            {
                this.Add(Colors.Red, 0.0f, "#FF0000");
                this.Add(Colors.Red, 0.1f, "#CC0000");
                this.Add(Colors.Red, 0.2f, "#990000");
                this.Add(Colors.Red, 0.3f, "#650000");
                this.Add(Colors.Red, 0.4f, "#320000");
                this.Add(Colors.Red, 0.5f, "#000000");
                this.Add(Colors.Red, 1.0f, "#000000");
            }
        }

        [Theory]
        [ClassData(typeof(BrightenColorTestData))]
        public void ShouldBrightenColor(Color inputColor, float amount, string expectedHexColor)
        {
            // Act
            var output = inputColor.Brighten(amount);

            // Assert
            output.ToHex().Should().Be(expectedHexColor);
        }

        private class BrightenColorTestData : TheoryData<Color, float, string>
        {
            public BrightenColorTestData()
            {
                this.Add(Colors.Red, 0.0f, "#FF0000");
                this.Add(Colors.Red, 0.1f, "#FF3333");
                this.Add(Colors.Red, 0.2f, "#FF6566");
                this.Add(Colors.Red, 0.3f, "#FE9999");
                this.Add(Colors.Red, 0.4f, "#FFCBCC");
                this.Add(Colors.Red, 0.5f, "#FFFFFF");
                this.Add(Colors.Red, 1.0f, "#FFFFFF");
            }
        }

        [Theory]
        [ClassData(typeof(BrightenRelativeColorTestData))]
        public void ShouldBrightenRelativeColor(Color inputColor, float amount, string expectedHexColor)
        {
            // Act
            var output = inputColor.BrightenRelative(amount);

            // Assert
            output.ToHex().Should().Be(expectedHexColor);
        }

        private class BrightenRelativeColorTestData : TheoryData<Color, float, string>
        {
            public BrightenRelativeColorTestData()
            {
                this.Add(Colors.Red, 0.5f, "#FF7F7F");
            }
        }
    }
}