using System.Globalization;
using Superdev.Maui.Localization;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    public class DefaultValidationMessageProviderTests
    {
        [Fact]
        public void ShouldFillTheBuiltInTemplate_FromTheArguments()
        {
            // Arrange
            var provider = new DefaultValidationMessageProvider();

            // Act
            var message = provider.GetMessage(ValidationErrorCodes.MinimumLength, new Dictionary<string, object?>
            {
                ["PropertyName"] = "User Name",
                ["MinimumLength"] = 3,
            });

            // Assert
            message.Should().Be("User Name must be at least 3 characters long.");
        }

        [Fact]
        public void ShouldPreferATranslation_ByPrefixedCode()
        {
            // Arrange
            var translations = new Mock<ITranslationProvider>();
            translations.Setup(t => t.Translate("Validation_NotEmpty", It.IsAny<CultureInfo?>())).Returns("{PropertyName} bitte ausfüllen.");
            var provider = new DefaultValidationMessageProvider(translations.Object);

            // Act
            var message = provider.GetMessage(ValidationErrorCodes.NotEmpty, new Dictionary<string, object?> { ["PropertyName"] = "Name" });

            // Assert
            message.Should().Be("Name bitte ausfüllen.");
        }

        [Theory]
        [InlineData("#Validation_NotEmpty#")]
        [InlineData("Validation_NotEmpty")]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldFallBackToTheBuiltInTemplate_WhenTheKeyIsNotTranslated(string? untranslated)
        {
            // Arrange
            var translations = new Mock<ITranslationProvider>();
            translations.Setup(t => t.Translate(It.IsAny<string>(), It.IsAny<CultureInfo?>())).Returns(untranslated!);
            var provider = new DefaultValidationMessageProvider(translations.Object);

            // Act
            var message = provider.GetMessage(ValidationErrorCodes.NotEmpty, new Dictionary<string, object?> { ["PropertyName"] = "Name" });

            // Assert
            message.Should().Be("Name must not be empty.");
        }

        [Fact]
        public void ShouldUseTheGenericTemplate_ForAnUnknownCode()
        {
            var provider = new DefaultValidationMessageProvider();

            provider.GetMessage("SwissPlate", new Dictionary<string, object?> { ["PropertyName"] = "Plate" })
                .Should().Be("Plate is not valid.");
        }

        [Fact]
        public void ShouldAcceptACustomKeyPrefix()
        {
            // Arrange
            var translations = new Mock<ITranslationProvider>();
            translations.Setup(t => t.Translate("Rule.EmailAddress", It.IsAny<CultureInfo?>())).Returns("Bad e-mail.");
            var provider = new DefaultValidationMessageProvider(translations.Object, "Rule.");

            // Act & Assert
            provider.GetMessage(ValidationErrorCodes.EmailAddress, ValidationError.EmptyArguments).Should().Be("Bad e-mail.");
        }

        [Fact]
        public void ShouldTranslateADisplayName_UnderItsPrefixedKey()
        {
            // Arrange
            var translations = new Mock<ITranslationProvider>();
            translations.Setup(t => t.Translate(It.IsAny<string>(), It.IsAny<CultureInfo?>())).Returns<string, CultureInfo?>((key, _) => $"#{key}#");
            translations.Setup(t => t.Translate("ValidationField_PostalCode", It.IsAny<CultureInfo?>())).Returns("Postleitzahl");
            var provider = new DefaultValidationMessageProvider(translations.Object);

            // Act & Assert
            provider.GetDisplayName("PostalCode").Should().Be("Postleitzahl");
            provider.GetDisplayName("Street").Should().BeNull("an untranslated name falls back to its words in the form");
        }

        [Fact]
        public void ShouldHaveNoDisplayName_WithoutATranslationProvider()
        {
            // Arrange
            var provider = new DefaultValidationMessageProvider();

            // Act
            var displayName = provider.GetDisplayName("PostalCode");

            // Assert
            displayName.Should().BeNull();
        }
    }
}
