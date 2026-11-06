using Microsoft.Extensions.DependencyInjection;
using Superdev.Maui.Navigation;

namespace Superdev.Maui.Tests.Navigation
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void ShouldRejectATypeWhichIsNotAPage()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act - a view is a visual element, but nothing can navigate to it
            Action action = () => services.RegisterForNavigation(typeof(ContentView));

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithParameterName("pageType")
                .WithMessage("*not a page*", "the registration used to compile and fail on the first navigation instead");
        }

        [Fact]
        public void ShouldRegisterAPage_UnderItsTypeName()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.RegisterForNavigation(typeof(ContentPage));

            // Assert
            services.Should().Contain(d =>
                d.ServiceType == typeof(PageRegistration) && Equals(d.ServiceKey, nameof(ContentPage)));
        }
    }
}
