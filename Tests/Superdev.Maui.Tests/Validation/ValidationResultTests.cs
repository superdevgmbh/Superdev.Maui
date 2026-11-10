using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    public class ValidationResultTests
    {
        [Fact]
        public void ShouldSucceed_WithNoErrors()
        {
            ValidationResult.Success.Succeeded.Should().BeTrue();
            ValidationResult.Success.Errors.Should().BeEmpty();
            ValidationResult.FromErrors(Array.Empty<ValidationError>()).Should().BeSameAs(ValidationResult.Success);
        }

        [Fact]
        public void ShouldBuildFromAPropertyToMessagesMap_InAnyDictionaryShape()
        {
            // Arrange
            IDictionary<string, string[]> problemDetails = new Dictionary<string, string[]> { ["Email"] = new[] { "taken", "invalid" } };
            var dto = new Dictionary<string, List<string>> { ["UserName"] = new List<string> { "required" }, ["Empty"] = new List<string>() };

            // Act
            var fromProblemDetails = ValidationResult.FromErrors(problemDetails);
            var fromDto = ValidationResult.FromErrors(dto);

            // Assert
            fromProblemDetails.Succeeded.Should().BeFalse();
            fromProblemDetails.Errors.Should().HaveCount(2);
            fromProblemDetails.Errors.Should().OnlyContain(e => e.PropertyName == "Email" && e.ErrorCode == ValidationErrorCodes.Custom);

            fromDto.Errors.Should().ContainSingle().Which.Message.Should().Be("required");
        }

        [Fact]
        public void ShouldBuildFromASingleError()
        {
            var result = ValidationResult.FromError("Email", "taken");

            result.Succeeded.Should().BeFalse();
            result.Errors.Single().Should().Be(new ValidationError("Email", ValidationErrorCodes.Custom, "taken"));
        }

        [Fact]
        public void ShouldConvertBackToAMap_GroupedByProperty()
        {
            // Arrange
            var result = ValidationResult.FromErrors(new[]
            {
                new ValidationError("Email", "A", "one"),
                new ValidationError("Email", "B", "two"),
                new ValidationError("UserName", "C", "three"),
            });

            // Act
            var map = result.ToErrorsDictionary();

            // Assert
            map["Email"].Should().Equal("one", "two");
            map["UserName"].Should().Equal("three");
        }
    }
}
