using Microsoft.Extensions.Logging.Abstractions;
using Superdev.Maui.Controls;
using Superdev.Maui.Navigation;
using Superdev.Maui.Navigation.Hosts;
using Superdev.Maui.Tests.Navigation.TestDoubles;

namespace Superdev.Maui.Tests.Navigation
{
    /// <summary>
    /// Covers the real host against real MAUI pages. The navigation service is only ever as correct as the
    /// page tree walk below it, and that walk exists twice: here and - mirrored - in <see cref="TestNavigationHost"/>.
    /// </summary>
    /// <remarks>
    /// Reporting of external removals is not covered here: MAUI raises <see cref="NavigationPage.Popped"/> from
    /// the platform side, so a pop without a handler raises nothing at all and any such test would pass
    /// vacuously. That path is verified on device.
    /// </remarks>
    public class MauiNavigationHostTests
    {
        public MauiNavigationHostTests()
        {
            TestDispatcherProvider.EnsureInstalled();
        }

        [Fact]
        public async Task ShouldResolveActivePage_ThroughFlyoutAndTabAndNavigationPage()
        {
            // Arrange
            var expectedPage = new ContentPage();
            var tabbedPage = new TabbedPage();
            tabbedPage.Children.Add(new NavigationPage(new ContentPage()));
            tabbedPage.Children.Add(new NavigationPage(expectedPage));

            var flyoutPage = new FlyoutPage
            {
                Flyout = new ContentPage { Title = "Flyout" },
                Detail = tabbedPage
            };

            var host = CreateHost(flyoutPage);
            await host.SetRootPageAsync(flyoutPage);
            host.SelectTab(tabbedPage, tabbedPage.Children[1]);

            // Act + Assert
            host.GetActivePage().Should().BeSameAs(expectedPage);
            host.GetActiveNavigationSurface().Should().BeSameAs(tabbedPage.Children[1]);
        }

        [Fact]
        public async Task ShouldResolveActivePage_FromTheModalStack()
        {
            // Arrange
            var host = CreateHost(out var navigationPage);
            var modalContent = new ContentPage();

            // Act
            await host.PushModalAsync(new NavigationPage(modalContent), animated: false);

            // Assert
            host.GetActivePage().Should().BeSameAs(modalContent, "a modal page covers the root page");
            host.GetActiveNavigationSurface().Should().BeOfType<NavigationPage>()
                .And.NotBeSameAs(navigationPage, "pushes inside a modal apply to the modal's own stack");
        }

        [Fact]
        public async Task ShouldResolveTheNavigationSurfaceBeneathAPopup()
        {
            // Arrange
            var host = CreateHost(out var navigationPage);
            var popupPage = new PopupPage();

            // Act
            await host.PushModalAsync(popupPage, animated: false);

            // Assert
            host.GetActivePage().Should().BeSameAs(popupPage, "the popup is what the user sees");
            host.GetActiveNavigationSurface().Should().BeSameAs(
                navigationPage, "a popup is an overlay, so push and pop apply to the stack underneath it");
        }

        [Fact]
        public async Task ShouldNotReportTabSelection_WhichTheHostPerformedItself()
        {
            // Arrange
            var tabbedPage = CreateTabbedPage();
            var host = CreateHost(tabbedPage);
            await host.SetRootPageAsync(tabbedPage);

            var reported = new List<TabSelectedEventArgs>();
            host.TabSelectedExternally += (_, e) => reported.Add(e);

            // Act
            host.SelectTab(tabbedPage, tabbedPage.Children[1]);

            // Assert
            reported.Should().BeEmpty();
            tabbedPage.CurrentPage.Should().BeSameAs(tabbedPage.Children[1]);
        }

        [Fact]
        public async Task ShouldReportTabSelection_WhichTheUserPerformed()
        {
            // Arrange
            var tabbedPage = CreateTabbedPage();
            var host = CreateHost(tabbedPage);
            await host.SetRootPageAsync(tabbedPage);

            var reported = new List<TabSelectedEventArgs>();
            host.TabSelectedExternally += (_, e) => reported.Add(e);

            // Act - as a tap on the tab bar would
            tabbedPage.CurrentPage = tabbedPage.Children[1];

            // Assert
            reported.Should().HaveCount(1);
            reported[0].TabbedPage.Should().BeSameAs(tabbedPage);
            reported[0].PreviousTab.Should().BeSameAs(tabbedPage.Children[0]);
        }

        [Fact]
        public async Task ShouldReportTabSelection_AgainAfterTheHostSelectedATab()
        {
            // Arrange - the suppression must not outlive the operation which caused it
            var tabbedPage = CreateTabbedPage();
            var host = CreateHost(tabbedPage);
            await host.SetRootPageAsync(tabbedPage);
            host.SelectTab(tabbedPage, tabbedPage.Children[1]);

            var reported = new List<TabSelectedEventArgs>();
            host.TabSelectedExternally += (_, e) => reported.Add(e);

            // Act
            tabbedPage.CurrentPage = tabbedPage.Children[0];

            // Assert
            reported.Should().HaveCount(1);
        }

        [Fact]
        public void ShouldRejectATab_WhichDoesNotBelongToTheTabbedPage()
        {
            // Arrange
            var tabbedPage = CreateTabbedPage();
            var host = CreateHost(tabbedPage);

            // Act
            var action = () => host.SelectTab(tabbedPage, new ContentPage());

            // Assert
            action.Should().Throw<NavigationException>();
        }

        [Fact]
        public async Task ShouldReportBackRequests_AndReturnWhetherTheyWereHandled()
        {
            // Arrange
            var host = CreateHost(out var navigationPage);
            var reported = new List<Page>();
            host.BackRequested += (_, e) =>
            {
                reported.Add(e.Page);
                e.Cancel = true;
            };

            // Act
            var handled = host.RequestBack(navigationPage);

            // Assert
            handled.Should().BeTrue("a cancelled back request means the host must not let the platform pop");
            reported.Should().ContainSingle().Which.Should().BeSameAs(navigationPage);
            await Task.CompletedTask;
        }

        [Fact]
        public void ShouldNotHandleBackRequests_WhichNobodyListensTo()
        {
            // Arrange
            var host = CreateHost(out var navigationPage);

            // Act
            var handled = host.RequestBack(navigationPage);

            // Assert
            handled.Should().BeFalse("without a listener the platform must keep its default back behavior");
        }

        [Fact]
        public async Task ShouldReplaceTheFlyoutDetail_AndCloseTheFlyout()
        {
            // Arrange
            var flyoutPage = new FlyoutPage
            {
                Flyout = new ContentPage { Title = "Flyout" },
                Detail = new NavigationPage(new ContentPage())
            };

            var host = CreateHost(flyoutPage);
            await host.SetRootPageAsync(flyoutPage);
            flyoutPage.IsPresented = true;

            var newDetailContent = new ContentPage();

            // Act
            host.SetFlyoutDetail(flyoutPage, new NavigationPage(newDetailContent));

            // Assert
            host.GetActivePage().Should().BeSameAs(newDetailContent);
            flyoutPage.IsPresented.Should().BeFalse("selecting a menu entry closes the flyout");
        }

        [Fact]
        public async Task ShouldReportTheNavigationStack_OfTheGivenSurfaceOnly()
        {
            // Arrange
            var host = CreateHost(out var navigationPage);
            var pushedPage = new ContentPage();

            // Act
            await host.PushAsync(navigationPage, pushedPage, animated: false);

            // Assert
            host.GetNavigationStack(navigationPage).Should().HaveCount(2).And.EndWith(pushedPage);
            host.GetNavigationStack(new ContentPage()).Should().BeEmpty("a content page carries no stack");
        }

        [Fact]
        public async Task ShouldPopToRoot()
        {
            // Arrange
            var host = CreateHost(out var navigationPage);
            await host.PushAsync(navigationPage, new ContentPage(), animated: false);
            await host.PushAsync(navigationPage, new ContentPage(), animated: false);

            // Act
            await host.PopToRootAsync(navigationPage, animated: false);

            // Assert
            host.GetNavigationStack(navigationPage).Should().HaveCount(1);
        }

        private static TabbedPage CreateTabbedPage()
        {
            var tabbedPage = new TabbedPage();
            tabbedPage.Children.Add(new ContentPage());
            tabbedPage.Children.Add(new ContentPage());
            return tabbedPage;
        }

        private static TestableMauiNavigationHost CreateHost(out NavigationPage navigationPage)
        {
            navigationPage = new NavigationPage(new ContentPage());
            var host = CreateHost((Page)navigationPage);
            host.SetRootPageAsync(navigationPage).GetAwaiter().GetResult();
            return host;
        }

        private static TestableMauiNavigationHost CreateHost(Page rootPage)
        {
            return new TestableMauiNavigationHost(new Window(rootPage));
        }

        /// <summary>
        /// Uses a window the test owns, because a unit test process has no platform window.
        /// </summary>
        private sealed class TestableMauiNavigationHost : MauiNavigationHost
        {
            private readonly Window window;

            public TestableMauiNavigationHost(Window window)
                : base(NullLogger<MauiNavigationHost>.Instance, new NavigationOptions())
            {
                this.window = window;
            }

            protected override Window? TryGetWindow()
            {
                return this.window;
            }
        }
    }
}
