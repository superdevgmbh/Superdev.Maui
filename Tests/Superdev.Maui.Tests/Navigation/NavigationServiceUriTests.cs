using Superdev.Maui.Navigation;
using Superdev.Maui.Tests.Navigation.TestDoubles;

namespace Superdev.Maui.Tests.Navigation
{
    /// <summary>
    /// Covers the navigation URI grammar: relative removal segments, deep links and query parameters.
    /// </summary>
    public class NavigationServiceUriTests
    {
        [Fact]
        public async Task ShouldRemoveCurrentPageAndPushTarget()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("GuardedPage");
            var guardedViewModel = (GuardedViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            guardedViewModel.CanNavigate = true;
            fixture.CallLog.Entries.Clear();

            // Act
            await fixture.NavigationService.NavigateAsync("../DetailPage");

            // Assert
            fixture.NavigationService.NavigationStack.Should().HaveCount(2);
            fixture.NavigationService.CurrentPage.Should().BeOfType<DetailPage>();
            fixture.NavigationService.NavigationStack[0].Should().BeOfType<HomePage>();

            fixture.CallLog.Entries.Should().ContainInOrder(
                "GuardedViewModel.NavigatedFromAsync(Back)",
                "DetailViewModel.NavigatedToAsync(Forward)",
                "GuardedViewModel.Dispose");
        }

        [Fact]
        public async Task ShouldRemoveSeveralPagesAndPushTarget()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("DetailPage");
            await fixture.NavigationService.PushAsync("ResultPage");

            // Act
            await fixture.NavigationService.NavigateAsync("../../LifecycleAwarePage");

            // Assert
            fixture.NavigationService.NavigationStack.Should().HaveCount(2);
            fixture.NavigationService.NavigationStack[0].Should().BeOfType<HomePage>();
            fixture.NavigationService.CurrentPage.Should().BeOfType<LifecycleAwarePage>();
        }

        [Fact]
        public async Task ShouldEvaluateGuardsOfRemovedPages()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("GuardedPage");
            ((GuardedViewModel)fixture.NavigationService.CurrentPage!.BindingContext).CanNavigate = false;

            // Act
            Func<Task> action = () => fixture.NavigationService.NavigateAsync("../DetailPage");

            // Assert
            await action.Should().ThrowAsync<NavigationCancelledException>();
            fixture.NavigationService.CurrentPage.Should().BeOfType<GuardedPage>();
            fixture.NavigationService.NavigationStack.Should().HaveCount(2);
        }

        [Fact]
        public async Task ShouldThrow_WhenRemovingMorePagesThanTheStackHolds()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            Func<Task> action = () => fixture.NavigationService.NavigateAsync("../DetailPage");

            // Assert
            await action.Should().ThrowAsync<NavigationException>().WithMessage("*root page cannot be removed*");
        }

        [Fact]
        public async Task ShouldThrow_WhenRelativeNavigationOnlyRemovesPages()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("DetailPage");

            // Act
            Func<Task> action = () => fixture.NavigationService.NavigateAsync("..");

            // Assert
            await action.Should().ThrowAsync<NavigationException>().WithMessage("*PopAsync*");
        }

        [Fact]
        public async Task ShouldDeliverQueryParametersOfADeepLink()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.NavigateAsync("DeepLinkPage?id=42&mode=edit");

            // Assert
            var viewModel = (DeepLinkViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            viewModel.ReceivedParameters.Should().NotBeNull();
            viewModel.ReceivedParameters!["mode"].Should().Be("edit");
            viewModel.ReceivedParameters.TryGetValue<int>("id", out var id).Should().BeTrue();
            id.Should().Be(42);
        }

        [Fact]
        public async Task ShouldNavigateFromAnApplicationLinkUri()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.NavigateAsync("superdev://app/NavigationPage/DeepLinkPage?id=7");

            // Assert
            fixture.NavigationService.CurrentPage.Should().BeOfType<DeepLinkPage>();
            var viewModel = (DeepLinkViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            viewModel.ReceivedParameters!.TryGetValue<int>("id", out var id).Should().BeTrue();
            id.Should().Be(7);
        }

        [Fact]
        public async Task ShouldThrow_WhenQueryStringAndParameterAreCombined()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            Func<Task> action = () => fixture.NavigationService.NavigateAsync(
                "DeepLinkPage?id=42", new TestParameter { Value = "x" });

            // Assert
            await action.Should().ThrowAsync<NavigationException>().WithMessage("*not both*");
        }

        [Fact]
        public async Task ShouldAskTheGuardsOfTheReplacedTree_OnAbsoluteNavigation()
        {
            // Arrange - a guarded page with unsaved changes, so to speak, is on the stack
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("GuardedPage");
            var guardedPage = fixture.NavigationService.CurrentPage!;
            fixture.CallLog.Entries.Clear();

            // Act - a deep link replaces the whole page tree
            Func<Task> action = () => fixture.NavigationService.NavigateAsync("/NavigationPage/DetailPage");

            // Assert
            await action.Should().ThrowAsync<NavigationCancelledException>(
                "a deep link arriving over unsaved changes is exactly the case a guard exists for");

            fixture.CallLog.Entries.Should().Contain("GuardedViewModel.CanNavigateAsync(False)");
            fixture.CallLog.Entries.Should().NotContain("GuardedViewModel.Dispose");
            fixture.NavigationService.CurrentPage.Should().BeSameAs(guardedPage, "a vetoed navigation changes nothing");
        }

        [Fact]
        public async Task ShouldReplaceTheTree_WhenItsGuardsAllowIt()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("GuardedPage");
            ((GuardedViewModel)fixture.NavigationService.CurrentPage!.BindingContext).CanNavigate = true;
            fixture.CallLog.Entries.Clear();

            // Act
            await fixture.NavigationService.NavigateAsync("/NavigationPage/DetailPage");

            // Assert
            fixture.NavigationService.CurrentPage.Should().BeOfType<DetailPage>();
            fixture.CallLog.Entries.Should().Contain("GuardedViewModel.CanNavigateAsync(True)");
            fixture.CallLog.Entries.Should().Contain("GuardedViewModel.Dispose").And.Contain("HomeViewModel.Dispose");
        }

        [Fact]
        public async Task ShouldTearDownInactiveTabChildren_WhenTheRootPageIsReplaced()
        {
            // Arrange - the second tab was never selected
            using var fixture = NavigationServiceFixture.Create();
            await fixture.NavigationService.NavigateAsync("/StaticTabbedPage");
            var tabbedPage = (StaticTabbedPage)fixture.Host.RootPage!;
            var inactiveTabChild = ((NavigationPage)tabbedPage.Children[1]).RootPage;

            // Act
            await fixture.NavigationService.NavigateAsync("/NavigationPage/HomePage");

            // Assert
            inactiveTabChild.BindingContext.Should().BeNull(
                "the children of tabs which were not selected leave the page tree as well, so they are torn down like every other page");
        }

        [Fact]
        public async Task ShouldDismissModalPages_WhenTheFlyoutDetailIsReplaced()
        {
            // Arrange - a modal page covers the flyout page
            using var fixture = NavigationServiceFixture.Create();
            await fixture.NavigationService.NavigateAsync("/ShellFlyoutPage/NavigationPage/HomePage");
            await fixture.NavigationService.PushModalAsync("DetailPage");
            var modalViewModel = (DetailViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            fixture.CallLog.Entries.Clear();

            // Act
            await fixture.NavigationService.NavigateAsync("NavigationPage/ResultPage");

            // Assert
            fixture.NavigationService.ModalStack.Should().BeEmpty("a modal page covers the flyout, so replacing its detail dismisses the modal page first");
            fixture.NavigationService.CurrentPage.Should().BeOfType<ResultPage>();

            fixture.CallLog.Entries.Should().ContainInOrder(
                "DetailViewModel.NavigatedFromAsync(Forward)",
                "ResultViewModel.NavigatedToAsync(Forward)");
            fixture.CallLog.Entries.Should().NotContain("HomeViewModel.NavigatedFromAsync(Forward)",
                "the covered detail page was not the active page, so it is not the one being left");
            fixture.CallLog.Entries.Should().Contain("HomeViewModel.Dispose");
            modalViewModel.IsDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldAskTheGuardOfAModalPage_WhenTheFlyoutDetailIsReplaced()
        {
            // Arrange - a guarded modal page covers the flyout page
            using var fixture = NavigationServiceFixture.Create();
            await fixture.NavigationService.NavigateAsync("/ShellFlyoutPage/NavigationPage/HomePage");
            await fixture.NavigationService.PushModalAsync("GuardedPage");

            // Act
            Func<Task> action = () => fixture.NavigationService.NavigateAsync("NavigationPage/ResultPage");

            // Assert
            await action.Should().ThrowAsync<NavigationCancelledException>();
            fixture.NavigationService.ModalStack.Should().HaveCount(1, "a vetoed navigation changes nothing");
        }

    }
}
