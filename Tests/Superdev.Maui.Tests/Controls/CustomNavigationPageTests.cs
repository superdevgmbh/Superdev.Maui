using Microsoft.Extensions.DependencyInjection;
using Superdev.Maui.Controls;
using Superdev.Maui.Navigation.Hosts;

namespace Superdev.Maui.Tests.Controls
{
    [Collection(ApplicationCollection.Name)]
    public class CustomNavigationPageTests : IDisposable
    {
        private readonly IPlatformApplication? previousPlatformApplication;
        private readonly Mock<INavigationHost> navigationHostMock = new Mock<INavigationHost>();

        public CustomNavigationPageTests()
        {
            MauiMocks.Init();
            this.previousPlatformApplication = IPlatformApplication.Current;
        }

        public void Dispose()
        {
            IPlatformApplication.Current = this.previousPlatformApplication;
        }

        [Fact]
        public async Task OnBackButtonPressed_NavigationHostTakesOver_DoesNotPop()
        {
            // Arrange
            this.UsePlatformApplication(withNavigationHost: true);
            this.navigationHostMock.Setup(h => h.RequestBack(It.IsAny<Page>())).Returns(true);
            var pushedPage = new ContentPage();
            var navigationPage = new CustomNavigationPage(new ContentPage());
            await navigationPage.PushAsync(pushedPage);

            // Act
            var handled = navigationPage.SendBackButtonPressed();

            // Assert
            handled.Should().BeTrue("the navigation service performs the pop itself, so the platform must not");
            navigationPage.CurrentPage.Should().BeSameAs(pushedPage);
            this.navigationHostMock.Verify(h => h.RequestBack(pushedPage), Times.Once);
        }

        [Fact]
        public void OnBackButtonPressed_WithNavigationHost_AsksWithTheCurrentPage()
        {
            // Arrange
            this.UsePlatformApplication(withNavigationHost: true);
            var rootPage = new ContentPage();
            var navigationPage = new CustomNavigationPage(rootPage);

            // Act
            navigationPage.SendBackButtonPressed();

            // Assert
            this.navigationHostMock.Verify(h => h.RequestBack(rootPage), Times.Once);
            this.navigationHostMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void OnBackButtonPressed_NavigationHostDoesNotHandleOnRootPage_ReturnsFalse()
        {
            // Arrange
            this.UsePlatformApplication(withNavigationHost: true);
            this.navigationHostMock.Setup(h => h.RequestBack(It.IsAny<Page>())).Returns(false);
            var navigationPage = new CustomNavigationPage(new ContentPage());

            // Act
            var handled = navigationPage.SendBackButtonPressed();

            // Assert
            handled.Should().BeFalse("a root page without a guard leaves the back press to the platform, which backgrounds the app");
        }

        [Fact]
        public async Task OnBackButtonPressed_NavigationHostDoesNotHandleOnPushedPage_PopsThePage()
        {
            // Arrange
            this.UsePlatformApplication(withNavigationHost: true);
            this.navigationHostMock.Setup(h => h.RequestBack(It.IsAny<Page>())).Returns(false);
            var rootPage = new ContentPage();
            var navigationPage = new CustomNavigationPage(rootPage);
            await navigationPage.PushAsync(new ContentPage());

            // Act
            var handled = navigationPage.SendBackButtonPressed();

            // Assert
            handled.Should().BeTrue("the page pops itself when the navigation service leaves the back press to it");
            navigationPage.CurrentPage.Should().BeSameAs(rootPage);
        }

        [Fact]
        public void OnBackButtonPressed_WithoutNavigationHost_LeavesTheBackPressToThePage()
        {
            // Arrange
            this.UsePlatformApplication(withNavigationHost: false);
            var navigationPage = new CustomNavigationPage(new ContentPage());

            // Act
            var handled = navigationPage.SendBackButtonPressed();

            // Assert
            handled.Should().BeFalse();
            this.navigationHostMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void OnBackButtonPressed_WithoutPlatformApplication_LeavesTheBackPressToThePage()
        {
            // Arrange
            IPlatformApplication.Current = null;
            var navigationPage = new CustomNavigationPage(new ContentPage());

            // Act
            var handled = navigationPage.SendBackButtonPressed();

            // Assert
            handled.Should().BeFalse();
        }

        private void UsePlatformApplication(bool withNavigationHost)
        {
            var services = new ServiceCollection();
            if (withNavigationHost)
            {
                services.AddSingleton(this.navigationHostMock.Object);
            }

            var platformApplicationMock = new Mock<IPlatformApplication>();
            platformApplicationMock.SetupGet(p => p.Services).Returns(services.BuildServiceProvider());
            IPlatformApplication.Current = platformApplicationMock.Object;
        }
    }
}
