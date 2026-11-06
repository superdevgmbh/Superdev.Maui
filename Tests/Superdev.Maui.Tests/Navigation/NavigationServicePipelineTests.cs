using Superdev.Maui.Navigation;
using Superdev.Maui.Tests.Navigation.TestDoubles;

namespace Superdev.Maui.Tests.Navigation
{
    /// <summary>
    /// Covers the guarantees of the pipeline which the rest of the suite takes for granted: main thread
    /// marshalling, serialization, disposal and the validation of the service container.
    /// </summary>
    public class NavigationServicePipelineTests
    {
        [Fact]
        public async Task ShouldMarshalToTheMainThread_WhenCalledFromAnotherThread()
        {
            // Arrange
            var mainThread = new TestMainThread { IsMainThread = false };
            using var fixture = NavigationServiceFixture.Create(mainThread: mainThread);

            // Act
            await fixture.SetupRootAsync();

            // Assert
            mainThread.InvokeCount.Should().BeGreaterThan(0, "a navigation which starts off the main thread has to be marshalled");
            fixture.NavigationService.CurrentPage.Should().BeOfType<HomePage>("the navigation still has to be performed");
        }

        [Fact]
        public async Task ShouldSerializeNavigations_WhichOverlap()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            var gate = fixture.GetService<NavigationGate>();
            gate.Close();

            // Act - the first navigation is held inside its initialization, the second one has to queue
            var firstNavigation = fixture.NavigationService.PushAsync("GatedPage");
            var secondNavigation = fixture.NavigationService.PushAsync("DetailPage");

            secondNavigation.IsCompleted.Should().BeFalse("the pipeline runs one navigation at a time");

            gate.Open();
            await Task.WhenAll(firstNavigation, secondNavigation);

            // Assert - the commit phases of the two navigations did not interleave. NavigatedTo runs outside
            // the navigation lock by design, so it may overlap with the next navigation's commit phase; what
            // the pipeline guarantees is that the second page is committed only after the first one was.
            var entries = fixture.CallLog.Entries;
            var gatedInitialized = entries.IndexOf($"{nameof(GatedViewModel)}.{nameof(GatedViewModel.InitializeAsync)}");
            var detailInitialized = entries.IndexOf($"{nameof(DetailViewModel)}.{nameof(DetailViewModel.InitializeAsync)}");

            gatedInitialized.Should().BeGreaterThanOrEqualTo(0);
            detailInitialized.Should().BeGreaterThan(gatedInitialized,
                "the second navigation starts only after the first one committed");

            var operations = fixture.Host.Operations;
            operations.FindIndex(o => o.StartsWith("Push(DetailPage")).Should().BeGreaterThan(
                operations.FindIndex(o => o.StartsWith("Push(GatedPage")),
                "the pages are pushed in the order the navigations were requested");

            fixture.NavigationService.CurrentPage.Should().BeOfType<DetailPage>();
        }

        [Fact]
        public async Task ShouldDisposeTheViewModelExactlyOnce_WhenAPageIsPopped()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("DetailPage");

            // Act
            await fixture.NavigationService.PopAsync();

            // Assert
            fixture.CallLog.Entries.Count(entry => entry == $"{nameof(DetailViewModel)}.Dispose")
                .Should().Be(1, "a disposal which runs twice would tear down a view model's dependencies twice");
        }

        [Fact]
        public async Task ShouldNotLeakPagesAndViewModels_OverRepeatedNavigationCycles()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            var weakReferences = new List<WeakReference>();

            // Act - several cycles, so that a leak which only accumulates is caught as well. The cycle runs
            // in its own method, so that no local of this frame keeps the last page alive.
            for (var i = 0; i < 5; i++)
            {
                var (weakPage, weakViewModel) = await PushAndPopDetailPageAsync(fixture);
                weakReferences.Add(weakPage);
                weakReferences.Add(weakViewModel);
            }

            for (var i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            // Assert
            weakReferences.Should().OnlyContain(weakReference => !weakReference.IsAlive,
                "neither the pages nor their view models may survive the navigation cycles");
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

        [Fact]
        public void ShouldBuildTheServiceProvider_WithScopeAndBuildValidation()
        {
            // Arrange + Act - the fixture builds with ValidateScopes and ValidateOnBuild enabled
            var act = () => NavigationServiceFixture.Create();

            // Assert
            act.Should().NotThrow(
                "every registered page and view model must be constructible, and no singleton may capture a scoped service");
        }

        [Fact]
        public async Task ShouldResolveThePageAndItsViewModelFromTheSameScope_AndDisposeItWithThePage()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();
            await fixture.NavigationService.PushAsync("DetailPage");

            var page = (DetailPage)fixture.NavigationService.CurrentPage!;
            var viewModel = (DetailViewModel)page.BindingContext;
            page.ScopedService.Should().BeSameAs(viewModel.ScopedService);

            // Act
            await fixture.NavigationService.PopAsync();

            // Assert
            page.ScopedService.IsDisposed.Should().BeTrue("the page scope is disposed together with the page");
        }
    }
}
