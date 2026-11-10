using UIModalPresentationStyle = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIModalPresentationStyle;
using static Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page;
using Superdev.Maui.Navigation;
using Superdev.Maui.Tests.Navigation.TestDoubles;

namespace Superdev.Maui.Tests.Navigation
{
    /// <summary>
    /// Covers the navigation hosts: flyout page composition, flyout detail replacement,
    /// back navigation triggered by the user, and the application lifecycle callbacks.
    /// </summary>
    public class NavigationServiceHostTests
    {
        [Fact]
        public async Task ShouldComposeFlyoutPageWithDetailPage()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();

            // Act
            await fixture.NavigationService.NavigateAsync("/ShellFlyoutPage/NavigationPage/HomePage");

            // Assert
            var rootPage = fixture.Host.RootPage.Should().BeOfType<ShellFlyoutPage>().Subject;
            rootPage.Detail.Should().BeOfType<Superdev.Maui.Controls.CustomNavigationPage>();
            fixture.NavigationService.CurrentPage.Should().BeOfType<HomePage>();
            fixture.CallLog.Entries.Should().Contain("HomeViewModel.NavigatedToAsync(Forward)");
        }

        [Fact]
        public async Task ShouldReplaceFlyoutDetail_WhenNavigatingRelativeToANavigationPage()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.NavigationService.NavigateAsync("/ShellFlyoutPage/NavigationPage/HomePage");
            var homeViewModel = (HomeViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            fixture.CallLog.Entries.Clear();

            // Act
            await fixture.NavigationService.NavigateAsync("NavigationPage/DetailPage");

            // Assert
            fixture.NavigationService.CurrentPage.Should().BeOfType<DetailPage>();
            fixture.Host.Operations.Should().Contain(o => o.StartsWith("SetFlyoutDetail"));
            fixture.CallLog.Entries.Should().ContainInOrder(
                "HomeViewModel.NavigatedFromAsync(Forward)",
                "DetailViewModel.NavigatedToAsync(Forward)");

            homeViewModel.IsDisposed.Should().BeTrue("the pages of the replaced detail must be disposed");
        }

        [Fact]
        public async Task ShouldNotReplaceFlyoutDetail_WhenNavigationGuardVetoes()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.NavigationService.NavigateAsync("/ShellFlyoutPage/NavigationPage/GuardedPage");
            var guardedViewModel = (GuardedViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            guardedViewModel.CanNavigate = false;

            // Act
            Func<Task> action = () => fixture.NavigationService.NavigateAsync("NavigationPage/DetailPage");

            // Assert
            await action.Should().ThrowAsync<NavigationCancelledException>();
            fixture.NavigationService.CurrentPage.Should().BeOfType<GuardedPage>();
            guardedViewModel.IsDisposed.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldCancelUserTriggeredDismissal_WhenPageHasNavigationGuard()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushModalAsync("GuardedPage", wrapInNavigationPage: false);

            var modalPage = fixture.NavigationService.CurrentPage!;
            ((GuardedViewModel)modalPage.BindingContext).CanNavigate = false;

            // Act
            var cancelled = fixture.Host.RaiseBackRequested(modalPage);

            // Assert
            cancelled.Should().BeTrue("a page with a navigation guard must not be dismissed by the platform");
            fixture.NavigationService.ModalStack.Should().HaveCount(1);
        }

        [Fact]
        public async Task ShouldNotCancelUserTriggeredDismissal_WhenPageHasNoNavigationGuard()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushModalAsync("DetailPage", wrapInNavigationPage: false);

            // Act
            var cancelled = fixture.Host.RaiseBackRequested(fixture.NavigationService.CurrentPage!);

            // Assert
            cancelled.Should().BeFalse("without a navigation guard the platform can dismiss the page itself");
        }

        [Fact]
        public async Task ShouldDispatchApplicationLifecycleToActivePage()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("LifecycleAwarePage");
            fixture.CallLog.Entries.Clear();

            // Act
            fixture.Host.RaiseApplicationLifecycle(resumed: false);
            fixture.Host.RaiseApplicationLifecycle(resumed: true);

            // Assert
            fixture.CallLog.Entries.Should().ContainInOrder(
                "LifecycleAwareViewModel.OnStoppedAsync",
                "LifecycleAwareViewModel.OnResumedAsync");
        }

        [Fact]
        public async Task ShouldDispatchApplicationLifecycleToCoveredPagesAsWell()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("LifecycleAwarePage");
            await fixture.NavigationService.PushAsync("DetailPage");
            fixture.CallLog.Entries.Clear();

            // Act
            fixture.Host.RaiseApplicationLifecycle(resumed: true);

            // Assert
            fixture.CallLog.Entries.Should().Contain(
                "LifecycleAwareViewModel.OnResumedAsync",
                "a page below the visible one holds resources too, so it must be notified as well");
        }

        [Fact]
        public async Task ShouldDispatchApplicationLifecycleToPagesBehindAModalPage()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("LifecycleAwarePage");
            await fixture.NavigationService.PushModalAsync("DetailPage");
            fixture.CallLog.Entries.Clear();

            // Act
            fixture.Host.RaiseApplicationLifecycle(resumed: false);

            // Assert
            fixture.CallLog.Entries.Should().Contain("LifecycleAwareViewModel.OnStoppedAsync");
        }

        [Fact]
        public async Task ShouldRaiseNavigationEvents_ForTheApplicationLifecycle()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            var navigatedRequests = new List<NavigationInfo>();
            fixture.NavigationService.Navigated += (_, e) => navigatedRequests.Add(e.Request);

            // Act
            fixture.Host.RaiseApplicationLifecycle(resumed: false);
            fixture.Host.RaiseApplicationLifecycle(resumed: true);

            // Assert - the callbacks are an operation of the pipeline, so they show up like any other
            navigatedRequests.Select(r => r.Operation).Should().Equal(
                NavigationOperation.ApplicationStopped,
                NavigationOperation.ApplicationResumed);
            navigatedRequests.Should().OnlyContain(r => r.Direction == NavigationDirection.None && r.PageName == null);
        }

        [Fact]
        public async Task ShouldQueueTheApplicationLifecycleBehindARunningNavigation()
        {
            // Arrange - a navigation is held inside its initialization
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("LifecycleAwarePage");

            var gate = fixture.GetService<NavigationGate>();
            gate.Close();
            var gatedPush = fixture.NavigationService.PushAsync("GatedPage");
            fixture.CallLog.Entries.Clear();

            var resumeDispatched = new TaskCompletionSource();
            fixture.NavigationService.Navigated += (_, e) =>
            {
                if (e.Request.Operation == NavigationOperation.ApplicationResumed)
                {
                    resumeDispatched.TrySetResult();
                }
            };

            // Act - the application resumes while the navigation is still running
            fixture.Host.RaiseApplicationLifecycle(resumed: true);

            // Assert - the callbacks wait for the navigation, so they never see a page tree in transition
            fixture.CallLog.Entries.Should().NotContain(
                "LifecycleAwareViewModel.OnResumedAsync",
                "the lifecycle callbacks are serialized behind the running navigation");

            gate.Open();
            await gatedPush;
            await resumeDispatched.Task.WaitAsync(TimeSpan.FromSeconds(5));

            fixture.CallLog.Entries.Should().Contain("LifecycleAwareViewModel.OnResumedAsync");
        }


        [Fact]
        public async Task ShouldPresentAGuardedModalPageFullScreen_EvenIfItAskedForASheet()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.PushModalAsync("GuardedSheetPage", wrapInNavigationPage: false);
            var guardedModalPage = fixture.NavigationService.ModalStack[^1];
            await fixture.NavigationService.PushModalAsync("SheetPage", wrapInNavigationPage: false);
            var sheetPage = fixture.NavigationService.ModalStack[^1];

            // Assert
            GetModalPresentationStyle(guardedModalPage).Should().Be(UIModalPresentationStyle.FullScreen,
                "a sheet can be swiped away, and that gesture cannot be vetoed by a guard");
            GetModalPresentationStyle(sheetPage).Should().Be(UIModalPresentationStyle.PageSheet,
                "a page without a guard keeps the presentation it asked for");
        }

        [Fact]
        public async Task ShouldKeepTheSheetOfAGuardedModalPage_WhenSwitchedOff()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create(configureOptions: o => o.PresentGuardedModalPagesFullScreen = false);
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.PushModalAsync("GuardedSheetPage", wrapInNavigationPage: false);

            // Assert
            GetModalPresentationStyle(fixture.NavigationService.ModalStack[^1]).Should().Be(UIModalPresentationStyle.PageSheet);
        }

        [Fact]
        public async Task ShouldAskTheRootPageGuardOnce_WhenBackIsPressedAgainWhileItAsks()
        {
            // Arrange - the guard of the root page holds its answer, like a dialog does
            using var fixture = NavigationServiceFixture.Create();
            var rootPage = await fixture.SetupRootAsync("GuardedPage");
            var guard = (GuardedViewModel)rootPage.BindingContext;
            var dialog = new TaskCompletionSource();
            guard.CanNavigate = true;
            guard.Hold = () => dialog.Task;

            // Act - the user presses back twice while the dialog is open
            var firstCancelled = fixture.Host.RaiseBackRequested(rootPage);
            var secondCancelled = fixture.Host.RaiseBackRequested(rootPage);

            // Assert
            firstCancelled.Should().BeTrue("the platform must wait for the guard");
            secondCancelled.Should().BeTrue("a second press while the guard asks is swallowed rather than starting a second guard");
            fixture.CallLog.Entries.Count(e => e == "GuardedViewModel.CanNavigateAsync(True)").Should().Be(1, "one guard, one dialog");

            dialog.SetResult();
            await fixture.NavigationService.BackFromRootPageConfirmed.WaitAsync(TimeSpan.FromSeconds(5));
            fixture.NavigationService.BackFromRootPageConfirmedCount.Should().Be(1);
        }

        [Fact]
        public async Task ShouldRouteBackThroughTheGuard_OnAPageInsideATab()
        {
            // Arrange - a guarded page sits on the navigation stack of a tab
            using var fixture = NavigationServiceFixture.Create();
            await fixture.NavigationService.NavigateAsync("/StaticTabbedPage");
            await fixture.NavigationService.SelectTabAsync("DetailPage");
            await fixture.NavigationService.PushAsync("GuardedPage");
            var guardedPage = fixture.NavigationService.CurrentPage!;
            fixture.CallLog.Entries.Clear();

            // Act - the user presses back
            var cancelled = fixture.Host.RaiseBackRequested(guardedPage);

            // Assert
            cancelled.Should().BeTrue("the platform must not pop before the guard was asked");
            fixture.CallLog.Entries.Should().Contain("GuardedViewModel.CanNavigateAsync(False)");
            fixture.NavigationService.CurrentPage.Should().BeSameAs(guardedPage, "the guard vetoed");
            fixture.NavigationService.BackFromRootPageConfirmedCount.Should().Be(0, "a page inside a tab is not the root page");
        }
    }
}
