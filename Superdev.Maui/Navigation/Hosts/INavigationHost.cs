namespace Superdev.Maui.Navigation.Hosts
{
    /// <summary>
    /// Abstracts the .NET MAUI navigation primitives (window root page, navigation stacks, modal stack,
    /// flyout detail page and tab selection).
    /// </summary>
    /// <remarks>
    /// This abstraction exists for two reasons: it makes <see cref="INavigationService"/> testable without a
    /// device or emulator, and it isolates the navigation host, so that a different host can be supported later
    /// without changing the navigation service.
    /// <para>
    /// All members are invoked on the UI thread; the navigation service guarantees this.
    /// </para>
    /// </remarks>
    public interface INavigationHost
    {
        /// <summary>
        /// Gets the root page of the window, or <c>null</c> if no root page is set yet.
        /// </summary>
        Page? RootPage { get; }

        /// <summary>
        /// Replaces the root page of the window.
        /// </summary>
        Task SetRootPageAsync(Page page);

        /// <summary>
        /// Gets the page the user currently sees: the topmost page of the modal stack if any,
        /// otherwise the root page, in both cases unwrapping <see cref="FlyoutPage.Detail"/>,
        /// <see cref="TabbedPage.CurrentPage"/> and <see cref="NavigationPage.CurrentPage"/> recursively.
        /// </summary>
        Page? GetActivePage();

        /// <summary>
        /// Gets the navigation surface push and pop operations apply to, i.e. the innermost
        /// <see cref="NavigationPage"/> of the currently active branch, or <c>null</c> if there is none.
        /// </summary>
        Page? GetActiveNavigationSurface();

        /// <summary>
        /// Gets the pages of the navigation stack owned by <paramref name="navigationSurface"/>, bottom to top.
        /// </summary>
        IReadOnlyList<Page> GetNavigationStack(Page navigationSurface);

        /// <summary>
        /// Gets the pages of the modal stack, bottom to top.
        /// </summary>
        IReadOnlyList<Page> GetModalStack();

        /// <summary>
        /// Pushes <paramref name="page"/> onto the navigation stack of <paramref name="navigationSurface"/>.
        /// </summary>
        Task PushAsync(Page navigationSurface, Page page, bool animated);

        /// <summary>
        /// Pops the topmost page from the navigation stack of <paramref name="navigationSurface"/>.
        /// </summary>
        Task PopAsync(Page navigationSurface, bool animated);

        /// <summary>
        /// Pops all pages except the root page from the navigation stack of <paramref name="navigationSurface"/>.
        /// </summary>
        Task PopToRootAsync(Page navigationSurface, bool animated);

        /// <summary>
        /// Removes <paramref name="page"/> from the navigation stack of <paramref name="navigationSurface"/>
        /// without a transition.
        /// </summary>
        void RemovePage(Page navigationSurface, Page page);

        /// <summary>
        /// Pushes <paramref name="page"/> onto the modal stack.
        /// </summary>
        Task PushModalAsync(Page page, bool animated);

        /// <summary>
        /// Pops the topmost page from the modal stack.
        /// </summary>
        Task PopModalAsync(bool animated);

        /// <summary>
        /// Replaces the detail page of <paramref name="flyoutPage"/>.
        /// </summary>
        void SetFlyoutDetail(FlyoutPage flyoutPage, Page detail);

        /// <summary>
        /// Selects the given <paramref name="tab"/> of <paramref name="tabbedPage"/>.
        /// </summary>
        void SelectTab(TabbedPage tabbedPage, Page tab);

        /// <summary>
        /// Occurs when a page left a navigation or modal stack for a reason this host did not cause itself,
        /// e.g. a hardware back navigation, an iOS swipe back gesture or code bypassing the navigation service.
        /// </summary>
        /// <remarks>
        /// This event is never raised for removals performed through this interface.
        /// The navigation service uses it to run the lifecycle callbacks and to dispose the page's service scope,
        /// so that no exit path leaks a page or a view model.
        /// </remarks>
        event EventHandler<PageRemovedEventArgs>? PageRemovedExternally;

        /// <summary>
        /// Reports a back navigation which the user triggered, e.g. by the Android hardware back button,
        /// so that it can run through the navigation service instead of the platform.
        /// </summary>
        /// <param name="page">The page the back navigation would remove.</param>
        /// <returns>
        /// <c>true</c> if the navigation service handles the back navigation, in which case the caller must
        /// not let the platform perform it; <c>false</c> if the platform should proceed.
        /// </returns>
        bool RequestBack(Page page);

        /// <summary>
        /// Occurs before an interceptable back navigation is performed, e.g. an Android hardware back navigation
        /// or a modal page which the user dismisses. Setting <see cref="BackRequestedEventArgs.Cancel"/> prevents
        /// the platform from performing it, which is how the navigation service takes over and evaluates the guards.
        /// </summary>
        event EventHandler<BackRequestedEventArgs>? BackRequested;

        /// <summary>
        /// Occurs when the user selected another tab of a tabbed page, i.e. without the navigation service
        /// being involved. The navigation service uses it to run the lifecycle callbacks of the tab children.
        /// </summary>
        event EventHandler<TabSelectedEventArgs>? TabSelectedExternally;

        /// <summary>
        /// Occurs when the application was resumed.
        /// </summary>
        event EventHandler? ApplicationResumed;

        /// <summary>
        /// Occurs when the application was sent to the background.
        /// </summary>
        event EventHandler? ApplicationStopped;
    }
}
