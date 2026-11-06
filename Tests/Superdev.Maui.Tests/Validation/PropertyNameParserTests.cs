using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    public class PropertyNameParserTests
    {
        [Theory]
        [InlineData("() => this.UserName", "UserName")]
        [InlineData("() => UserName", "UserName")]
        [InlineData("()=>this.UserName", "UserName")]
        [InlineData("  () =>  vm.UserName  ", "UserName")]
        [InlineData("() => viewModel.UserName", "UserName")]
        [InlineData("() => this.Person.Name", null)]
        [InlineData("() => this.UserName?.Trim()", null)]
        [InlineData("() => GetName()", null)]
        [InlineData("x => x.Name", null)]
        [InlineData("", null)]
        [InlineData(null, null)]
        public void ShouldAcceptAPlainPropertyAccess_AndRejectTheRest(string? expression, string? expected)
        {
            // Act
            var parsed = PropertyNameParser.TryParse(expression, out var propertyName);

            // Assert
            parsed.Should().Be(expected != null);
            propertyName.Should().Be(expected);
        }

        [Fact]
        public void ShouldPointAtTheExplicitOverload_WhenRejecting()
        {
            // Act
            var act = () => PropertyNameParser.Parse("() => this.Person.Name");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*'() => this.Person.Name'*Property(nameof(this.UserName), () => this.UserName)*");
        }
    }
}
