using System.Globalization;
using Superdev.Maui.Localization;
using Superdev.Maui.Services;
using IPreferences = Superdev.Maui.Services.IPreferences;

namespace Superdev.Maui.Tests.Localization
{
    [Collection(ProcessCultureCollection.Name)]
    public class LocalizerTests : IDisposable
    {
        private readonly IMainThread mainThread;
        private readonly CultureInfo previousCulture = CultureInfo.CurrentCulture;
        private readonly CultureInfo previousUICulture = CultureInfo.CurrentUICulture;
        private readonly CultureInfo? previousDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        private readonly CultureInfo? previousDefaultUICulture = CultureInfo.DefaultThreadCurrentUICulture;

        public LocalizerTests()
        {
            this.mainThread = new StaticMainThread();
        }

        public void Dispose()
        {
            // The localizer applies the language it resolves to the whole process.
            CultureInfo.CurrentCulture = this.previousCulture;
            CultureInfo.CurrentUICulture = this.previousUICulture;
            CultureInfo.DefaultThreadCurrentCulture = this.previousDefaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = this.previousDefaultUICulture;
        }

        [Fact]
        public void CurrentCulture_NoPlatformLocaleAndNoSupportedLanguages_UsesTheUICulture()
        {
            // Arrange
            CultureInfo.CurrentCulture = new CultureInfo("fr-CH");
            CultureInfo.CurrentUICulture = new CultureInfo("de-CH");
            var preferencesMock = new Mock<IPreferences>();

            // Act
            var localizer = new TestLocalizer(preferencesMock.Object, this.mainThread)
            {
                PlatformLocale = null
            };

            // Assert
            localizer.CurrentCulture.Name.Should().Be("de-CH", "resources are looked up in the UI culture, formats in the culture");
        }

        [Fact]
        public void CurrentCulture_NoPlatformLocaleWithSupportedLanguages_UsesTheFirstSupportedLanguage()
        {
            // Arrange
            var preferencesMock = new Mock<IPreferences>();

            // Act
            var localizer = new TestLocalizer(preferencesMock.Object, this.mainThread)
            {
                PlatformLocale = null,
                SupportedLanguages = [SupportedLanguages.German, SupportedLanguages.English]
            };

            // Assert
            localizer.CurrentCulture.Should().Be(SupportedLanguages.German);
        }

        [Fact]
        public void CurrentCulture_NoPlatformLocaleWithDefaultLanguage_UsesTheDefaultLanguage()
        {
            // Arrange
            var preferencesMock = new Mock<IPreferences>();

            // Act
            var localizer = new TestLocalizer(preferencesMock.Object, this.mainThread)
            {
                PlatformLocale = null,
                SupportedLanguages = [SupportedLanguages.German, SupportedLanguages.English],
                DefaultLanguage = SupportedLanguages.English
            };

            // Assert
            localizer.CurrentCulture.Should().Be(SupportedLanguages.English);
        }

        [Fact]
        public void CurrentCulture_SetToInvariantWithoutSupportedLanguages_KeepsTheUICulture()
        {
            // Arrange
            var preferencesMock = new Mock<IPreferences>();
            var localizer = new TestLocalizer(preferencesMock.Object, this.mainThread)
            {
                PlatformLocale = "it-CH"
            };

            // Act
            localizer.CurrentCulture = CultureInfo.InvariantCulture;

            // Assert
            localizer.CurrentCulture.Name.Should().Be("it-CH", "the language in use is the only one left to fall back to");
        }

        [Fact]
        public void ShouldGetCurrentCultureInfo_FromAppSettings()
        {
            // Arrange
            const string platformLocale = "en-US";
            var expectedCultureInfo = SupportedLanguages.English;

            var preferencesMock = new Mock<IPreferences>();
            preferencesMock.Setup(p => p.Get<string>(Localizer.DefaultPreferencesKey, null))
                .Returns(() => expectedCultureInfo.Name);

            ILocalizer localizer = new TestLocalizer(preferencesMock.Object, this.mainThread)
            {
                PlatformLocale = platformLocale
            };

            // Act
            var currentCulture = localizer.CurrentCulture;

            // Assert
            currentCulture.Should().Be(expectedCultureInfo);

            preferencesMock.Verify(p => p.Get<string>(Localizer.DefaultPreferencesKey, null), Times.Once);
            preferencesMock.VerifyNoOtherCalls();
        }

        [Theory]
        [ClassData(typeof(PlatformLocaleCultureInfoMappingTestData))]
        public void ShouldGetCurrentCultureInfo_FromPlatformCultureInfo(string platformLocale, CultureInfo expectedCultureInfo)
        {
            // Arrange
            var preferencesMock = new Mock<IPreferences>();

            var localizer = new TestLocalizer(preferencesMock.Object, this.mainThread)
            {
                PlatformLocale = platformLocale,
                SupportedLanguages = SupportedLanguages.GetAll().ToArray()
            };

            // Act
            var currentCulture = localizer.CurrentCulture;

            // Assert
            currentCulture.Should().Be(expectedCultureInfo);

            preferencesMock.Verify(p => p.Get<string>(Localizer.DefaultPreferencesKey, null), Times.Exactly(2));
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
        private readonly string? platformLocale;

        public TestLocalizer(IPreferences preferences, IMainThread mainThread)
            : base(preferences, mainThread, initialize: false)
        {
        }

        public string? PlatformLocale
        {
            get => this.platformLocale;
            init
            {
                this.platformLocale = value;
                this.InitializeFromPreferencesOrSystem();
            }
        }

        public override string GetPlatformLocale()
        {
            return this.PlatformLocale;
        }
    }
}