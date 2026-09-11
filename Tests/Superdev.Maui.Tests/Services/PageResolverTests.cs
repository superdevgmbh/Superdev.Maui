using Superdev.Maui.Navigation;
using Superdev.Maui.Services;
using Superdev.Maui.Tests.Extensions;
using Xunit.Abstractions;

namespace Superdev.Maui.Tests.Services
{
    public class PageResolverTests
    {
        private readonly AutoMocker autoMocker;
        private readonly Mock<IKeyedServiceProvider> serviceProviderMock;

        public PageResolverTests(ITestOutputHelper testOutputHelper)
        {
            this.autoMocker = new AutoMocker();
            this.autoMocker.UseTestOutputHelperLogger<KeyboardService>(testOutputHelper);

            this.serviceProviderMock = new Mock<IKeyedServiceProvider>();
            this.autoMocker.Use<IServiceProvider>(this.serviceProviderMock.Object);
            this.serviceProviderMock.Setup(s => s.GetService(It.IsAny<Type>()))
                .Returns<Type>(Activator.CreateInstance);
        }

        [Fact]
        public void ShouldResolvePage_AutoResolve_WithoutViewModel_Success()
        {
            // Arrange
            IPageResolver pageResolver = this.autoMocker.CreateInstance<PageResolver>(true);

            // Act
            var page = pageResolver.ResolvePage("TestResolveWPage");

            // Assert
            page.Should().BeOfType<TestResolveWPage>();
            page.BindingContext.Should().BeNull();
        }

        [Fact]
        public void ShouldResolvePage_AutoResolve_WithViewModel_Success()
        {
            // Arrange
            IPageResolver pageResolver = this.autoMocker.CreateInstance<PageResolver>(true);

            // Act
            var page = pageResolver.ResolvePage("TestResolvePage");

            // Assert
            page.Should().BeOfType<TestResolvePage>();
            page.BindingContext.Should().BeOfType<TestResolveViewModel>();
        }

        [Fact]
        public void ShouldResolvePage_FromRegistration_WithViewModel()
        {
            // Arrange
            this.serviceProviderMock.Setup(s => s.GetKeyedService(It.Is<Type>(t => t == typeof(PageRegistration)), It.IsAny<object?>()))
                .Returns((Type _, string key) => new PageRegistration
                {
                    PageType = typeof(TestPopupPage),
                    ViewModelType = typeof(TestPopupViewModel),
                    Name = "TestPopup"
                });

            IPageResolver pageResolver = this.autoMocker.CreateInstance<PageResolver>(true);

            // Act
            var page = pageResolver.ResolvePage("TestPopup");

            // Assert
            page.Should().BeOfType<TestPopupPage>();
            page.BindingContext.Should().BeOfType<TestPopupViewModel>();
        }

        [Fact]
        public void ShouldResolvePage_ThrowsPageResolveException_PageNotFound()
        {
            // Arrange
            IPageResolver pageResolver = this.autoMocker.CreateInstance<PageResolver>(true);

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

        public class TestPopupPage : ContentPage
        {
        }

        public class TestPopupViewModel : BindableObject
        {
        }
    }
}