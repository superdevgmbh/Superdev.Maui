using Microsoft.Extensions.Logging.Abstractions;
using Superdev.Maui.Resources.Styles;
using Superdev.Maui.Services;
using Application = Microsoft.Maui.Controls.Application;

namespace Superdev.Maui.Tests.Resources.Styles
{
    [Collection(ApplicationCollection.Name)]
    public class ThemeHelperTests : IDisposable
    {
        private const string ThemeName = "TestTheme";

        private readonly Application? previousApplication;
        private readonly Mock<ITheme> themeMock;

        public ThemeHelperTests()
        {
            this.previousApplication = Application.Current;

            var application = new Application();
            application.Resources.MergedDictionaries.Add(new SuperdevMauiStyles());
            this.themeMock = new Mock<ITheme>();
            application.Resources[ThemeName] = this.themeMock.Object;
            Application.Current = application;
        }

        public void Dispose()
        {
            Application.Current = this.previousApplication;
        }

        [Fact]
        public void AppTheme_WhileFollowingSystemTheme_AppliesThemeOnce()
        {
            // Arrange
            using var themeHelper = this.CreateThemeHelper();

            // Act
            themeHelper.AppTheme = AppTheme.Dark;

            // Assert
            this.themeMock.VerifyGet(t => t.ColorConfiguration, Times.Once(), "each theme application reads the theme's colours once");
        }

        [Fact]
        public void AppTheme_WhileFollowingSystemTheme_StopsFollowingIt()
        {
            // Arrange
            using var themeHelper = this.CreateThemeHelper();

            // Act
            themeHelper.AppTheme = AppTheme.Dark;

            // Assert
            themeHelper.UseSystemTheme.Should().BeFalse();
        }

        [Fact]
        public void ToggleTheme_WhileFollowingSystemTheme_AppliesThemeOnce()
        {
            // Arrange
            using var themeHelper = this.CreateThemeHelper();

            // Act
            themeHelper.ToggleTheme();

            // Assert
            this.themeMock.VerifyGet(t => t.ColorConfiguration, Times.Once(), "each theme application reads the theme's colours once");
        }

        [Fact]
        public void ToggleTheme_WhileFollowingSystemTheme_SwitchesToTheOtherThemeManually()
        {
            // Arrange
            using var themeHelper = this.CreateThemeHelper();
            themeHelper.AppTheme = AppTheme.Light;
            themeHelper.UseSystemTheme = true;

            // Act
            themeHelper.ToggleTheme();

            // Assert
            themeHelper.UseSystemTheme.Should().BeFalse();
            themeHelper.AppTheme.Should().Be(AppTheme.Dark, "the platform theme is not dark in a test run");
        }

        [Fact]
        public void AppTheme_Changed_RaisesThemeChangedOnceWithTheNewTheme()
        {
            // Arrange
            using var themeHelper = this.CreateThemeHelper();
            var raisedThemes = new List<AppTheme>();
            themeHelper.ThemeChanged += (_, appTheme) => raisedThemes.Add(appTheme);

            // Act
            themeHelper.AppTheme = AppTheme.Dark;

            // Assert
            raisedThemes.Should().Equal(AppTheme.Dark);
        }

        [Fact]
        public void ToggleTheme_Twice_RaisesThemeChangedForEachTheme()
        {
            // Arrange
            using var themeHelper = this.CreateThemeHelper();
            var raisedThemes = new List<AppTheme>();
            themeHelper.ThemeChanged += (_, appTheme) => raisedThemes.Add(appTheme);

            // Act
            themeHelper.ToggleTheme();
            themeHelper.ToggleTheme();

            // Assert
            raisedThemes.Should().Equal(AppTheme.Dark, AppTheme.Light);
        }

        [Fact]
        public void ApplyTheme_WhileFollowingSystemTheme_LeavesUserAppThemeUnspecified()
        {
            // Arrange
            using var appInfo = new AppInfoScope(AppTheme.Light);
            RaisePlatformThemeChanged();

            // Act
            using var themeHelper = this.CreateThemeHelper();

            // Assert
            Application.Current!.UserAppTheme.Should().Be(AppTheme.Unspecified, "a pinned user theme hides changes of the system theme");
        }

        [Fact]
        public void PlatformThemeChanged_WhileFollowingSystemTheme_AppliesTheNewTheme()
        {
            // Arrange
            using var appInfo = new AppInfoScope(AppTheme.Light);
            RaisePlatformThemeChanged();
            using var themeHelper = this.CreateThemeHelper();
            var raisedThemes = new List<AppTheme>();
            themeHelper.ThemeChanged += (_, appTheme) => raisedThemes.Add(appTheme);

            // Act
            appInfo.RequestedTheme = AppTheme.Dark;
            RaisePlatformThemeChanged();

            // Assert
            raisedThemes.Should().Equal(AppTheme.Dark);
        }

        [Fact]
        public void PlatformThemeChanged_WithAChosenTheme_KeepsTheChosenTheme()
        {
            // Arrange
            using var appInfo = new AppInfoScope(AppTheme.Light);
            RaisePlatformThemeChanged();
            using var themeHelper = this.CreateThemeHelper();
            themeHelper.AppTheme = AppTheme.Light;
            var raisedThemes = new List<AppTheme>();
            themeHelper.ThemeChanged += (_, appTheme) => raisedThemes.Add(appTheme);

            // Act
            appInfo.RequestedTheme = AppTheme.Dark;
            RaisePlatformThemeChanged();

            // Assert
            raisedThemes.Should().BeEmpty();
            Application.Current!.UserAppTheme.Should().Be(AppTheme.Light);
        }

        [Fact]
        public void UseSystemTheme_EnabledAfterAChosenTheme_UnpinsUserAppTheme()
        {
            // Arrange
            using var appInfo = new AppInfoScope(AppTheme.Light);
            RaisePlatformThemeChanged();
            using var themeHelper = this.CreateThemeHelper();
            themeHelper.AppTheme = AppTheme.Dark;

            // Act
            themeHelper.UseSystemTheme = true;

            // Assert
            Application.Current!.UserAppTheme.Should().Be(AppTheme.Unspecified);
        }

        [Fact]
        public void UseSystemTheme_EnabledAfterAChosenTheme_AppliesThemeOnce()
        {
            // Arrange
            using var appInfo = new AppInfoScope(AppTheme.Light);
            RaisePlatformThemeChanged();
            using var themeHelper = this.CreateThemeHelper();
            themeHelper.AppTheme = AppTheme.Dark;
            this.themeMock.Invocations.Clear();

            // Act
            themeHelper.UseSystemTheme = true;

            // Assert
            this.themeMock.VerifyGet(t => t.ColorConfiguration, Times.Once(), "unpinning the user theme raises RequestedThemeChanged, which must not apply the theme again");
        }

        [Fact]
        public void UseSystemTheme_DisabledWithoutAChosenTheme_KeepsTheShownTheme()
        {
            // Arrange
            using var appInfo = new AppInfoScope(AppTheme.Dark);
            RaisePlatformThemeChanged();
            using var themeHelper = this.CreateThemeHelper();

            // Act
            themeHelper.UseSystemTheme = false;

            // Assert
            themeHelper.AppTheme.Should().Be(AppTheme.Dark);
            Application.Current!.UserAppTheme.Should().Be(AppTheme.Dark);
        }

        /// <summary>
        /// Lets the application read the platform theme again, as MAUI does when the system theme changes.
        /// </summary>
        private static void RaisePlatformThemeChanged()
        {
            ((Microsoft.Maui.IApplication)Application.Current!).ThemeChanged();
        }

        /// <summary>
        /// Creates a helper which follows the system theme and has applied <see cref="ThemeName"/> once;
        /// the invocations of that first application are cleared.
        /// </summary>
        private ThemeHelper CreateThemeHelper()
        {
            var themeHelper = new ThemeHelper(NullLogger<ThemeHelper>.Instance, new InMemoryPreferences(), Mock.Of<IFontConverter>());
            themeHelper.ApplyTheme(ThemeName);
            this.themeMock.Invocations.Clear();
            return themeHelper;
        }
    }
}
