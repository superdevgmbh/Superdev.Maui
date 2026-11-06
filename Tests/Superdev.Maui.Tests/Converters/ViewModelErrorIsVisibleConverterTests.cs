using System.Globalization;
using Superdev.Maui.Converters;
using Superdev.Maui.Mvvm;

namespace Superdev.Maui.Tests.Converters
{
    public class ViewModelErrorIsVisibleConverterTests
    {
        public static TheoryData<object?, bool, bool> ConvertTestData => new TheoryData<object?, bool, bool>
        {
            // value, isInverted, expected
            { new ViewModelError(null, "Title", "Text"), false, true },
            { new ViewModelError(null, "Title", "Text"), true, false },
            { ViewModelError.None, false, false },
            { ViewModelError.None, true, true },
            { null, false, false },
            { null, true, true },
            { "not an error", false, false },
            { "not an error", true, true },
        };

        [Theory]
        [MemberData(nameof(ConvertTestData))]
        public void ShouldConvert(object? value, bool isInverted, bool expected)
        {
            // Arrange
            var converter = new ViewModelErrorIsVisibleConverter { IsInverted = isInverted };

            // Act
            var isVisible = converter.Convert(value, typeof(bool), null, CultureInfo.InvariantCulture);

            // Assert
            isVisible.Should().Be(expected);
        }

        [Fact]
        public void ShouldThrowNotSupportedException_ConvertBack()
        {
            // Arrange
            var converter = new ViewModelErrorIsVisibleConverter();

            // Act
            Action action = () => converter.ConvertBack(true, typeof(ViewModelError), null, CultureInfo.InvariantCulture);

            // Assert
            action.Should().Throw<NotSupportedException>();
        }
    }
}
