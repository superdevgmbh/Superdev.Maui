using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Superdev.Maui.Services;
using Superdev.Maui.Tests.Extensions;
using Xunit.Abstractions;
using Application = Microsoft.Maui.Controls.Application;
using PlatformConfigurationAndroid = Microsoft.Maui.Controls.PlatformConfiguration.Android;

namespace Superdev.Maui.Tests.Services
{
    public class KeyboardServiceTests
    {
        private readonly AutoMocker autoMocker;

        public KeyboardServiceTests(ITestOutputHelper testOutputHelper)
        {
            this.autoMocker = new AutoMocker();
            this.autoMocker.UseTestOutputHelperLogger<KeyboardService>(testOutputHelper);
        }

        [Fact]
        public void ShouldUseWindowSoftInputModeAdjust()
        {
            // Arrange
            var logger = this.autoMocker.Get<ILogger<KeyboardService>>();

            var application = new Application();
            var platformElementConfigurationMock = this.autoMocker.GetMock<IPlatformElementConfiguration<PlatformConfigurationAndroid, Application>>();
            platformElementConfigurationMock.Setup(c => c.Element)
                .Returns(application);

            IKeyboardService keyboardService = new KeyboardService(logger, platformElementConfigurationMock.Object);

            // Act
            keyboardService.UseWindowSoftInputModeAdjust("page1", WindowSoftInputModeAdjust.Resize);

            // Assert
            var windowSoftInputModeAdjust = application.On<PlatformConfigurationAndroid>().GetWindowSoftInputModeAdjust();
            windowSoftInputModeAdjust.Should().Be(WindowSoftInputModeAdjust.Resize);
        }

        [Fact]
        public void ShouldUseAndResetWindowSoftInputModeAdjust_SinglePage()
        {
            // Arrange
            var logger = this.autoMocker.Get<ILogger<KeyboardService>>();

            var application = new Application();
            var platformElementConfigurationMock = this.autoMocker.GetMock<IPlatformElementConfiguration<PlatformConfigurationAndroid, Application>>();
            platformElementConfigurationMock.Setup(c => c.Element)
                .Returns(application);

            IKeyboardService keyboardService = new KeyboardService(logger, platformElementConfigurationMock.Object);

            // Act
            keyboardService.UseWindowSoftInputModeAdjust("page1", WindowSoftInputModeAdjust.Resize);
            keyboardService.ResetWindowSoftInputModeAdjust("page1");

            // Assert
            var windowSoftInputModeAdjust = application.On<PlatformConfigurationAndroid>().GetWindowSoftInputModeAdjust();
            windowSoftInputModeAdjust.Should().Be(WindowSoftInputModeAdjust.Pan);
        }

        [Fact]
        public void ShouldUseAndResetWindowSoftInputModeAdjust_MultiplePages()
        {
            // Arrange
            var logger = this.autoMocker.Get<ILogger<KeyboardService>>();

            var application = new Application();
            var platformElementConfigurationMock = this.autoMocker.GetMock<IPlatformElementConfiguration<PlatformConfigurationAndroid, Application>>();
            platformElementConfigurationMock.Setup(c => c.Element)
                .Returns(application);

            IKeyboardService keyboardService = new KeyboardService(logger, platformElementConfigurationMock.Object);

            // Act
            keyboardService.UseWindowSoftInputModeAdjust("page1", WindowSoftInputModeAdjust.Resize);
            keyboardService.UseWindowSoftInputModeAdjust("page2", WindowSoftInputModeAdjust.Resize);
            keyboardService.ResetWindowSoftInputModeAdjust("page1");
            keyboardService.ResetWindowSoftInputModeAdjust("page2");

            // Assert
            var windowSoftInputModeAdjust = application.On<PlatformConfigurationAndroid>().GetWindowSoftInputModeAdjust();
            windowSoftInputModeAdjust.Should().Be(WindowSoftInputModeAdjust.Pan);
        }

        [Fact]
        public void ShouldResetUnregisteredPage()
        {
            // Arrange
            var logger = this.autoMocker.Get<ILogger<KeyboardService>>();

            var application = new Application();
            var platformElementConfigurationMock = this.autoMocker.GetMock<IPlatformElementConfiguration<PlatformConfigurationAndroid, Application>>();
            platformElementConfigurationMock.Setup(c => c.Element)
                .Returns(application);

            IKeyboardService keyboardService = new KeyboardService(logger, platformElementConfigurationMock.Object);

            // Act
            keyboardService.ResetWindowSoftInputModeAdjust("page99");

            // Assert
            var windowSoftInputModeAdjust = application.On<PlatformConfigurationAndroid>().GetWindowSoftInputModeAdjust();
            windowSoftInputModeAdjust.Should().Be(WindowSoftInputModeAdjust.Pan);
        }
    }
}