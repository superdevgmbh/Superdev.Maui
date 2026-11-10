using Superdev.Maui.Navigation;

namespace Superdev.Maui.Tests.Navigation
{
    public class NavigationQueryParametersTests
    {
        public enum DetailMode
        {
            View,
            Edit
        }

        [Theory]
        [InlineData("Edit", DetailMode.Edit)]
        [InlineData("edit", DetailMode.Edit)]
        [InlineData("EDIT", DetailMode.Edit)]
        [InlineData("1", DetailMode.Edit)]
        [InlineData("View", DetailMode.View)]
        public void ShouldGetEnumValue(string rawValue, DetailMode expectedMode)
        {
            // Arrange
            var parameters = new NavigationQueryParameters(new Dictionary<string, string> { ["mode"] = rawValue });

            // Act
            var found = parameters.TryGetEnumValue<DetailMode>("mode", out var mode);

            // Assert
            found.Should().BeTrue();
            mode.Should().Be(expectedMode);
        }

        [Theory]
        [InlineData("Delete")]
        [InlineData("7")]
        [InlineData("")]
        public void ShouldNotGetEnumValue_WhenTheValueNamesNoMember(string rawValue)
        {
            // Arrange
            var parameters = new NavigationQueryParameters(new Dictionary<string, string> { ["mode"] = rawValue });

            // Act
            var found = parameters.TryGetEnumValue<DetailMode>("mode", out var mode);

            // Assert
            found.Should().BeFalse("a number the enumeration does not define is not a member either");
            mode.Should().Be(default(DetailMode));
        }

        [Fact]
        public void ShouldNotGetEnumValue_WhenTheParameterIsMissing()
        {
            // Arrange
            var parameters = new NavigationQueryParameters(new Dictionary<string, string>());

            // Act
            var found = parameters.TryGetEnumValue<DetailMode>("mode", out _);

            // Assert
            found.Should().BeFalse();
        }
    }
}
