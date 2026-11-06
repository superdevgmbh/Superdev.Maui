using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    public class ValidationMessageFormatterTests
    {
        [Fact]
        public void ShouldReplacePlaceholders_AndLeaveUnknownOnes()
        {
            var message = ValidationMessageFormatter.Format(
                "{PropertyName} must be between {From} and {To}, not {Unknown}.",
                new Dictionary<string, object?> { ["PropertyName"] = "Age", ["From"] = 0, ["To"] = 130 });

            message.Should().Be("Age must be between 0 and 130, not {Unknown}.");
        }

        [Fact]
        public void ShouldFormatValues_InTheCurrentCulture()
        {
            using var _ = new CultureScope("de-DE");

            var message = ValidationMessageFormatter.Format("{Value}", new Dictionary<string, object?> { ["Value"] = 1.5m });

            message.Should().Be("1,5");
        }

        [Fact]
        public void ShouldReturnTheTemplate_WhenThereAreNoArguments()
        {
            ValidationMessageFormatter.Format("{PropertyName} is not valid.", ValidationError.EmptyArguments)
                .Should().Be("{PropertyName} is not valid.");
        }
    }
}
