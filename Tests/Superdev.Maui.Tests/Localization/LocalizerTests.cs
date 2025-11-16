using System.Globalization;
using Superdev.Maui.Localization;
using Superdev.Maui.Services;
using IPreferences = Superdev.Maui.Services.IPreferences;

namespace Superdev.Maui.Tests.Localization
{
    public class LocalizerTests
    {
        private const string AppSettingsLanguageKey = "AppSettingsLanguage";

        private readonly IMainThread mainThread;

        public LocalizerTests()
        {
            this.mainThread = new StaticMainThread();
        }

        [Fact]
        public void ShouldGetCurrentCultureInfo_FromAppSettings()
        {
            // Arrange
            const string platformLocale = "en-US";
            var expectedCultureInfo = SupportedLanguages.English;

            var preferencesMock = new Mock<IPreferences>();
            preferencesMock.Setup(p => p.Get<string>(AppSettingsLanguageKey, null, null, null))
                .Returns(expectedCultureInfo.Name);

            ILocalizer localizer = new TestLocalizer(platformLocale, preferencesMock.Object, this.mainThread);

            // Act
            var currentCulture = localizer.CurrentCulture;

            // Assert
            currentCulture.Should().Be(expectedCultureInfo);

            preferencesMock.Verify(p => p.Get<string>(AppSettingsLanguageKey, null, null, null), Times.Once);
            preferencesMock.VerifyNoOtherCalls();
        }

        [Theory]
        [ClassData(typeof(PlatformLocaleCultureInfoMappingTestData))]
        public void ShouldGetCurrentCultureInfo_FromPlatformCultureInfo(string platformLocale, CultureInfo expectedCultureInfo)
        {
            // Arrange
            var preferencesMock = new Mock<IPreferences>();

            ILocalizer localizer = new TestLocalizer(platformLocale, preferencesMock.Object, this.mainThread);

            // Act
            var currentCulture = localizer.CurrentCulture;

            // Assert
            currentCulture.Should().Be(expectedCultureInfo);

            preferencesMock.Verify(p => p.Get<string>(AppSettingsLanguageKey, null, null, null), Times.Once);
            preferencesMock.VerifyNoOtherCalls();
        }

        public class PlatformLocaleCultureInfoMappingTestData : TheoryData<string, CultureInfo>
        {
            public PlatformLocaleCultureInfoMappingTestData()
            {
                // Typical .NET cultures
                this.Add("en", SupportedLanguages.English);
                this.Add("en-gb", SupportedLanguages.English);
                this.Add("en-GB", SupportedLanguages.English);
                this.Add("en-US", SupportedLanguages.English);
                this.Add("fr-FR", SupportedLanguages.French);
                this.Add("fr-CH", SupportedLanguages.French);
                this.Add("it-IT", SupportedLanguages.Italian);
                this.Add("it-CH", SupportedLanguages.Italian);

                // Typical Android platform locales
                this.Add("en_GB", SupportedLanguages.English);
                this.Add("fr_FR", SupportedLanguages.French);
                this.Add("fr_CH", SupportedLanguages.French);
                this.Add("it_IT", SupportedLanguages.Italian);
                this.Add("pl_PL", SupportedLanguages.English);
                this.Add("nl_NL", SupportedLanguages.English);
                this.Add("es_ES", SupportedLanguages.English);
                this.Add("ro_RO", SupportedLanguages.English);
                this.Add("ru_RU", SupportedLanguages.English);
                this.Add("ru_UA", SupportedLanguages.English);
                this.Add("pt_BR", SupportedLanguages.English);

                // Typical iOS platform locales
                this.Add("gsw-CH", SupportedLanguages.German);
                this.Add("de-FR", SupportedLanguages.German);
                this.Add("en-SG", SupportedLanguages.English);
                this.Add("pl-PL", SupportedLanguages.English);
                this.Add("en-CH", SupportedLanguages.English);

                // Invalid languages or cultures
                this.Add("fr-XX", SupportedLanguages.French);
                this.Add("fr_XX", SupportedLanguages.French);
                this.Add("de-", SupportedLanguages.German);
                this.Add("de_", SupportedLanguages.German);
                this.Add("XX", SupportedLanguages.English);
                this.Add("xx-ZZ", SupportedLanguages.English);
            }
        }
    }

    public class TestLocalizer : Localizer
    {
        private readonly string platformLocale;

        public TestLocalizer(string platformLocale, IPreferences preferences, IMainThread mainThread)
            : base(preferences, mainThread)
        {
            this.platformLocale = platformLocale;
        }

        public override string GetPlatformLocale()
        {
            return this.platformLocale;
        }
    }
}