using System.Globalization;
using Superdev.Maui.Converters;

namespace Superdev.Maui.Tests.Converters
{
    public class PropertyErrorsToColorConverterTests
    {
        [Fact]
        public void ShouldFallBack_WithoutTheThemeResources()
        {
            // Arrange - no application, as in a test or the previewer: the lookup must not throw inside a binding
            var converter = new PropertyErrorsToColorConverter();

            // Act
            var withErrors = converter.Convert(new[] { "Required." }, typeof(Color), null, CultureInfo.InvariantCulture);
            var withoutErrors = converter.Convert(Array.Empty<string>(), typeof(Color), null, CultureInfo.InvariantCulture);

            // Assert
            withErrors.Should().Be(Colors.Red);
            withoutErrors.Should().BeNull("the control keeps its platform colour");
        }
    }
}
