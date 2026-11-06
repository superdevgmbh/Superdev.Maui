using Superdev.Maui.Navigation;
using Superdev.Maui.Tests.Navigation.TestDoubles;

namespace Superdev.Maui.Tests.Navigation
{
    /// <summary>
    /// Covers tabbed pages: selecting a statically declared tab and reacting to a tab the user selected.
    /// </summary>
    public class NavigationServiceTabTests
    {
        [Fact]
        public async Task ShouldSelectTabByPageName()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.NavigationService.NavigateAsync("/StaticTabbedPage");
            fixture.CallLog.Entries.Clear();

            // Act
            await fixture.NavigationService.SelectTabAsync("DetailPage");

            // Assert
            var tabbedPage = (StaticTabbedPage)fixture.Host.RootPage!;
            tabbedPage.CurrentPage.Should().BeOfType<NavigationPage>();
            fixture.NavigationService.CurrentPage.Should().BeOfType<DetailPage>();
            fixture.Host.Operations.Should().Contain(o => o.StartsWith("SelectTab"));
        }

        [Fact]
        public async Task ShouldSelectTabByViewModelType()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.NavigationService.NavigateAsync("/StaticTabbedPage");

            // Act
            await fixture.NavigationService.SelectTabAsync<DetailViewModel>();

            // Assert
            fixture.NavigationService.CurrentPage.Should().BeOfType<DetailPage>();
        }

        [Fact]
        public async Task ShouldDispatchLifecycleToTheActiveTabChildOnly()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.NavigationService.NavigateAsync("/StaticTabbedPage");
            fixture.CallLog.Entries.Clear();

            // Act
            await fixture.NavigationService.SelectTabAsync("DetailPage");

            // Assert
            fixture.CallLog.Entries.Should().ContainInOrder(
                "HomeViewModel.NavigatedFromAsync(Forward)",
                "DetailViewModel.InitializeAsync",
                "DetailViewModel.NavigatedToAsync(Forward)");
        }

        [Fact]
        public async Task ShouldInitializeTabChildOnlyOnce()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.NavigationService.NavigateAsync("/StaticTabbedPage");
            await fixture.NavigationService.SelectTabAsync("DetailPage");
            await fixture.NavigationService.SelectTabAsync("HomePage");
            fixture.CallLog.Entries.Clear();

            // Act
            await fixture.NavigationService.SelectTabAsync("DetailPage");

            // Assert
            fixture.CallLog.Entries.Should().NotContain("DetailViewModel.InitializeAsync");
            fixture.CallLog.Entries.Should().Contain("DetailViewModel.NavigatedToAsync(Forward)");
        }

        [Fact]
        public async Task ShouldThrow_WhenNoTabHostsThePage()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.NavigationService.NavigateAsync("/StaticTabbedPage");

            // Act
            Func<Task> action = () => fixture.NavigationService.SelectTabAsync("ResultPage");

            // Assert
            await action.Should().ThrowAsync<NavigationException>().WithMessage("*No tab*");
        }

        [Fact]
        public async Task ShouldThrow_WhenNoTabbedPageIsActive()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.SetupRootAsync();

            // Act
            Func<Task> action = () => fixture.NavigationService.SelectTabAsync("DetailPage");

            // Assert
            await action.Should().ThrowAsync<NavigationException>().WithMessage("*no tabbed page*");
        }

        [Fact]
        public async Task ShouldInitializeTabChild_BeforeTheTabIsShown()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create();
            await fixture.NavigationService.NavigateAsync("/StaticTabbedPage");
            var tabbedPage = (StaticTabbedPage)fixture.Host.RootPage!;

            string[]? entriesWhenTabChanged = null;
            tabbedPage.CurrentPageChanged += (_, _) => entriesWhenTabChanged = fixture.CallLog.Entries.ToArray();

            // Act
            await fixture.NavigationService.SelectTabAsync("DetailPage");

            // Assert
            entriesWhenTabChanged.Should().NotBeNull()
                .And.Contain("DetailViewModel.InitializeAsync", "initialization runs before the page becomes visible, as the callback promises");
        }

    }
}
