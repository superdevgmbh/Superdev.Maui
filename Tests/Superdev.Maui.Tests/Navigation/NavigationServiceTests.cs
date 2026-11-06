using Microsoft.Extensions.DependencyInjection;
using Superdev.Maui.Navigation;
using Superdev.Maui.Navigation.Hosts;
using Superdev.Maui.Tests.Navigation.TestDoubles;

namespace Superdev.Maui.Tests.Navigation
{
    public class NavigationServiceTests
    {
        [Fact]
        public async Task ShouldPushPage_DispatchingLifecycleInOrder()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.PushAsync("DetailPage");

            // Assert
            fixture.CallLog.Entries.Should().ContainInOrder(
                "DetailViewModel.InitializeAsync",
                "HomeViewModel.NavigatedFromAsync(Forward)",
                "DetailViewModel.NavigatedToAsync(Forward)");

            fixture.NavigationService.NavigationStack.Should().HaveCount(2);
            fixture.NavigationService.CurrentPage.Should().BeOfType<DetailPage>();
        }

        [Fact]
        public async Task ShouldPushPage_DeliveringTypedParameter()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.PushAsync("DetailPage", new TestParameter { Value = "42" });

            // Assert
            var viewModel = (DetailViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            viewModel.ReceivedParameter!.Value.Should().Be("42");
            fixture.CallLog.Entries.Should().Contain("DetailViewModel.InitializeAsync(42)");
        }

        [Fact]
        public async Task ShouldPushPage_DeliveringDerivedParameterToBaseTypeReceiver()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.PushAsync("DetailPage", new DerivedTestParameter { Value = "derived" });

            // Assert
            var viewModel = (DetailViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            viewModel.ReceivedParameter.Should().BeOfType<DerivedTestParameter>();
        }

        [Fact]
        public async Task ShouldThrowNavigationParameterException_WhenTargetCannotReceiveParameter()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            Func<Task> action = () => fixture.NavigationService.PushAsync("HomePage", new TestParameter { Value = "x" });

            // Assert
            await action.Should().ThrowAsync<NavigationParameterException>();
            fixture.NavigationService.NavigationStack.Should().HaveCount(1, "the navigation stack must not be touched");
            fixture.CallLog.Entries.Should().NotContain(e => e.Contains("InitializeAsync"));
            fixture.CallLog.Entries.Should().NotContain(e => e.Contains("NavigatedToAsync"));
            fixture.CallLog.Entries.Should().Contain(
                "HomeViewModel.Dispose", "the service scope of the rejected page must be disposed");
        }

        [Fact]
        public async Task ShouldThrowPageResolveException_WhenPageIsNotRegistered()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            Func<Task> action = () => fixture.NavigationService.PushAsync("UnknownPage");

            // Assert
            await action.Should().ThrowAsync<PageResolveException>();
            fixture.NavigationService.NavigationStack.Should().HaveCount(1);
        }

        [Fact]
        public async Task ShouldPopPage_DispatchingLifecycleAndDisposingScope()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("DetailPage");

            var detailPage = (DetailPage)fixture.NavigationService.CurrentPage!;
            var detailViewModel = (DetailViewModel)detailPage.BindingContext;
            fixture.CallLog.Entries.Clear();

            // Act
            await fixture.NavigationService.PopAsync();

            // Assert
            fixture.CallLog.Entries.Should().ContainInOrder(
                "DetailViewModel.NavigatedFromAsync(Back)",
                "HomeViewModel.NavigatedToAsync(Back)",
                "DetailViewModel.Dispose");

            detailViewModel.IsDisposed.Should().BeTrue();
            detailPage.ScopedService.IsDisposed.Should().BeTrue();
            detailPage.BindingContext.Should().BeNull("the binding context is detached, so that the page cannot keep the view model alive");
        }

        [Fact]
        public async Task ShouldSharedScopedServiceBetweenPageAndViewModel()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.PushAsync("DetailPage");

            // Assert
            var detailPage = (DetailPage)fixture.NavigationService.CurrentPage!;
            var detailViewModel = (DetailViewModel)detailPage.BindingContext;
            detailPage.ScopedService.Should().BeSameAs(detailViewModel.ScopedService);
        }

        [Fact]
        public async Task ShouldCancelPop_WhenNavigationGuardVetoes()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("GuardedPage");

            var guardedViewModel = (GuardedViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            guardedViewModel.CanNavigate = false;
            fixture.CallLog.Entries.Clear();

            // Act
            Func<Task> action = () => fixture.NavigationService.PopAsync();

            // Assert
            await action.Should().ThrowAsync<NavigationCancelledException>();
            fixture.NavigationService.NavigationStack.Should().HaveCount(2, "the page must remain on the navigation stack");
            fixture.CallLog.Entries.Should().NotContain(e => e.Contains("NavigatedFromAsync"));
            guardedViewModel.IsDisposed.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldPop_WhenNavigationGuardConfirms()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("GuardedPage");

            var guardedViewModel = (GuardedViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            guardedViewModel.CanNavigate = true;

            // Act
            await fixture.NavigationService.PopAsync();

            // Assert
            fixture.NavigationService.NavigationStack.Should().HaveCount(1);
            guardedViewModel.IsDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldDeliverResultToRevealedPage_WhenPoppingWithResult()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("DetailPage");

            var detailViewModel = (DetailViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            await fixture.NavigationService.PushAsync("ResultPage");

            // Act
            await fixture.NavigationService.PopAsync(new TestResult { Value = "done" });

            // Assert
            detailViewModel.ReceivedResult!.Value.Should().Be("done");
            fixture.CallLog.Entries.Should().Contain("DetailViewModel.NavigatedToAsync(Back, done)");
        }

        [Fact]
        public async Task ShouldCompletePushForResult_WithTheResultOfThePoppedPage()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            var resultTask = fixture.NavigationService.PushForResultAsync<TestResult>("ResultPage");
            fixture.NavigationService.CurrentPage.Should().BeOfType<ResultPage>();

            await fixture.NavigationService.PopAsync(new TestResult { Value = "ok" });
            var result = await resultTask;

            // Assert
            result!.Value.Should().Be("ok");
        }

        [Fact]
        public async Task ShouldCompletePushForResult_WithDefault_WhenPoppedWithoutResult()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            var resultTask = fixture.NavigationService.PushForResultAsync<TestResult>("ResultPage");
            await fixture.NavigationService.PopAsync();
            var result = await resultTask;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ShouldCompletePushForResult_WhenPageIsRemovedExternally()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            var resultTask = fixture.NavigationService.PushForResultAsync<TestResult>("ResultPage");
            var resultPage = fixture.NavigationService.CurrentPage!;

            // Act
            fixture.Host.RaisePageRemovedExternally(resultPage, PageRemovalKind.Popped);

            // Assert
            var result = await resultTask.WaitAsync(TimeSpan.FromSeconds(5));
            result.Should().BeNull();
        }

        [Fact]
        public async Task ShouldPopToRoot_DisposingAllIntermediatePages()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("DetailPage");
            var detailViewModel = (DetailViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            await fixture.NavigationService.PushAsync("ResultPage");
            var resultViewModel = (ResultViewModel)fixture.NavigationService.CurrentPage!.BindingContext;

            // Act
            await fixture.NavigationService.PopToRootAsync();

            // Assert
            fixture.NavigationService.NavigationStack.Should().HaveCount(1);
            fixture.NavigationService.CurrentPage.Should().BeOfType<HomePage>();
            detailViewModel.IsDisposed.Should().BeTrue();
            resultViewModel.IsDisposed.Should().BeTrue();
            fixture.CallLog.Entries.Should().Contain("HomeViewModel.NavigatedToAsync(Back)");
        }

        [Fact]
        public async Task ShouldPushAndPopModalPage()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.PushModalAsync("DetailPage");
            var detailViewModel = (DetailViewModel)fixture.NavigationService.CurrentPage!.BindingContext;

            // Assert
            fixture.NavigationService.ModalStack.Should().HaveCount(1);
            fixture.CallLog.Entries.Should().ContainInOrder(
                "DetailViewModel.InitializeAsync",
                "HomeViewModel.NavigatedFromAsync(Forward)",
                "DetailViewModel.NavigatedToAsync(Forward)");

            // Act
            await fixture.NavigationService.PopModalAsync();

            // Assert
            fixture.NavigationService.ModalStack.Should().BeEmpty();
            detailViewModel.IsDisposed.Should().BeTrue();
            fixture.CallLog.Entries.Should().Contain("HomeViewModel.NavigatedToAsync(Back)");
        }

        [Fact]
        public async Task ShouldCompensateExternalPageRemoval()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("DetailPage");

            var detailPage = (DetailPage)fixture.NavigationService.CurrentPage!;
            var detailViewModel = (DetailViewModel)detailPage.BindingContext;
            fixture.CallLog.Entries.Clear();

            // Act
            fixture.Host.RaisePageRemovedExternally(detailPage, PageRemovalKind.Popped);

            // Assert
            fixture.CallLog.Entries.Should().ContainInOrder(
                "DetailViewModel.NavigatedFromAsync(Back)",
                "HomeViewModel.NavigatedToAsync(Back)",
                "DetailViewModel.Dispose");

            detailViewModel.IsDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldAllowNavigation_FromNavigatedTo()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create(services => { });
            await fixture.SetupRootAsync();

            var navigationHook = fixture.GetService<NavigationHook>();
            navigationHook.OnNavigatedTo = () => fixture.NavigationService.PushAsync("DetailPage");

            // Act
            await fixture.NavigationService.PushAsync("RedirectPage");

            // Assert
            fixture.NavigationService.CurrentPage.Should().BeOfType<DetailPage>();
            fixture.NavigationService.NavigationStack.Should().HaveCount(3);
        }

        [Fact]
        public async Task ShouldThrow_WhenNavigatingFromInitialize()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            var navigationHook = fixture.GetService<NavigationHook>();
            navigationHook.OnInitialize = () => fixture.NavigationService.PushAsync("DetailPage");

            // Act
            Func<Task> action = () => fixture.NavigationService.PushAsync("RedirectPage");

            // Assert
            await action.Should().ThrowAsync<NavigationException>()
                .WithMessage("*already in progress*");
        }

        [Fact]
        public async Task ShouldRaiseNavigationEvents()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            var navigatingRequests = new List<NavigationInfo>();
            var navigatedRequests = new List<NavigationInfo>();
            fixture.NavigationService.Navigating += (_, e) => navigatingRequests.Add(e.Request);
            fixture.NavigationService.Navigated += (_, e) => navigatedRequests.Add(e.Request);

            // Act
            await fixture.NavigationService.PushAsync("DetailPage");

            // Assert
            navigatingRequests.Should().HaveCount(1);
            navigatingRequests[0].Operation.Should().Be(NavigationOperation.Push);
            navigatingRequests[0].PageName.Should().Be("DetailPage");
            navigatedRequests.Should().HaveCount(1);
        }

        [Fact]
        public async Task ShouldRaiseNavigationFailed_WhenGuardVetoes()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("GuardedPage");
            ((GuardedViewModel)fixture.NavigationService.CurrentPage!.BindingContext).CanNavigate = false;

            var failures = new List<NavigationFailedEventArgs>();
            fixture.NavigationService.NavigationFailed += (_, e) => failures.Add(e);

            // Act
            await Assert.ThrowsAsync<NavigationCancelledException>(() => fixture.NavigationService.PopAsync());

            // Assert
            failures.Should().HaveCount(1);
            failures[0].Exception.Should().BeOfType<NavigationCancelledException>();
        }

        [Fact]
        public async Task ShouldNavigateAbsolute_ReplacingTheRootPage()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("DetailPage");
            var detailViewModel = (DetailViewModel)fixture.NavigationService.CurrentPage!.BindingContext;

            // Act
            await fixture.NavigationService.NavigateAsync("/NavigationPage/ResultPage");

            // Assert
            fixture.NavigationService.CurrentPage.Should().BeOfType<ResultPage>();
            fixture.NavigationService.NavigationStack.Should().HaveCount(1);
            detailViewModel.IsDisposed.Should().BeTrue("the pages of the replaced tree must be disposed");
            fixture.CallLog.Entries.Should().Contain("ResultViewModel.NavigatedToAsync(Forward)");
        }

        [Fact]
        public async Task ShouldNavigateRelative_PushingAllSegments()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.NavigateAsync("DetailPage/ResultPage");

            // Assert
            fixture.NavigationService.NavigationStack.Should().HaveCount(3);
            fixture.NavigationService.CurrentPage.Should().BeOfType<ResultPage>();

            // Only the final target becomes the active page.
            fixture.CallLog.Entries.Should().Contain("DetailViewModel.InitializeAsync");
            fixture.CallLog.Entries.Should().Contain("ResultViewModel.NavigatedToAsync(Forward)");
            fixture.CallLog.Entries.Should().NotContain("DetailViewModel.NavigatedToAsync(Forward)");
        }

        [Fact]
        public async Task ShouldInitializeBaseViewModel_SettingIsInitialized()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.PushAsync("BaseViewModelBackedPage");

            // Assert
            var viewModel = (BaseViewModelBackedViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            viewModel.WasInitialized.Should().BeTrue();
            viewModel.IsInitialized.Should().BeTrue();
            viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldSetIsInitialized_WhenBaseViewModelReceivesTypedParameter()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            await fixture.NavigationService.PushAsync("BaseViewModelBackedPage", new TestParameter { Value = "7" });

            // Assert
            var viewModel = (BaseViewModelBackedViewModel)fixture.NavigationService.CurrentPage!.BindingContext;
            viewModel.ReceivedParameter!.Value.Should().Be("7");
            viewModel.WasInitialized.Should().BeFalse("the typed initialization callback replaces the parameterless one");
            viewModel.IsInitialized.Should().BeTrue("IsInitialized is set no matter which callback ran");
        }

        [Fact]
        public async Task ShouldNotLeakPageAndViewModel_AfterPop()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            var (weakPage, weakViewModel) = await PushAndPopDetailPageAsync(fixture);

            for (var i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            // Assert
            weakPage.IsAlive.Should().BeFalse("the popped page must not be kept alive");
            weakViewModel.IsAlive.Should().BeFalse("the view model of the popped page must not be kept alive");
        }

        private static async Task<(WeakReference Page, WeakReference ViewModel)> PushAndPopDetailPageAsync(
            NavigationServiceFixture fixture)
        {
            await fixture.NavigationService.PushAsync("DetailPage");

            var page = fixture.NavigationService.CurrentPage!;
            var weakReferences = (new WeakReference(page), new WeakReference(page.BindingContext));

            await fixture.NavigationService.PopAsync();

            return weakReferences;
        }

        public static TheoryData<string, string, Func<INavigationService, Task>> UnregisteredTargetNavigations =>
            new TheoryData<string, string, Func<INavigationService, Task>>
            {
                { "PushAsync<TViewModel>", nameof(UnregisteredViewModel), s => s.PushAsync<UnregisteredViewModel>() },
                { "PushModalAsync<TViewModel>", nameof(UnregisteredViewModel), s => s.PushModalAsync<UnregisteredViewModel>() },
                { "PushAsRootAsync<TViewModel>", nameof(UnregisteredViewModel), s => s.PushAsRootAsync<UnregisteredViewModel>() },
                { "PushForResultAsync<TViewModel, TResult>", nameof(UnregisteredViewModel), s => s.PushForResultAsync<UnregisteredViewModel, string>() },
                { "PopToAsync<TViewModel>", nameof(UnregisteredViewModel), s => s.PopToAsync<UnregisteredViewModel>() },
                { "PopToAsync(name)", "NoSuchPage", s => s.PopToAsync("NoSuchPage") },
                { "SelectTabAsync<TViewModel>", nameof(UnregisteredViewModel), s => s.SelectTabAsync<UnregisteredViewModel>() },
                { "SelectTabAsync(name)", "NoSuchPage", s => s.SelectTabAsync("NoSuchPage") },
            };

        [Theory]
        [MemberData(nameof(UnregisteredTargetNavigations))]
        public async Task ShouldReportUnregisteredTarget_ThroughTheTaskAndTheFailureEvent(
            string operation, string expectedPageName, Func<INavigationService, Task> navigate)
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            var failures = new List<NavigationFailedEventArgs>();
            fixture.NavigationService.NavigationFailed += (_, e) => failures.Add(e);

            // Act - the call itself must not throw; the target is resolved inside the pipeline
            var navigation = navigate(fixture.NavigationService);
            Func<Task> awaiting = () => navigation;

            // Assert
            await awaiting.Should().ThrowAsync<PageResolveException>($"{operation} names a target which is not registered");

            failures.Should().ContainSingle($"{operation} must report the failure like any other navigation failure")
                .Which.Request.PageName.Should().Be(expectedPageName);
            failures[0].Exception.Should().BeOfType<PageResolveException>();
        }

        [Fact]
        public async Task ShouldReportTheResolvedPageName_WhenNavigatingByViewModelType()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            var navigatingRequests = new List<NavigationInfo>();
            fixture.NavigationService.Navigating += (_, e) => navigatingRequests.Add(e.Request);

            // Act
            await fixture.NavigationService.PushAsync<DetailViewModel>();

            // Assert
            navigatingRequests.Should().ContainSingle()
                .Which.PageName.Should().Be("DetailPage", "the request reports the navigation name the type resolved to");
        }

        private sealed class UnregisteredViewModel
        {
        }


        [Fact]
        public async Task ShouldSurfaceTheCallbackException_WhenATypedInitializeThrowsBeforeItsFirstAwait()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act - the typed callback is reached through reflection
            Func<Task> action = () => fixture.NavigationService.PushAsync("ThrowingTypedInitializePage", new TestParameter { Value = "x" });

            // Assert
            await action.Should().ThrowExactlyAsync<InvalidOperationException>(
                "a callback which throws before its first await must surface as itself, not wrapped by reflection");
        }

        [Fact]
        public async Task ShouldCompletePushForResult_OnlyAfterThePageWasTornDown()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            var resultTask = fixture.NavigationService.PushForResultAsync<TestResult>("ResultPage");
            var resultPage = (ResultPage)fixture.NavigationService.CurrentPage!;

            // Observed at the moment the awaiting caller resumes, not afterwards.
            var scopeDisposedWhenResumed = resultTask.ContinueWith(_ => resultPage.ScopedService.IsDisposed);

            // Act
            await fixture.NavigationService.PopAsync(new TestResult { Value = "done" });

            // Assert
            (await resultTask)!.Value.Should().Be("done");
            (await scopeDisposedWhenResumed).Should().BeTrue(
                "the caller resumes only once the page's scope is gone, which is what the result source promises");
        }

        [Fact]
        public async Task ShouldNotDeliverTheResultOfAFailedPop_WithTheNextPop()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            var resultTask = fixture.NavigationService.PushForResultAsync<TestResult>("ResultPage");

            // Act - the platform refuses the pop which carries the result
            fixture.Host.FailNextPop = true;
            Func<Task> failingPop = () => fixture.NavigationService.PopAsync(new TestResult { Value = "stale" });
            await failingPop.Should().ThrowAsync<InvalidOperationException>();
            fixture.NavigationService.CurrentPage.Should().BeOfType<ResultPage>("the failed pop left the page in place");

            await fixture.NavigationService.PopAsync();
            var result = await resultTask;

            // Assert
            result.Should().BeNull("the second pop carried no result, and the result of the failed one must not be delivered in its place");
        }

        [Fact]
        public void ShouldDisposeThePagesCreatedSoFar_WhenCreateWindowFailsOnALaterSegment()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();

            // Act - the last segment does not resolve, after the first page was already created
            Action action = () => fixture.NavigationService.CreateWindow("/NavigationPage/HomePage/NoSuchPage");

            // Assert
            action.Should().Throw<PageResolveException>();
            fixture.CallLog.Entries.Should().Contain("HomeViewModel.Dispose",
                "the page tree is built outside the pipeline, and a page created before the failing segment must not leak its scope");
        }

    }
}
