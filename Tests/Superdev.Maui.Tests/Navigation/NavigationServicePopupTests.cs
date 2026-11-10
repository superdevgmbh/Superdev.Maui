using Superdev.Maui.Navigation;
using Superdev.Maui.Tests.Navigation.TestDoubles;

namespace Superdev.Maui.Tests.Navigation
{
    /// <summary>
    /// Covers popup pages: they are presented as transparent modal pages, but take part in the
    /// navigation lifecycle, the service scopes, the navigation guards and the result navigation
    /// exactly like any other page.
    /// </summary>
    public class NavigationServicePopupTests
    {
        [Fact]
        public async Task ShouldPresentPopupAsModalPage_WhenPushed()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.PushAsync("ContextMenuPopupPage");

            // Assert
            fixture.NavigationService.ModalStack.Should().HaveCount(1, "a popup lives on the modal stack");
            fixture.NavigationService.NavigationStack.Should().HaveCount(1, "the navigation stack is untouched");
            fixture.NavigationService.CurrentPage.Should().BeOfType<ContextMenuPopupPage>();

            fixture.Host.Operations.Should().Contain(o => o.StartsWith("PushModal(ContextMenuPopupPage"));
            fixture.Host.Operations.Should().NotContain(o => o.StartsWith("Push(ContextMenuPopupPage"));
        }

        [Fact]
        public async Task ShouldDispatchLifecycleAndDeliverParameterToPopup()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.PushAsync("ContextMenuPopupPage", new TestParameter { Value = "menu" });

            // Assert
            var viewModel = (ContextMenuViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            viewModel.ReceivedParameter!.Value.Should().Be("menu");

            fixture.CallLog.Entries.Should().ContainInOrder(
                "ContextMenuViewModel.InitializeAsync(menu)",
                "HomeViewModel.NavigatedFromAsync(Forward)",
                "ContextMenuViewModel.NavigatedToAsync(Forward)");
        }

        [Fact]
        public async Task ShouldClosePopup_WhenPoppingWithTheOrdinaryPopCall()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("ContextMenuPopupPage");
            var popupPage = (ContextMenuPopupPage)fixture.NavigationService.CurrentPage!;
            var viewModel = (ContextMenuViewModel)popupPage.BindingContext;
            fixture.CallLog.Entries.Clear();

            // Act
            await fixture.NavigationService.PopAsync();

            // Assert
            fixture.NavigationService.ModalStack.Should().BeEmpty();
            fixture.NavigationService.CurrentPage.Should().BeOfType<HomePage>();

            fixture.CallLog.Entries.Should().ContainInOrder(
                "ContextMenuViewModel.NavigatedFromAsync(Back)",
                "HomeViewModel.NavigatedToAsync(Back)",
                "ContextMenuViewModel.Dispose");

            popupPage.ScopedService.IsDisposed.Should().BeTrue();
            viewModel.IsDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldClosePopup_WhenBackgroundIsClicked()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("ContextMenuPopupPage");
            var popupPage = (ContextMenuPopupPage)fixture.NavigationService.CurrentPage!;

            // Act
            popupPage.SimulateBackgroundClick();

            // Assert
            fixture.NavigationService.ModalStack.Should().BeEmpty();
            popupPage.ScopedService.IsDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldNotClosePopup_WhenBackgroundClickIsDisabled()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("ModalPopupPage");
            var popupPage = (ModalPopupPage)fixture.NavigationService.CurrentPage!;

            // Act
            popupPage.SimulateBackgroundClick();

            // Assert
            fixture.NavigationService.ModalStack.Should().HaveCount(1);
            popupPage.ScopedService.IsDisposed.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldNotClosePopup_WhenNavigationGuardVetoes()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("ContextMenuPopupPage");

            // The context menu shares its view model registration with a guard in this fixture setup,
            // therefore the guard is attached explicitly here.
            var popupPage = (ContextMenuPopupPage)fixture.NavigationService.CurrentPage!;
            popupPage.BindingContext = new GuardedViewModel(fixture.CallLog, popupPage.ScopedService)
            {
                CanNavigate = false
            };

            // Act
            Func<Task> action = () => fixture.NavigationService.PopAsync();

            // Assert
            await action.Should().ThrowAsync<NavigationCancelledException>();
            fixture.NavigationService.ModalStack.Should().HaveCount(1);
        }

        [Fact]
        public async Task ShouldReturnResultFromPopup()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            var resultTask = fixture.NavigationService.PushForResultAsync<TestResult>("ContextMenuPopupPage");
            fixture.NavigationService.CurrentPage.Should().BeOfType<ContextMenuPopupPage>();

            await fixture.NavigationService.PopAsync(new TestResult { Value = "selected" });
            var result = await resultTask;

            // Assert
            result!.Value.Should().Be("selected");
            fixture.NavigationService.ModalStack.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldCompletePopupResult_WhenClosedByBackgroundClick()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            var resultTask = fixture.NavigationService.PushForResultAsync<TestResult>("ContextMenuPopupPage");
            var popupPage = (ContextMenuPopupPage)fixture.NavigationService.CurrentPage!;

            // Act
            popupPage.SimulateBackgroundClick();

            // Assert
            var result = await resultTask.WaitAsync(TimeSpan.FromSeconds(5));
            result.Should().BeNull("a popup which was dismissed without a result completes with the default value");
        }

        [Fact]
        public async Task ShouldNotLeakPopupPageAndViewModel()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            var (weakPage, weakViewModel) = await PushAndClosePopupAsync(fixture);

            for (var i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            // Assert
            weakPage.IsAlive.Should().BeFalse();
            weakViewModel.IsAlive.Should().BeFalse();
        }

        private static async Task<(WeakReference Page, WeakReference ViewModel)> PushAndClosePopupAsync(
            NavigationServiceFixture fixture)
        {
            await fixture.NavigationService.PushAsync("ContextMenuPopupPage");

            var page = fixture.NavigationService.CurrentPage!;
            var weakReferences = (new WeakReference(page), new WeakReference(page.BindingContext));

            await fixture.NavigationService.PopAsync();

            return weakReferences;
        }

        [Fact]
        public async Task ShouldClosePopup_WhenThePopIsQueuedBehindThePushOfThePopup()
        {
            // Arrange - a navigation is held inside its initialization, so everything after it queues
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            var gate = fixture.GetService<NavigationGate>();
            gate.Close();
            var gatedPush = fixture.NavigationService.PushAsync("GatedPage");

            // Act - when the pop is requested no popup is open yet; by the time it runs, one is
            var popupPush = fixture.NavigationService.PushAsync("ContextMenuPopupPage");
            var pop = fixture.NavigationService.PopAsync();

            gate.Open();
            await Task.WhenAll(gatedPush, popupPush, pop);

            // Assert - the pop applied to the popup, not to the page underneath it
            fixture.NavigationService.ModalStack.Should().BeEmpty(
                "whether a pop closes a popup or a page is decided inside the pipeline, against the stack as it is then");
            fixture.NavigationService.CurrentPage.Should().BeOfType<GatedPage>();
            fixture.NavigationService.NavigationStack.Should().HaveCount(2);
        }

        [Fact]
        public async Task ShouldCloseAllPopups()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("ContextMenuPopupPage");
            await fixture.NavigationService.PushAsync("ContextMenuPopupPage");
            fixture.CallLog.Entries.Clear();

            // Act
            await fixture.NavigationService.PopAllPopupsAsync();

            // Assert
            fixture.NavigationService.ModalStack.Should().BeEmpty();
            fixture.NavigationService.CurrentPage.Should().BeOfType<HomePage>();
            fixture.CallLog.Entries.Count(e => e == "ContextMenuViewModel.Dispose").Should().Be(2);
        }

        [Fact]
        public async Task ShouldNotRaiseNavigationEvents_WhenThereIsNoPopupToClose()
        {
            // Arrange - a modal page which is not a popup is on top
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushModalAsync("DetailPage");

            var navigatingRequests = new List<NavigationInfo>();
            fixture.NavigationService.Navigating += (_, e) => navigatingRequests.Add(e.Request);

            // Act
            await fixture.NavigationService.PopAllPopupsAsync();

            // Assert
            navigatingRequests.Should().BeEmpty("deciding that there is nothing to close is not a navigation");
            fixture.NavigationService.ModalStack.Should().HaveCount(1, "a modal page which is not a popup is left alone");
        }

    }
}
