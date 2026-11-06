using Microsoft.Extensions.Logging;

namespace Superdev.Maui.Navigation.Hosts
{
    /// <inheritdoc cref="INavigationHost"/>
    public class MauiNavigationHost : INavigationHost
    {
        private readonly ILogger logger;
        private readonly NavigationOptions navigationOptions;

        /// <summary>
        /// The pages this host is about to remove itself. Removal events for these pages are not reported
        /// as external removals. The navigation service serializes all operations, so no synchronization is needed.
        /// </summary>
        private readonly HashSet<Page> pendingRemovals = new HashSet<Page>();

        private readonly HashSet<NavigationPage> subscribedNavigationPages = new HashSet<NavigationPage>();

        private readonly Dictionary<TabbedPage, Page?> subscribedTabbedPages = new Dictionary<TabbedPage, Page?>();

        /// <summary>
        /// The tabbed page whose tab this host is about to select itself.
        /// </summary>
        private TabbedPage? pendingTabSelection;

        private Window? subscribedWindow;

        private Page? subscribedRootPage;

        /// <inheritdoc cref="MauiNavigationHost"/>
        public MauiNavigationHost(ILogger<MauiNavigationHost> logger, NavigationOptions navigationOptions)
        {
            this.logger = logger;
            this.navigationOptions = navigationOptions;
        }

        public event EventHandler<PageRemovedEventArgs>? PageRemovedExternally;

        public event EventHandler<BackRequestedEventArgs>? BackRequested;

        public event EventHandler<TabSelectedEventArgs>? TabSelectedExternally;

        public event EventHandler? ApplicationResumed;

        public event EventHandler? ApplicationStopped;

        public Page? RootPage
        {
            get
            {
                var window = this.TryGetWindow();
                if (window == null)
                {
                    return null;
                }

                this.EnsureSubscribed(window);
                this.EnsureRootPageSubscribed(window.Page);

                return window.Page;
            }
        }

        public Task SetRootPageAsync(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);

            var window = this.GetWindow();

            window.Page = page;

            this.EnsureSubscribed(window);
            this.EnsureRootPageSubscribed(page);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Subscribes to the root page of the window, whoever assigned it.
        /// </summary>
        /// <remarks>
        /// A window built by <see cref="INavigationService.CreateWindow" />, or one an application creates
        /// itself, never passes through <see cref="SetRootPageAsync" />. Without this, the removal events of
        /// its navigation pages would never be observed, so a page removed by the platform - the iOS back
        /// button or its swipe back gesture - would neither receive its lifecycle callbacks nor have its
        /// service scope disposed.
        /// </remarks>
        private void EnsureRootPageSubscribed(Page? rootPage)
        {
            if (ReferenceEquals(this.subscribedRootPage, rootPage))
            {
                return;
            }

            if (this.subscribedRootPage is { } previousRootPage)
            {
                this.Unsubscribe(previousRootPage);
            }

            this.subscribedRootPage = rootPage;

            if (rootPage != null)
            {
                this.Subscribe(rootPage);
            }
        }

        public Page? GetActivePage()
        {
            var modalPage = this.GetModalStack().LastOrDefault();
            var page = modalPage ?? this.RootPage;
            return page == null ? null : GetLeafPage(page);
        }

        public Page? GetActiveNavigationSurface()
        {
            // A popup page is an overlay rather than a navigation context: push and pop operations
            // apply to the navigation stack underneath it.
            var modalPage = this.GetModalStack().LastOrDefault(p => p is not Superdev.Maui.Controls.PopupPage);
            var page = modalPage ?? this.RootPage;
            return page == null ? null : GetNavigationSurface(page);
        }

        public IReadOnlyList<Page> GetNavigationStack(Page navigationSurface)
        {
            ArgumentNullException.ThrowIfNull(navigationSurface);

            return navigationSurface is NavigationPage navigationPage
                ? navigationPage.Navigation.NavigationStack.ToArray()
                : Array.Empty<Page>();
        }

        public IReadOnlyList<Page> GetModalStack()
        {
            var rootPage = this.RootPage;
            return rootPage == null
                ? Array.Empty<Page>()
                : rootPage.Navigation.ModalStack.ToArray();
        }

        public async Task PushAsync(Page navigationSurface, Page page, bool animated)
        {
            ArgumentNullException.ThrowIfNull(navigationSurface);
            ArgumentNullException.ThrowIfNull(page);

            await navigationSurface.Navigation.PushAsync(page, animated);

            // A pushed page may itself be a tabbed page, whose tab changes have to be reported as well.
            this.Subscribe(page);
        }

        public async Task PopAsync(Page navigationSurface, bool animated)
        {
            ArgumentNullException.ThrowIfNull(navigationSurface);

            var poppedPage = this.GetNavigationStack(navigationSurface).LastOrDefault();

            using (this.SuppressRemovalEvents(poppedPage))
            {
                await navigationSurface.Navigation.PopAsync(animated);
            }

            if (poppedPage != null)
            {
                this.Unsubscribe(poppedPage);
            }
        }

        public async Task PopToRootAsync(Page navigationSurface, bool animated)
        {
            ArgumentNullException.ThrowIfNull(navigationSurface);

            var removedPages = this.GetNavigationStack(navigationSurface).Skip(1).ToArray();

            using (this.SuppressRemovalEvents(removedPages))
            {
                await navigationSurface.Navigation.PopToRootAsync(animated);
            }

            foreach (var removedPage in removedPages)
            {
                this.Unsubscribe(removedPage);
            }
        }

        public void RemovePage(Page navigationSurface, Page page)
        {
            ArgumentNullException.ThrowIfNull(navigationSurface);
            ArgumentNullException.ThrowIfNull(page);

            using (this.SuppressRemovalEvents(page))
            {
                navigationSurface.Navigation.RemovePage(page);
            }

            this.Unsubscribe(page);
        }

        public async Task PushModalAsync(Page page, bool animated)
        {
            ArgumentNullException.ThrowIfNull(page);

            var rootPage = this.RootPage ?? throw new NavigationException("Failed to resolve the root page");

            await rootPage.Navigation.PushModalAsync(page, animated);
            this.Subscribe(page);
        }

        public async Task PopModalAsync(bool animated)
        {
            var rootPage = this.RootPage ?? throw new NavigationException("Failed to resolve the root page");

            var modalPage = this.GetModalStack().LastOrDefault();

            using (this.SuppressRemovalEvents(modalPage))
            {
                await rootPage.Navigation.PopModalAsync(animated);
            }

            if (modalPage != null)
            {
                this.Unsubscribe(modalPage);
            }
        }

        public void SetFlyoutDetail(FlyoutPage flyoutPage, Page detail)
        {
            ArgumentNullException.ThrowIfNull(flyoutPage);
            ArgumentNullException.ThrowIfNull(detail);

            if (flyoutPage.Detail is { } previousDetail)
            {
                this.Unsubscribe(previousDetail);
            }

            flyoutPage.Detail = detail;

            if (this.navigationOptions.CloseFlyoutAfterNavigation)
            {
                flyoutPage.IsPresented = false;
            }

            this.Subscribe(detail);
        }

        public void SelectTab(TabbedPage tabbedPage, Page tab)
        {
            ArgumentNullException.ThrowIfNull(tabbedPage);
            ArgumentNullException.ThrowIfNull(tab);

            if (!tabbedPage.Children.Contains(tab))
            {
                throw new NavigationException("The given page is not a tab of the given tabbed page");
            }

            this.pendingTabSelection = tabbedPage;
            try
            {
                tabbedPage.CurrentPage = tab;
                this.subscribedTabbedPages[tabbedPage] = tab;
            }
            finally
            {
                this.pendingTabSelection = null;
            }
        }

        /// <summary>
        /// Gets the window this host operates on. Multiple windows are not supported;
        /// override <see cref="TryGetWindow"/> to select a different window.
        /// </summary>
        protected Window GetWindow()
        {
            return this.TryGetWindow() ?? throw new NavigationException("Failed to resolve the application window");
        }

        /// <summary>
        /// Gets the window this host operates on, or <c>null</c> if the application has no window yet.
        /// </summary>
        protected virtual Window? TryGetWindow()
        {
            return Application.Current?.Windows.ElementAtOrDefault(0);
        }

        /// <summary>
        /// Unwraps flyout, tabbed and navigation pages to find the page the user actually sees.
        /// </summary>
        private static Page GetLeafPage(Page page)
        {
            while (true)
            {
                var childPage = page switch
                {
                    FlyoutPage flyoutPage => flyoutPage.Detail,
                    TabbedPage tabbedPage => tabbedPage.CurrentPage,
                    NavigationPage navigationPage => navigationPage.CurrentPage,
                    _ => null
                };

                if (childPage == null || ReferenceEquals(childPage, page))
                {
                    return page;
                }

                page = childPage;
            }
        }

        /// <summary>
        /// Finds the innermost navigation page of the currently active branch.
        /// </summary>
        private static Page? GetNavigationSurface(Page page)
        {
            Page? navigationSurface = null;

            while (true)
            {
                if (page is NavigationPage)
                {
                    navigationSurface = page;
                }

                var childPage = page switch
                {
                    FlyoutPage flyoutPage => flyoutPage.Detail,
                    TabbedPage tabbedPage => tabbedPage.CurrentPage,
                    NavigationPage navigationPage => navigationPage.CurrentPage,
                    _ => null
                };

                if (childPage == null || ReferenceEquals(childPage, page))
                {
                    return navigationSurface;
                }

                page = childPage;
            }
        }

        private IDisposable SuppressRemovalEvents(params Page?[] pages)
        {
            var suppressedPages = pages.Where(p => p != null).Select(p => p!).ToArray();

            foreach (var page in suppressedPages)
            {
                this.pendingRemovals.Add(page);
            }

            return new RemovalSuppression(this, suppressedPages);
        }

        private void EnsureSubscribed(Window window)
        {
            if (ReferenceEquals(this.subscribedWindow, window))
            {
                return;
            }

            if (this.subscribedWindow is { } previousWindow)
            {
                previousWindow.ModalPopped -= this.OnModalPopped;
                previousWindow.ModalPopping -= this.OnModalPopping;
                previousWindow.Resumed -= this.OnApplicationResumed;
                previousWindow.Stopped -= this.OnApplicationStopped;
            }

            window.ModalPopped += this.OnModalPopped;
            window.ModalPopping += this.OnModalPopping;
            window.Resumed += this.OnApplicationResumed;
            window.Stopped += this.OnApplicationStopped;
            this.subscribedWindow = window;
        }

        /// <summary>
        /// Subscribes to the removal events of all navigation pages within the given page tree.
        /// </summary>
        private void Subscribe(Page page)
        {
            foreach (var navigationPage in EnumerateNavigationPages(page))
            {
                if (this.subscribedNavigationPages.Add(navigationPage))
                {
                    navigationPage.Popped += this.OnPopped;
                    navigationPage.PoppedToRoot += this.OnPoppedToRoot;
                }
            }

            foreach (var tabbedPage in EnumerateTabbedPages(page))
            {
                if (this.subscribedTabbedPages.TryAdd(tabbedPage, tabbedPage.CurrentPage))
                {
                    tabbedPage.CurrentPageChanged += this.OnCurrentPageChanged;
                }
            }
        }

        /// <summary>
        /// Enumerates the tabbed pages of the given page tree.
        /// </summary>
        private static IEnumerable<TabbedPage> EnumerateTabbedPages(Page page)
        {
            switch (page)
            {
                case TabbedPage tabbedPage:
                    yield return tabbedPage;
                    break;

                case FlyoutPage { Detail: { } detail }:
                    foreach (var nested in EnumerateTabbedPages(detail))
                    {
                        yield return nested;
                    }

                    break;

                case NavigationPage navigationPage:
                    foreach (var stackPage in navigationPage.Navigation.NavigationStack.ToArray())
                    {
                        if (stackPage != null && !ReferenceEquals(stackPage, page))
                        {
                            foreach (var nested in EnumerateTabbedPages(stackPage))
                            {
                                yield return nested;
                            }
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Reports a tab which the user selected, so that the navigation service can run the
        /// lifecycle callbacks of the tab children.
        /// </summary>
        private void OnCurrentPageChanged(object? sender, EventArgs e)
        {
            if (sender is not TabbedPage tabbedPage || ReferenceEquals(this.pendingTabSelection, tabbedPage))
            {
                return;
            }

            this.subscribedTabbedPages.TryGetValue(tabbedPage, out var previousTab);
            this.subscribedTabbedPages[tabbedPage] = tabbedPage.CurrentPage;

            if (ReferenceEquals(previousTab, tabbedPage.CurrentPage))
            {
                return;
            }

            this.logger.LogDebug(
                "Tab {TabType} of {TabbedPageType} was selected externally (previous: {PreviousTabType})",
                tabbedPage.CurrentPage?.GetType().Name, tabbedPage.GetType().Name, previousTab?.GetType().Name);

            this.TabSelectedExternally?.Invoke(
                this, new TabSelectedEventArgs { TabbedPage = tabbedPage, PreviousTab = previousTab });
        }

        private void Unsubscribe(Page page)
        {
            foreach (var navigationPage in EnumerateNavigationPages(page))
            {
                if (this.subscribedNavigationPages.Remove(navigationPage))
                {
                    navigationPage.Popped -= this.OnPopped;
                    navigationPage.PoppedToRoot -= this.OnPoppedToRoot;
                }
            }

            foreach (var tabbedPage in EnumerateTabbedPages(page))
            {
                if (this.subscribedTabbedPages.Remove(tabbedPage))
                {
                    tabbedPage.CurrentPageChanged -= this.OnCurrentPageChanged;
                }
            }
        }

        private static IEnumerable<NavigationPage> EnumerateNavigationPages(Page page)
        {
            if (page is NavigationPage navigationPage)
            {
                yield return navigationPage;

                foreach (var stackPage in navigationPage.Navigation.NavigationStack.ToArray())
                {
                    if (stackPage != null && !ReferenceEquals(stackPage, page))
                    {
                        foreach (var nested in EnumerateNavigationPages(stackPage))
                        {
                            yield return nested;
                        }
                    }
                }
            }
            else if (page is FlyoutPage flyoutPage)
            {
                if (flyoutPage.Detail is { } detail)
                {
                    foreach (var nested in EnumerateNavigationPages(detail))
                    {
                        yield return nested;
                    }
                }
            }
            else if (page is TabbedPage tabbedPage)
            {
                foreach (var child in tabbedPage.Children.ToArray())
                {
                    foreach (var nested in EnumerateNavigationPages(child))
                    {
                        yield return nested;
                    }
                }
            }
        }

        private void OnPopped(object? sender, NavigationEventArgs e)
        {
            this.RaisePageRemovedExternally(e.Page, PageRemovalKind.Popped);
        }

        private void OnPoppedToRoot(object? sender, NavigationEventArgs e)
        {
            if (e is PoppedToRootEventArgs poppedToRootEventArgs)
            {
                foreach (var page in poppedToRootEventArgs.PoppedPages)
                {
                    this.RaisePageRemovedExternally(page, PageRemovalKind.PoppedToRoot);
                }
            }
        }

        private void OnModalPopped(object? sender, ModalPoppedEventArgs e)
        {
            this.RaisePageRemovedExternally(e.Modal, PageRemovalKind.ModalPopped);
        }

        /// <summary>
        /// A modal page which the user dismisses, e.g. by dragging an iOS sheet down, can still be prevented.
        /// This is what allows a navigation guard to veto such a dismissal.
        /// </summary>
        private void OnModalPopping(object? sender, ModalPoppingEventArgs e)
        {
            if (this.pendingRemovals.Contains(e.Modal))
            {
                return;
            }

            var eventArgs = new BackRequestedEventArgs { Page = e.Modal };
            this.BackRequested?.Invoke(this, eventArgs);
            e.Cancel = eventArgs.Cancel;
        }

        private void OnApplicationResumed(object? sender, EventArgs e)
        {
            this.ApplicationResumed?.Invoke(this, EventArgs.Empty);
        }

        private void OnApplicationStopped(object? sender, EventArgs e)
        {
            this.ApplicationStopped?.Invoke(this, EventArgs.Empty);
        }

        private void RaisePageRemovedExternally(Page? page, PageRemovalKind kind)
        {
            if (page == null || this.pendingRemovals.Contains(page))
            {
                return;
            }

            this.logger.LogDebug(
                "Page {PageType} was removed externally ({Kind})", page.GetType().Name, kind);

            this.PageRemovedExternally?.Invoke(this, new PageRemovedEventArgs { Page = page, Kind = kind });
        }

        public bool RequestBack(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);

            var eventArgs = new BackRequestedEventArgs { Page = page };
            this.BackRequested?.Invoke(this, eventArgs);
            return eventArgs.Cancel;
        }

        private sealed class RemovalSuppression : IDisposable
        {
            private readonly MauiNavigationHost navigationHost;
            private readonly IReadOnlyList<Page> pages;

            public RemovalSuppression(MauiNavigationHost navigationHost, IReadOnlyList<Page> pages)
            {
                this.navigationHost = navigationHost;
                this.pages = pages;
            }

            public void Dispose()
            {
                foreach (var page in this.pages)
                {
                    this.navigationHost.pendingRemovals.Remove(page);
                }
            }
        }
    }
}
