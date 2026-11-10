namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Provides view model first page navigation.
    /// </summary>
    /// <remarks>
    /// The navigation service is registered as a singleton. The navigation stack it operates on is derived
    /// from the current window at call time, so any component can inject it, no matter which page is active.
    /// <para>
    /// All operations are marshalled to the UI thread and serialized. Navigating from within
    /// <see cref="INavigatedTo"/> is allowed and is queued behind the running navigation. Navigating from within
    /// a navigation guard, <see cref="IInitialize"/>, <see cref="INavigatedFrom"/> or a handler of
    /// <see cref="Navigating"/> throws, because those callbacks run while the navigation is still in progress.
    /// </para>
    /// <para>
    /// Navigation failures throw a <see cref="NavigationException"/>. If a method returns without throwing,
    /// the navigation was performed. Exceptions thrown by lifecycle callbacks are not wrapped. Every failure,
    /// including a target which is not registered, is reported through the returned task, the log and
    /// <see cref="NavigationFailed"/>; the call itself does not throw.
    /// </para>
    /// </remarks>
    public interface INavigationService
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="INavigationService"/>.
        /// </summary>
        public static INavigationService Current => NavigationService.Current;

        /// <summary>
        /// Gets the page the user currently sees, or <c>null</c> if the application has no window yet.
        /// </summary>
        Page? CurrentPage { get; }

        /// <summary>
        /// Gets a snapshot of the currently active navigation stack, bottom to top.
        /// </summary>
        IReadOnlyList<Page> NavigationStack { get; }

        /// <summary>
        /// Gets a snapshot of the modal stack, bottom to top.
        /// </summary>
        IReadOnlyList<Page> ModalStack { get; }

        /// <summary>
        /// Occurs before a navigation operation is performed.
        /// </summary>
        event EventHandler<NavigatingEventArgs>? Navigating;

        /// <summary>
        /// Occurs after a navigation operation was performed successfully.
        /// </summary>
        event EventHandler<NavigatedEventArgs>? Navigated;

        /// <summary>
        /// Occurs when a navigation operation failed or was vetoed by a navigation guard.
        /// </summary>
        event EventHandler<NavigationFailedEventArgs>? NavigationFailed;

        /// <summary>
        /// Creates the application's start window and navigates it to <paramref name="uri"/>.
        /// Call this from <c>App.CreateWindow</c>.
        /// </summary>
        /// <param name="uri">
        /// A slash-separated list of navigation names which describes the initial page tree,
        /// e.g. <c>"/NavigationPage/HomePage"</c> or <c>"/MainPage/NavigationPage/HomePage"</c>.
        /// </param>
        /// <remarks>
        /// The page tree is created synchronously, so that the window has its root page right away.
        /// The lifecycle callbacks run as soon as the window was created.
        /// </remarks>
        Window CreateWindow(string uri);

        /// <summary>
        /// Pushes the page registered under <paramref name="pageName"/> onto the navigation stack.
        /// </summary>
        /// <param name="pageName">The navigation name of the page.</param>
        /// <param name="animated">Animates the navigation transition.</param>
        /// <exception cref="PageResolveException">If no page is registered under <paramref name="pageName"/>.</exception>
        Task PushAsync(string pageName, bool animated = true);

        /// <summary>
        /// Pushes the page registered under <paramref name="pageName"/> onto the navigation stack
        /// and delivers <paramref name="parameter"/> to the target page and its view model.
        /// </summary>
        /// <param name="pageName">The navigation name of the page.</param>
        /// <param name="parameter">The navigation parameter.</param>
        /// <param name="animated">Animates the navigation transition.</param>
        /// <exception cref="NavigationParameterException">
        /// If neither the target page nor its view model implements <see cref="IInitialize{TParameter}"/>
        /// or <see cref="INavigatedTo{TParameter}"/> for the type of <paramref name="parameter"/>.
        /// </exception>
        Task PushAsync(string pageName, object parameter, bool animated = true);

        /// <summary>
        /// Pushes the page associated with <typeparamref name="TViewModel"/> onto the navigation stack.
        /// </summary>
        /// <typeparam name="TViewModel">The view model type the page is registered with.</typeparam>
        /// <param name="animated">Animates the navigation transition.</param>
        Task PushAsync<TViewModel>(bool animated = true) where TViewModel : class;

        /// <summary>
        /// Pushes the page associated with <typeparamref name="TViewModel"/> onto the navigation stack
        /// and delivers <paramref name="parameter"/> to the target page and its view model.
        /// </summary>
        /// <typeparam name="TViewModel">The view model type the page is registered with.</typeparam>
        /// <param name="parameter">The navigation parameter.</param>
        /// <param name="animated">Animates the navigation transition.</param>
        Task PushAsync<TViewModel>(object parameter, bool animated = true) where TViewModel : class;

        /// <summary>
        /// Pushes the page registered under <paramref name="pageName"/> onto the navigation stack and returns
        /// a task which completes when that page was removed again.
        /// </summary>
        /// <typeparam name="TResult">The type of the expected result.</typeparam>
        /// <param name="pageName">The navigation name of the page.</param>
        /// <param name="parameter">The navigation parameter.</param>
        /// <param name="animated">Animates the navigation transition.</param>
        /// <returns>
        /// The result which the page passed to <see cref="PopAsync(object, bool)"/>,
        /// or <c>default</c> if the page was removed without a result, e.g. by a system back navigation.
        /// </returns>
        Task<TResult?> PushForResultAsync<TResult>(string pageName, object? parameter = null, bool animated = true);

        /// <summary>
        /// Pushes the page associated with <typeparamref name="TViewModel"/> onto the navigation stack and returns
        /// a task which completes when that page was removed again.
        /// </summary>
        /// <typeparam name="TViewModel">The view model type the page is registered with.</typeparam>
        /// <typeparam name="TResult">The type of the expected result.</typeparam>
        /// <param name="parameter">The navigation parameter.</param>
        /// <param name="animated">Animates the navigation transition.</param>
        Task<TResult?> PushForResultAsync<TViewModel, TResult>(object? parameter = null, bool animated = true)
            where TViewModel : class;

        /// <summary>
        /// Pushes the page registered under <paramref name="pageName"/> onto the modal stack.
        /// </summary>
        /// <param name="pageName">The navigation name of the page.</param>
        /// <param name="animated">Animates the navigation transition.</param>
        /// <param name="wrapInNavigationPage">
        /// Wraps the page in a <see cref="NavigationPage"/>, so that further pages can be pushed within
        /// the modal context. If not given, <see cref="NavigationOptions.WrapModalPagesInNavigationPage"/> applies.
        /// </param>
        Task PushModalAsync(string pageName, bool animated = true, bool? wrapInNavigationPage = null);

        /// <summary>
        /// Pushes the page registered under <paramref name="pageName"/> onto the modal stack
        /// and delivers <paramref name="parameter"/> to the target page and its view model.
        /// </summary>
        /// <param name="pageName">The navigation name of the page.</param>
        /// <param name="parameter">The navigation parameter.</param>
        /// <param name="animated">Animates the navigation transition.</param>
        /// <param name="wrapInNavigationPage">
        /// Wraps the page in a <see cref="NavigationPage"/>, so that further pages can be pushed within
        /// the modal context. If not given, <see cref="NavigationOptions.WrapModalPagesInNavigationPage"/> applies.
        /// </param>
        Task PushModalAsync(string pageName, object parameter, bool animated = true, bool? wrapInNavigationPage = null);

        /// <summary>
        /// Pushes the page associated with <typeparamref name="TViewModel"/> onto the modal stack.
        /// </summary>
        /// <typeparam name="TViewModel">The view model type the page is registered with.</typeparam>
        Task PushModalAsync<TViewModel>(bool animated = true, bool? wrapInNavigationPage = null) where TViewModel : class;

        /// <summary>
        /// Pushes the page associated with <typeparamref name="TViewModel"/> onto the modal stack
        /// and delivers <paramref name="parameter"/> to the target page and its view model.
        /// </summary>
        /// <typeparam name="TViewModel">The view model type the page is registered with.</typeparam>
        Task PushModalAsync<TViewModel>(object parameter, bool animated = true, bool? wrapInNavigationPage = null)
            where TViewModel : class;

        /// <summary>
        /// Pushes the page registered under <paramref name="pageName"/> onto the modal stack and returns
        /// a task which completes when that page was dismissed again.
        /// </summary>
        /// <typeparam name="TResult">The type of the expected result.</typeparam>
        Task<TResult?> PushModalForResultAsync<TResult>(
            string pageName, object? parameter = null, bool animated = true, bool? wrapInNavigationPage = null);

        /// <summary>
        /// Pushes the page associated with <typeparamref name="TViewModel"/> onto the modal stack and returns
        /// a task which completes when that page was dismissed again.
        /// </summary>
        /// <typeparam name="TViewModel">The view model type the page is registered with.</typeparam>
        /// <typeparam name="TResult">The type of the expected result.</typeparam>
        Task<TResult?> PushModalForResultAsync<TViewModel, TResult>(
            object? parameter = null, bool animated = true, bool? wrapInNavigationPage = null)
            where TViewModel : class;

        /// <summary>
        /// Pops the current page from the navigation stack.
        /// </summary>
        /// <param name="animated">Animates the navigation transition.</param>
        /// <exception cref="NavigationCancelledException">If a navigation guard vetoed the navigation.</exception>
        Task PopAsync(bool animated = true);

        /// <summary>
        /// Pops the current page from the navigation stack and delivers <paramref name="result"/> to the
        /// page which is revealed, and to a pending <c>PushForResultAsync</c> caller, if any.
        /// </summary>
        /// <param name="result">The result of the page which is popped.</param>
        /// <param name="animated">Animates the navigation transition.</param>
        Task PopAsync(object result, bool animated = true);

        /// <summary>
        /// Dismisses the topmost page of the modal stack.
        /// </summary>
        /// <param name="animated">Animates the navigation transition.</param>
        Task PopModalAsync(bool animated = true);

        /// <summary>
        /// Dismisses the topmost page of the modal stack and delivers <paramref name="result"/> to the
        /// page which is revealed, and to a pending <c>PushModalForResultAsync</c> caller, if any.
        /// </summary>
        /// <param name="result">The result of the page which is dismissed.</param>
        /// <param name="animated">Animates the navigation transition.</param>
        Task PopModalAsync(object result, bool animated = true);

        /// <summary>
        /// Pops all pages of the navigation stack except its root page.
        /// </summary>
        /// <param name="animated">Animates the navigation transition.</param>
        Task PopToRootAsync(bool animated = true);

        /// <summary>
        /// Navigates back to the named page of the current navigation stack, removing every page above it.
        /// </summary>
        /// <exception cref="NavigationException">The page is not on the current navigation stack.</exception>
        /// <exception cref="NavigationCancelledException">A guard of a removed page vetoed the operation.</exception>
        Task PopToAsync(string pageName, bool animated = true);

        /// <inheritdoc cref="PopToAsync(string, bool)"/>
        Task PopToAsync<TViewModel>(bool animated = true) where TViewModel : class;

        /// <summary>
        /// Closes every popup which is currently open, topmost first. Popups which are covered by an
        /// ordinary modal page are not closed.
        /// </summary>
        /// <exception cref="NavigationCancelledException">A guard of a popup vetoed the dismissal.</exception>
        Task PopAllPopupsAsync(bool animated = true);

        /// <summary>
        /// Pushes the page and removes every page which was on the navigation stack before, so that the
        /// pushed page becomes the only one. The guards of the removed pages are evaluated.
        /// </summary>
        /// <exception cref="NavigationCancelledException">A guard of a removed page vetoed the operation.</exception>
        Task PushAsRootAsync(string pageName, object? parameter = null, bool animated = true);

        /// <inheritdoc cref="PushAsRootAsync(string, object?, bool)"/>
        Task PushAsRootAsync<TViewModel>(object? parameter = null, bool animated = true) where TViewModel : class;

        /// <summary>
        /// Removes a page which is on the current navigation stack but is not the visible one, e.g. in order
        /// to drop an intermediate step of a flow. The guard of the page is evaluated and its service scope
        /// is disposed.
        /// </summary>
        /// <exception cref="NavigationException">
        /// The page is not on the current navigation stack, or it is the visible page. Use
        /// <see cref="PopAsync(bool)"/> to remove the visible page.
        /// </exception>
        Task RemovePageAsync(Page page);

        /// <summary>
        /// Selects the tab of the currently active <see cref="TabbedPage"/> which hosts the page
        /// registered under <paramref name="tabName"/>.
        /// </summary>
        /// <param name="tabName">The navigation name of the page which the tab hosts.</param>
        /// <remarks>
        /// Only the statically declared tabs of the active tabbed page are selected; tabs are not created.
        /// The lifecycle callbacks are delivered to the outgoing and the incoming tab child only.
        /// </remarks>
        /// <exception cref="NavigationException">
        /// If no tabbed page is active, or if none of its tabs hosts the given page.
        /// </exception>
        Task SelectTabAsync(string tabName);

        /// <summary>
        /// Selects the tab of the currently active <see cref="TabbedPage"/> which hosts the page
        /// associated with <typeparamref name="TViewModel"/>.
        /// </summary>
        /// <typeparam name="TViewModel">The view model type the hosted page is registered with.</typeparam>
        Task SelectTabAsync<TViewModel>() where TViewModel : class;

        /// <summary>
        /// Performs a multi-segment navigation.
        /// </summary>
        /// <param name="uri">
        /// A slash-separated list of navigation names. A leading slash replaces the root page of the window,
        /// otherwise the segments are pushed onto the current navigation stack. The segment
        /// <c>NavigationPage</c> (or the name of a registered <see cref="NavigationPage"/> subclass) wraps
        /// the pages which follow, e.g. <c>"/NavigationPage/HomePage"</c>.
        /// </param>
        /// <param name="animated">Animates the navigation transition.</param>
        Task NavigateAsync(string uri, bool animated = true);

        /// <summary>
        /// Performs a multi-segment navigation and delivers <paramref name="parameter"/> to the final target.
        /// </summary>
        /// <param name="uri">A slash-separated list of navigation names.</param>
        /// <param name="parameter">The navigation parameter for the final target.</param>
        /// <param name="animated">Animates the navigation transition.</param>
        Task NavigateAsync(string uri, object parameter, bool animated = true);
    }
}
