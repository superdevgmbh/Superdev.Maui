using Superdev.Maui.Navigation;
using Superdev.Maui.Services;
using Superdev.Maui.Tests.Extensions;
using Xunit.Abstractions;

namespace Superdev.Maui.Tests.Services
{
    public class PageResolverTests
    {
        private readonly AutoMocker autoMocker;

        public PageResolverTests(ITestOutputHelper testOutputHelper)
        {
            this.autoMocker = new AutoMocker();
            this.autoMocker.UseTestOutputHelperLogger<KeyboardService>(testOutputHelper);

            var serviceProviderMock = this.autoMocker.GetMock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetService(It.IsAny<Type>()))
                .Returns<Type>(Activator.CreateInstance);
        }

        [Fact]
        public void ShouldResolvePageWithoutViewModel_Success()
        {
            // Arrange
            IPageResolver pageResolver = this.autoMocker.CreateInstance<PageResolver>();

            // Act
            var page = pageResolver.ResolvePage("TestResolveWPage");

            // Assert
            page.Should().BeOfType<TestResolveWPage>();
            page.BindingContext.Should().BeNull();
        }

        [Fact]
        public void ShouldResolvePageWithViewModel_Success()
        {
            // Arrange
            IPageResolver pageResolver = this.autoMocker.CreateInstance<PageResolver>();

            // Act
            var page = pageResolver.ResolvePage("TestResolvePage");

            // Assert
            page.Should().BeOfType<TestResolvePage>();
            page.BindingContext.Should().BeOfType<TestResolveViewModel>();
        }

        [Fact]
        public void ShouldResolvePage_ThrowsPageResolveException_PageNotFound()
        {
            // Arrange
            IPageResolver pageResolver = this.autoMocker.CreateInstance<PageResolver>();

            // Act
            Action action = () => pageResolver.ResolvePage("NonExistingPage");

            // Assert
            var pageResolveException = action.Should().Throw<PageResolveException>().Which;
            pageResolveException.Message.Should().Be("Page with name 'NonExistingPage' not found");
        }

        public class TestResolveWPage : ContentPage
        {
        }

        public class TestResolvePage : ContentPage
        {
        }

        public class TestResolveViewModel : BindableObject
        {
        }
    }
}