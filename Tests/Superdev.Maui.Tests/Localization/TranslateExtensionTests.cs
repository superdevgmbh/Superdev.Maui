using System.Globalization;
using Superdev.Maui.Converters;
using Superdev.Maui.Localization;

namespace Superdev.Maui.Tests.Localization
{
    public class TranslateExtensionTests
    {
        public TranslateExtensionTests()
        {
            var localizerMock = new Mock<ILocalizer>();
            localizerMock.Setup(l => l.CurrentCulture)
                .Returns(new CultureInfo("en"));

            var translationProviderMock = new Mock<ITranslationProvider>();
            translationProviderMock.Setup(t => t.Translate(It.IsAny<string>(), It.IsAny<CultureInfo>()))
                .Returns((string k, CultureInfo _) => k);

            TranslateExtension.Init(localizerMock.Object, translationProviderMock.Object);
        }

        [Fact]
        public void ShouldProvideValue_WithKey()
        {
            // Arrange
            var translateExtension = new TranslateExtension
            {
                Key = "Key"
            };

            // Act
            var value = translateExtension.ProvideValue(null);

            // Assert
            var multiBinding = value as MultiBinding;
            multiBinding.Should().NotBeNull();
            multiBinding.Bindings.Count.Should().Be(2);
            ((Binding)multiBinding.Bindings[0]).Converter.Should().BeNull();
            multiBinding.Converter.Should().NotBeNull();
        }

        [Fact]
        public void ShouldProvideValue_WithKeyAndConverter()
        {
            // Arrange
            var translateExtension = new TranslateExtension
            {
                Key = "Key",
                Converter = new DebugConverter()
            };

            // Act
            var value = translateExtension.ProvideValue(null);

            // Assert
            var multiBinding = value as MultiBinding;
            multiBinding.Should().NotBeNull();
            multiBinding.Bindings.Count.Should().Be(2);

            var converter0 = ((Binding)multiBinding.Bindings[0]).Converter;
            converter0.Should().NotBeNull();
            converter0.Should().BeOfType(translateExtension.Converter.GetType());
            multiBinding.Converter.Should().NotBeNull();
        }
    }
}