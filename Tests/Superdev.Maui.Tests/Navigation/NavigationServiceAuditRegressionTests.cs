using Superdev.Maui.Navigation;
using Superdev.Maui.Navigation.Hosts;
using Superdev.Maui.Tests.Navigation.TestDoubles;

namespace Superdev.Maui.Tests.Navigation
{
    /// <summary>
    /// One test per defect found by the navigation audit. Each of them fails on the implementation which
    /// the audit reviewed, so that none of these defects can come back unnoticed.
    /// </summary>
    public class NavigationServiceAuditRegressionTests
    {
        [Fact]
        public async Task ShouldShowPopupContent_WhenItIsPresentedWithoutAnimation()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.PushAsync("ContextMenuPopupPage", animated: false);

            // Assert
            var popupPage = fixture.NavigationService.CurrentPage.Should().BeOfType<ContextMenuPopupPage>().Subject;
            popupPage.Content!.Opacity.Should().Be(1,
                "preparing the appearing animation hides the content, so it must not be prepared when no " +
                "animation is going to run");
        }

        [Fact]
        public async Task ShouldPresentPopupAsAnOverlay_WhenItIsTheTargetOfANavigationUri()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            var navigationStackBefore = fixture.NavigationService.NavigationStack.Count;

            // Act
            await fixture.NavigationService.NavigateAsync("ContextMenuPopupPage");

            // Assert
            fixture.NavigationService.ModalStack.Should().ContainSingle()
                .Which.Should().BeOfType<ContextMenuPopupPage>();
            fixture.NavigationService.NavigationStack.Should().HaveCount(navigationStackBefore,
                "a popup is an overlay, so it must not be pushed onto the navigation stack");
        }

        [Fact]
        public async Task ShouldRejectAPopup_WhichIsCombinedWithOtherUriSegments()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            var relative = () => fixture.NavigationService.NavigateAsync("DetailPage/ContextMenuPopupPage");
            var absolute = () => fixture.NavigationService.NavigateAsync("/NavigationPage/ContextMenuPopupPage");

            // Assert
            await relative.Should().ThrowAsync<NavigationException>();
            await absolute.Should().ThrowAsync<NavigationException>();
        }

        [Fact]
        public async Task ShouldDisposeThePageScope_WhenTheInitializationFails()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            var navigationStackBefore = fixture.NavigationService.NavigationStack.Count;

            // Act
            var act = () => fixture.NavigationService.PushAsync("ThrowingInitializePage");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();

            fixture.CallLog.Entries.Should().Contain($"{nameof(ThrowingInitializeViewModel)}.Dispose",
                "a page which never reached the navigation stack must not keep its service scope");
            fixture.NavigationService.NavigationStack.Should().HaveCount(navigationStackBefore);
        }

        /// <remarks>
        /// This guards the disposal only. That it no longer blocks on the asynchronous disposal cannot be
        /// observed here, because the test main thread runs continuations inline.
        /// </remarks>
        [Fact]
        public async Task ShouldDisposeThePageScope_WhenTheNavigationParameterIsRejected()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            var act = () => fixture.NavigationService.PushAsync("HomePage", new TestParameter { Value = "x" });

            // Assert
            await act.Should().ThrowAsync<NavigationParameterException>();
            fixture.CallLog.Entries.Should().Contain($"{nameof(HomeViewModel)}.Dispose");
        }

        [Fact]
        public async Task ShouldDisposeTheRemovedPage_WhenTheRevealedPageFailsToHandleTheNavigation()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync("ThrowingNavigatedToPage");
            await fixture.NavigationService.PushAsync("DetailPage");
            fixture.CallLog.Entries.Clear();

            // Act
            var act = () => fixture.NavigationService.PopAsync();

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            fixture.CallLog.Entries.Should().Contain($"{nameof(DetailViewModel)}.Dispose",
                "the popped page is gone either way, so its service scope has to be released");
        }

        [Fact]
        public async Task ShouldDisposeAViewModel_WhichWasResolvedByConvention()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create(conventionBasedResolution: true);
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("ConventionOnlyPage");

            // Act
            await fixture.NavigationService.PopAsync();

            // Assert
            fixture.CallLog.Entries.Should().Contain($"{nameof(ConventionOnlyViewModel)}.Dispose",
                "a view model which the container did not create is still owned by the page scope");
        }

        [Fact]
        public async Task ShouldEvaluateTheNavigationGuard_WhenBackIsPressedOnTheRootPage()
        {
            // Arrange - a guarded page which is the root of its navigation stack, so nothing can be popped
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync("GuardedPage");

            var guardedViewModel = (GuardedViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            guardedViewModel.CanNavigate = false;

            // Act
            var handled = fixture.Host.RaiseBackRequested(fixture.NavigationService.CurrentPage!);

            // Assert
            handled.Should().BeTrue("the platform must not act before the guard was asked");
            fixture.CallLog.Entries.Should().Contain(
                entry => entry.StartsWith($"{nameof(GuardedViewModel)}.CanNavigateAsync"));
            fixture.NavigationService.BackFromRootPageConfirmedCount.Should().Be(0);
        }

        [Fact]
        public async Task ShouldConfirmBackFromTheRootPage_WhenTheNavigationGuardPermitsIt()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync("GuardedPage");

            var guardedViewModel = (GuardedViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            guardedViewModel.CanNavigate = true;

            // Act
            fixture.Host.RaiseBackRequested(fixture.NavigationService.CurrentPage!);

            // Assert
            fixture.NavigationService.BackFromRootPageConfirmedCount.Should().Be(1);
        }

        [Fact]
        public async Task ShouldRaiseTheNavigatingEvent_WhenTheUserSelectsATab()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.NavigationService.NavigateAsync("/StaticTabbedPage");
            var tabbedPage = (StaticTabbedPage)fixture.Host.RootPage!;

            var navigatingRequests = new List<NavigationInfo>();
            fixture.NavigationService.Navigating += (_, e) => navigatingRequests.Add(e.Request);

            // Act
            fixture.Host.RaiseTabSelectedExternally(tabbedPage, tabbedPage.Children[1]);

            // Assert
            navigatingRequests.Should().ContainSingle()
                .Which.Operation.Should().Be(NavigationOperation.SelectTab);
        }

        [Fact]
        public async Task ShouldRaiseTheNavigatingEvent_WhenAPageIsRemovedExternally()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("DetailPage");
            var detailPage = fixture.NavigationService.CurrentPage!;

            var navigatingRequests = new List<NavigationInfo>();
            fixture.NavigationService.Navigating += (_, e) => navigatingRequests.Add(e.Request);

            // Act
            fixture.Host.RaisePageRemovedExternally(detailPage, PageRemovalKind.Popped);

            // Assert
            navigatingRequests.Should().ContainSingle()
                .Which.Operation.Should().Be(NavigationOperation.SystemBack);
        }

        [Fact]
        public async Task ShouldRunTheStartNavigationThroughThePipeline()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();

            var navigatingRequests = new List<NavigationInfo>();
            fixture.NavigationService.Navigating += (_, e) => navigatingRequests.Add(e.Request);

            var navigated = new TaskCompletionSource();
            fixture.NavigationService.Navigated += (_, _) => navigated.TrySetResult();

            var window = fixture.NavigationService.CreateWindow("/NavigationPage/HomePage");
            await fixture.Host.SetRootPageAsync(window.Page!);

            // Act - the platform reports the window it created
            ((IWindow)window).Created();

            // Assert
            await navigated.Task.WaitAsync(TimeSpan.FromSeconds(5));

            navigatingRequests.Should().ContainSingle("the start navigation uses the same pipeline as every other navigation");
            fixture.CallLog.Entries.Should().Contain($"{nameof(HomeViewModel)}.{nameof(HomeViewModel.InitializeAsync)}");
            fixture.CallLog.Entries.Should().Contain($"{nameof(HomeViewModel)}.NavigatedToAsync(Forward)");
        }
    }
}
