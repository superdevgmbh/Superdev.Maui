using Microsoft.Extensions.DependencyInjection;
using Superdev.Maui.Navigation;
using Superdev.Maui.Tests.Navigation.TestDoubles;

namespace Superdev.Maui.Tests.Navigation
{
    /// <summary>
    /// Locks in the defects which the comparison against the reference libraries surfaced.
    /// </summary>
    public class NavigationServiceRegressionTests
    {
        [Fact]
        public async Task ShouldDeliverQueryParametersPerSegment()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.NavigateAsync("DeepLinkPage?id=1/DeepLinkChildPage?id=2");

            // Assert
            fixture.NavigationService.NavigationStack.Should().HaveCount(
                3, "a query string must not swallow the segment which follows it");

            var childViewModel = (DeepLinkChildViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            childViewModel.ReceivedParameters!["id"].Should().Be("2");

            var parentPage = fixture.NavigationService.NavigationStack[1];
            var parentViewModel = (DeepLinkViewModel)parentPage.BindingContext;
            parentViewModel.ReceivedParameters!["id"].Should().Be("1", "every segment receives its own parameters");
        }

        [Fact]
        public async Task ShouldThrow_WhenTheLastSegmentCombinesAQueryStringAndAParameter()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            Func<Task> action = () => fixture.NavigationService.NavigateAsync(
                "DeepLinkPage?id=1", new TestParameter { Value = "x" });

            // Assert
            await action.Should().ThrowAsync<NavigationException>().WithMessage("*not both*");
        }

        [Fact]
        public async Task ShouldAllowAQueryStringOnAnIntermediateSegmentTogetherWithAParameter()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.NavigateAsync(
                "DeepLinkPage?id=1/DetailPage", new TestParameter { Value = "detail" });

            // Assert
            var detailViewModel = (DetailViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            detailViewModel.ReceivedParameter!.Value.Should().Be("detail");
        }

        [Fact]
        public async Task ShouldThrow_WhenANavigationPageSegmentCannotBeApplied()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            Func<Task> action = () => fixture.NavigationService.NavigateAsync("NavigationPage/DetailPage");

            // Assert
            await action.Should().ThrowAsync<NavigationException>()
                .WithMessage("*absolute navigation URI*");

            fixture.NavigationService.NavigationStack.Should().HaveCount(
                1, "a childless navigation page must not be pushed onto the stack");
        }

        [Fact]
        public async Task ShouldRouteHardwareBackThroughTheNavigationService()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("DetailPage");

            var detailPage = fixture.NavigationService.CurrentPage!;
            var detailViewModel = (DetailViewModel)detailPage.BindingContext;
            fixture.CallLog.Entries.Clear();

            // Act
            var handled = fixture.Host.RequestBack(detailPage);

            // Assert
            handled.Should().BeTrue("the navigation service performs the pop, so the platform must not");

            fixture.CallLog.Entries.Should().ContainInOrder(
                "DetailViewModel.NavigatedFromAsync(Back)",
                "HomeViewModel.NavigatedToAsync(Back)");

            detailViewModel.IsDisposed.Should().BeTrue();
            fixture.NavigationService.NavigationStack.Should().HaveCount(1);
        }

        [Fact]
        public async Task ShouldNotPopOnHardwareBack_WhenNavigationGuardVetoes()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("GuardedPage");

            var guardedPage = fixture.NavigationService.CurrentPage!;
            ((GuardedViewModel)guardedPage.BindingContext).CanNavigate = false;

            // Act
            var handled = fixture.Host.RequestBack(guardedPage);

            // Assert
            handled.Should().BeTrue();
            fixture.NavigationService.NavigationStack.Should().HaveCount(
                2, "the guard vetoed the back navigation");
        }

        [Fact]
        public async Task ShouldNotClosePopup_WhenItIsNotTheTopmostPage()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("ContextMenuPopupPage");
            var firstPopup = (ContextMenuPopupPage)fixture.NavigationService.CurrentPage!;

            await fixture.NavigationService.PushModalAsync("DetailPage", wrapInNavigationPage: false);

            // Act - a covered popup must never close the page which is on top of it
            firstPopup.SimulateBackgroundClick();

            // Assert
            fixture.NavigationService.ModalStack.Should().HaveCount(2);
            fixture.NavigationService.CurrentPage.Should().BeOfType<DetailPage>();
        }

        [Fact]
        public async Task ShouldCloseTheTopmostPopup_WhenItsBackgroundIsClicked()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("ContextMenuPopupPage");
            await fixture.NavigationService.PushAsync("ContextMenuPopupPage");

            var topPopup = (ContextMenuPopupPage)fixture.NavigationService.CurrentPage!;

            // Act
            topPopup.SimulateBackgroundClick();

            // Assert
            fixture.NavigationService.ModalStack.Should().HaveCount(1, "only the tapped popup closes");
            topPopup.ScopedService.IsDisposed.Should().BeTrue();
        }

        [Fact]
        public void ShouldNotResolvePagesByConvention_WhenTheTypeIsNotAPage()
        {
            // Arrange - convention resolution is enabled, and a non-page type shares the name
            using var fixture = NavigationServiceFixture.Create(conventionBasedResolution: true);

            // Act
            Action action = () => fixture.NavigationRegistry.GetRegistration(nameof(NotAPage));

            // Assert
            action.Should().Throw<PageResolveException>().WithMessage("*not registered*");
        }

        [Fact]
        public async Task ShouldUnsubscribeFromTheHost_WhenDisposed()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("LifecycleAwarePage");
            fixture.CallLog.Entries.Clear();

            // Act
            fixture.NavigationService.Dispose();
            fixture.Host.RaiseApplicationLifecycle(resumed: true);

            // Assert
            fixture.CallLog.Entries.Should().BeEmpty("a disposed navigation service must not react to host events");
        }
    }

    /// <summary>
    /// Shares its name shape with a page, but is not one.
    /// </summary>
    internal sealed class NotAPage
    {
    }
}
