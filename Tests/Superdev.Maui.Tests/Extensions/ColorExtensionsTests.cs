using Superdev.Maui.Extensions;
using Superdev.Maui.Services;

namespace Superdev.Maui.Tests.Extensions
{
    public class ColorExtensionsTests
    {
        [Theory]
        [InlineData("#000000", "#FFFFFF")]
        [InlineData("#FFFFFF", "#000000")]
        public void ShouldInvertColorWithAlpha(string inputHexColor, string expectedHexColor)
        {
            // Arrange
            var inputColor = Color.FromArgb(inputHexColor);

            // Act - the channels are 0..1; inverting on a 0..255 scale clamped every channel to full
            var output = inputColor.Invert(alpha: 1f);

            // Assert
            output.ToHex().Should().Be(expectedHexColor);
        }

        [Fact]
        public void ShouldInvertEveryChannelOfAMidToneColor()
        {
            // Arrange - the case the 0..255 arithmetic got wrong: every channel below full clamped to 1
            var inputColor = Color.FromArgb("#3399FF");

            // Act
            var output = inputColor.Invert(alpha: 0.5f);

            // Assert
            output.Red.Should().BeApproximately(1f - inputColor.Red, 0.01f);
            output.Green.Should().BeApproximately(1f - inputColor.Green, 0.01f);
            output.Blue.Should().BeApproximately(1f - inputColor.Blue, 0.01f);
            output.Alpha.Should().BeApproximately(0.5f, 0.01f);
        }

        [Theory]
        [InlineData("#000000", StatusBarStyle.Dark)]
        [InlineData("#202020", StatusBarStyle.Dark)]
        [InlineData("#FFFFFF", StatusBarStyle.Light)]
        [InlineData("#F0F0F0", StatusBarStyle.Light)]
        public void ShouldDeriveStatusBarStyleFromLuma(string inputHexColor, StatusBarStyle expectedStatusBarStyle)
        {
            // Arrange
            var inputColor = Color.FromArgb(inputHexColor);

            // Act - luma is 0..1, so comparing it against 128 always yielded Dark
            var output = inputColor.ToStatusBarStyle();

            // Assert
            output.Should().Be(expectedStatusBarStyle);
        }

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