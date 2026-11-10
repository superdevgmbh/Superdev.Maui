using Superdev.Maui.Navigation.Hosts;

namespace Superdev.Maui.Tests.Navigation.TestDoubles
{
    /// <summary>
    /// An <see cref="INavigationHost"/> which maintains the navigation state in memory,
    /// so that the navigation service can be tested without a device.
    /// </summary>
    /// <remarks>
    /// Navigation stacks are delegated to the real MAUI navigation pages, which work without a handler.
    /// The window root page and the modal stack are modelled, because those require a platform window.
    /// </remarks>
    internal sealed class TestNavigationHost : INavigationHost
    {
        private readonly List<Page> modalStack = new List<Page>();

        /// <summary>
        /// Gets the log of all operations which were performed on this host.
        /// </summary>
        public List<string> Operations { get; } = new List<string>();

        public Page? RootPage { get; private set; }

        /// <summary>
        /// Makes the next pop fail, as a platform which refuses the operation would.
        /// </summary>
        public bool FailNextPop { get; set; }

        public event EventHandler<PageRemovedEventArgs>? PageRemovedExternally;

        public event EventHandler<BackRequestedEventArgs>? BackRequested;

        public event EventHandler<TabSelectedEventArgs>? TabSelectedExternally;

        public event EventHandler? ApplicationResumed;

        public event EventHandler? ApplicationStopped;

        public Task SetRootPageAsync(Page page)
        {
            this.RootPage = page;
            this.Operations.Add($"SetRootPage({page.GetType().Name})");
            return Task.CompletedTask;
        }

        public Page? GetActivePage()
        {
            var page = this.modalStack.LastOrDefault() ?? this.RootPage;
            return page == null ? null : GetLeafPage(page);
        }

        public Page? GetActiveNavigationSurface()
        {
            // A popup page is an overlay rather than a navigation context: push and pop operations
            // apply to the navigation stack underneath it.
            var modalPage = this.modalStack.LastOrDefault(p => p is not Superdev.Maui.Controls.PopupPage);
            var page = modalPage ?? this.RootPage;
            return page == null ? null : GetNavigationSurface(page);
        }

        public IReadOnlyList<Page> GetNavigationStack(Page navigationSurface)
        {
            return navigationSurface is NavigationPage navigationPage
                ? navigationPage.Navigation.NavigationStack.ToArray()
                : Array.Empty<Page>();
        }

        public IReadOnlyList<Page> GetModalStack()
        {
            return this.modalStack.ToArray();
        }

        public async Task PushAsync(Page navigationSurface, Page page, bool animated)
        {
            this.Operations.Add($"Push({page.GetType().Name}, animated: {animated})");
            await navigationSurface.Navigation.PushAsync(page, animated);
        }

        public async Task PopAsync(Page navigationSurface, bool animated)
        {
            this.Operations.Add($"Pop(animated: {animated})");

            if (this.FailNextPop)
            {
                this.FailNextPop = false;
                throw new InvalidOperationException("The platform refused to pop the page");
            }

            await navigationSurface.Navigation.PopAsync(animated);
        }

        public async Task PopToRootAsync(Page navigationSurface, bool animated)
        {
            this.Operations.Add($"PopToRoot(animated: {animated})");
            await navigationSurface.Navigation.PopToRootAsync(animated);
        }

        public void RemovePage(Page navigationSurface, Page page)
        {
            this.Operations.Add($"RemovePage({page.GetType().Name})");
            navigationSurface.Navigation.RemovePage(page);
        }

        public Task PushModalAsync(Page page, bool animated)
        {
            this.Operations.Add($"PushModal({page.GetType().Name}, animated: {animated})");
            this.modalStack.Add(page);
            return Task.CompletedTask;
        }

        public Task PopModalAsync(bool animated)
        {
            this.Operations.Add($"PopModal(animated: {animated})");

            if (this.modalStack.Count > 0)
            {
                this.modalStack.RemoveAt(this.modalStack.Count - 1);
            }

            return Task.CompletedTask;
        }

        public void SetFlyoutDetail(FlyoutPage flyoutPage, Page detail)
        {
            this.Operations.Add($"SetFlyoutDetail({detail.GetType().Name})");
            flyoutPage.Detail = detail;
        }

        public void SelectTab(TabbedPage tabbedPage, Page tab)
        {
            this.Operations.Add($"SelectTab({tab.GetType().Name})");
            tabbedPage.CurrentPage = tab;
        }

        /// <summary>
        /// Simulates a tab which the user selected on the tab bar.
        /// </summary>
        public void RaiseTabSelectedExternally(TabbedPage tabbedPage, Page tab)
        {
            var previousTab = tabbedPage.CurrentPage;
            tabbedPage.CurrentPage = tab;

            this.TabSelectedExternally?.Invoke(
                this, new TabSelectedEventArgs { TabbedPage = tabbedPage, PreviousTab = previousTab });
        }

        /// <summary>
        /// Simulates a page which was removed without the navigation service being involved,
        /// e.g. by a hardware back navigation or an iOS swipe back gesture.
        /// </summary>
        public void RaisePageRemovedExternally(Page page, PageRemovalKind kind)
        {
            if (kind == PageRemovalKind.ModalPopped)
            {
                this.modalStack.Remove(page);
            }
            else if (this.GetActiveNavigationSurface() is NavigationPage navigationPage)
            {
                navigationPage.Navigation.RemovePage(page);
            }

            this.PageRemovedExternally?.Invoke(this, new PageRemovedEventArgs { Page = page, Kind = kind });
        }

        /// <summary>
        /// Simulates an interceptable back navigation and reports whether the navigation service took over.
        /// </summary>
        public bool RequestBack(Page page)
        {
            this.Operations.Add($"RequestBack({page.GetType().Name})");

            var eventArgs = new BackRequestedEventArgs { Page = page };
            this.BackRequested?.Invoke(this, eventArgs);
            return eventArgs.Cancel;
        }

        /// <inheritdoc cref="RequestBack"/>
        public bool RaiseBackRequested(Page page)
        {
            return this.RequestBack(page);
        }

        /// <summary>
        /// Simulates the application being resumed or sent to the background.
        /// </summary>
        public void RaiseApplicationLifecycle(bool resumed)
        {
            if (resumed)
            {
                this.ApplicationResumed?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                this.ApplicationStopped?.Invoke(this, EventArgs.Empty);
            }
        }

        private static Page GetLeafPage(Page page)
        {
            while (true)
            {
                var childPage = GetChildPage(page);
                if (childPage == null || ReferenceEquals(childPage, page))
                {
                    return page;
                }

                page = childPage;
            }
        }

        private static Page? GetNavigationSurface(Page page)
        {
            Page? navigationSurface = null;

            while (true)
            {
                if (page is NavigationPage)
                {
                    navigationSurface = page;
                }

                var childPage = GetChildPage(page);
                if (childPage == null || ReferenceEquals(childPage, page))
                {
                    return navigationSurface;
                }

                page = childPage;
            }
        }

        private static Page? GetChildPage(Page page)
        {
            return page switch
            {
                FlyoutPage flyoutPage => flyoutPage.Detail,
                TabbedPage tabbedPage => tabbedPage.CurrentPage,
                NavigationPage navigationPage => navigationPage.CurrentPage,
                _ => null
            };
        }
    }
}
