using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    public class ValidationErrorTests
    {
        [Fact]
        public void ShouldCarryTheBackendsFourMembers()
        {
            // Arrange
            var arguments = new Dictionary<string, object?> { ["MinimumLength"] = 3 };

            // Act
            var error = new ValidationError("UserName", "MinimumLength", "too short", arguments);

            // Assert
            error.PropertyName.Should().Be("UserName");
            error.ErrorCode.Should().Be("MinimumLength");
            error.Message.Should().Be("too short");
            error.Arguments.Should().BeSameAs(arguments);
        }

        [Fact]
        public void ShouldDefaultToNoArguments()
        {
            var error = new ValidationError("UserName", "NotEmpty", "required");

            error.Arguments.Should().BeEmpty();
            error.Arguments.Should().BeSameAs(ValidationError.EmptyArguments);
        }

        [Fact]
        public void ShouldCompareByValue()
        {
            var a = new ValidationError("UserName", "NotEmpty", "required");
            var b = new ValidationError("UserName", "NotEmpty", "required");

            a.Should().Be(b);
            (a with { Message = "other" }).Should().NotBe(b);
        }
    }
}
