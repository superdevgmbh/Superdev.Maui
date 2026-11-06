using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Controls;
using Superdev.Maui.Navigation.Hosts;
using Superdev.Maui.Navigation.Internals;

namespace Superdev.Maui.Navigation
{
    /// <inheritdoc cref="INavigationService"/>
    public class NavigationService : INavigationService, IDisposable
    {
        private const string NavigationPageSegment = nameof(NavigationPage);
        private const string RemovePageSegment = "..";

        private static readonly AsyncLocal<bool> IsInNavigationPhase = new AsyncLocal<bool>();
        private static readonly Lock CurrentSyncRoot = new Lock();
        private static INavigationService? current;

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly NavigationServiceContext context;

        private List<Page>? pagesCreatedInCommitPhase;
        private bool isConfirmingBackFromRootPage;
        private bool isDisposed;

        /// <inheritdoc cref="NavigationService"/>
        public NavigationService(NavigationServiceContext context)
        {
            this.context = context;
            this.context.NavigationHost.PageRemovedExternally += this.OnPageRemovedExternally;
            this.context.NavigationHost.BackRequested += this.OnBackRequested;
            this.context.NavigationHost.TabSelectedExternally += this.OnTabSelectedExternally;
            this.context.NavigationHost.ApplicationResumed += this.OnApplicationResumed;
            this.context.NavigationHost.ApplicationStopped += this.OnApplicationStopped;
        }

        /// <summary>
        /// Gets the singleton instance of <see cref="INavigationService"/> from the application's service provider,
        /// so that an application which registers its own implementation gets that implementation here as well.
        /// </summary>
        public static INavigationService Current
        {
            get
            {
                lock (CurrentSyncRoot)
                {
                    if (current is NavigationService { isDisposed: true })
                    {
                        current = null;
                    }

                    return current ??= IPlatformApplication.Current?.Services.GetService<INavigationService>()
                        ?? throw new InvalidOperationException(
                            $"{nameof(INavigationService)} is not registered. Call UseSuperdevMaui() in MauiProgram.");
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler<NavigatingEventArgs>? Navigating;

        /// <inheritdoc />
        public event EventHandler<NavigatedEventArgs>? Navigated;

        /// <inheritdoc />
        public event EventHandler<NavigationFailedEventArgs>? NavigationFailed;

        /// <inheritdoc />
        public Page? CurrentPage => this.context.NavigationHost.GetActivePage();

        /// <inheritdoc />
        public IReadOnlyList<Page> NavigationStack
        {
            get
            {
                var navigationSurface = this.context.NavigationHost.GetActiveNavigationSurface();
                return navigationSurface == null
                    ? Array.Empty<Page>()
                    : this.context.NavigationHost.GetNavigationStack(navigationSurface);
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<Page> ModalStack => this.context.NavigationHost.GetModalStack();

        /// <inheritdoc />
        public Task PushAsync(string pageName, bool animated = true)
        {
            return this.PushInternalAsync(PageTarget.Named(pageName), parameter: null, animated, resultSource: null);
        }

        /// <inheritdoc />
        public Task PushAsync(string pageName, object parameter, bool animated = true)
        {
            ArgumentNullException.ThrowIfNull(parameter);
            return this.PushInternalAsync(PageTarget.Named(pageName), parameter, animated, resultSource: null);
        }

        /// <inheritdoc />
        public Task PushAsync<TViewModel>(bool animated = true) where TViewModel : class
        {
            return this.PushInternalAsync(PageTarget.ForViewModel<TViewModel>(), parameter: null, animated, resultSource: null);
        }

        /// <inheritdoc />
        public Task PushAsync<TViewModel>(object parameter, bool animated = true) where TViewModel : class
        {
            ArgumentNullException.ThrowIfNull(parameter);
            return this.PushInternalAsync(PageTarget.ForViewModel<TViewModel>(), parameter, animated, resultSource: null);
        }

        /// <inheritdoc />
        public Task<TResult?> PushForResultAsync<TResult>(string pageName, object? parameter = null, bool animated = true)
        {
            return this.PushForResultInternalAsync<TResult>(PageTarget.Named(pageName), parameter, animated, modal: false, wrapInNavigationPage: null);
        }

        /// <inheritdoc />
        public Task<TResult?> PushForResultAsync<TViewModel, TResult>(object? parameter = null, bool animated = true)
            where TViewModel : class
        {
            return this.PushForResultInternalAsync<TResult>(
                PageTarget.ForViewModel<TViewModel>(), parameter, animated, modal: false, wrapInNavigationPage: null);
        }

        /// <inheritdoc />
        public Task PushModalAsync(string pageName, bool animated = true, bool? wrapInNavigationPage = null)
        {
            return this.PushModalInternalAsync(PageTarget.Named(pageName), parameter: null, animated, wrapInNavigationPage, resultSource: null);
        }

        /// <inheritdoc />
        public Task PushModalAsync(string pageName, object parameter, bool animated = true, bool? wrapInNavigationPage = null)
        {
            ArgumentNullException.ThrowIfNull(parameter);
            return this.PushModalInternalAsync(PageTarget.Named(pageName), parameter, animated, wrapInNavigationPage, resultSource: null);
        }

        /// <inheritdoc />
        public Task PushModalAsync<TViewModel>(bool animated = true, bool? wrapInNavigationPage = null) where TViewModel : class
        {
            return this.PushModalInternalAsync(
                PageTarget.ForViewModel<TViewModel>(), parameter: null, animated, wrapInNavigationPage, resultSource: null);
        }

        /// <inheritdoc />
        public Task PushModalAsync<TViewModel>(object parameter, bool animated = true, bool? wrapInNavigationPage = null)
            where TViewModel : class
        {
            ArgumentNullException.ThrowIfNull(parameter);
            return this.PushModalInternalAsync(
                PageTarget.ForViewModel<TViewModel>(), parameter, animated, wrapInNavigationPage, resultSource: null);
        }

        /// <inheritdoc />
        public Task<TResult?> PushModalForResultAsync<TResult>(
            string pageName, object? parameter = null, bool animated = true, bool? wrapInNavigationPage = null)
        {
            return this.PushForResultInternalAsync<TResult>(PageTarget.Named(pageName), parameter, animated, modal: true, wrapInNavigationPage);
        }

        /// <inheritdoc />
        public Task<TResult?> PushModalForResultAsync<TViewModel, TResult>(
            object? parameter = null, bool animated = true, bool? wrapInNavigationPage = null)
            where TViewModel : class
        {
            return this.PushForResultInternalAsync<TResult>(
                PageTarget.ForViewModel<TViewModel>(), parameter, animated, modal: true, wrapInNavigationPage);
        }

        /// <inheritdoc />
        public Task PopAsync(bool animated = true)
        {
            return this.PopInternalAsync(result: null, animated);
        }

        /// <inheritdoc />
        public Task PopAsync(object result, bool animated = true)
        {
            ArgumentNullException.ThrowIfNull(result);
            return this.PopInternalAsync(result, animated);
        }

        /// <inheritdoc />
        public Task PopModalAsync(bool animated = true)
        {
            return this.PopModalInternalAsync(result: null, animated);
        }

        /// <inheritdoc />
        public Task PopModalAsync(object result, bool animated = true)
        {
            ArgumentNullException.ThrowIfNull(result);
            return this.PopModalInternalAsync(result, animated);
        }

        /// <inheritdoc />
        public Task PopToRootAsync(bool animated = true)
        {
            var request = new NavigationInfo
            {
                Operation = NavigationOperation.PopToRoot,
                Direction = NavigationDirection.Back,
                Animated = animated
            };

            return this.ExecuteAsync(request, async () =>
            {
                var navigationSurface = this.GetActiveNavigationSurfaceOrThrow();
                var navigationStack = this.context.NavigationHost.GetNavigationStack(navigationSurface);

                if (navigationStack.Count <= 1)
                {
                    return NavigationOutcome.Empty;
                }

                var removedPages = navigationStack.Skip(1).Reverse().ToArray();
                var rootPage = navigationStack[0];

                await this.EnsureCanNavigateAsync(removedPages);

                await this.context.LifecycleDispatcher.NavigatedFromAsync(navigationStack[^1], NavigationDirection.Back);
                await this.context.NavigationHost.PopToRootAsync(navigationSurface, animated);

                return new NavigationOutcome
                {
                    NavigatedToPage = rootPage,
                    NavigatedToDirection = NavigationDirection.Back,
                    DisposeBag = removedPages
                };
            });
        }

        /// <inheritdoc />
        public Task PopToAsync(string pageName, bool animated = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pageName);
            return this.PopToInternalAsync(PageTarget.Named(pageName), animated);
        }

        /// <inheritdoc />
        public Task PopToAsync<TViewModel>(bool animated = true) where TViewModel : class
        {
            return this.PopToInternalAsync(PageTarget.ForViewModel<TViewModel>(), animated);
        }

        private Task PopToInternalAsync(PageTarget target, bool animated)
        {
            var request = new NavigationInfo
            {
                Operation = NavigationOperation.PopTo,
                Direction = NavigationDirection.Back,
                PageName = target.ToString(),
                Animated = animated
            };

            PageRegistration registration = null!;
            string pageName = null!;

            return this.ExecuteAsync(
                request,
                prepare: () =>
                {
                    (registration, pageName) = target.Resolve(this.context.NavigationRegistry);
                    return request.With(NavigationOperation.PopTo, pageName);
                },
                commitPhase: async () =>
                {
                    var navigationSurface = this.GetActiveNavigationSurfaceOrThrow();
                    var navigationStack = this.context.NavigationHost.GetNavigationStack(navigationSurface);

                    // The topmost page of that type wins, so that a page which occurs twice navigates back
                    // by the shortest distance.
                    var targetIndex = -1;
                    for (var i = navigationStack.Count - 1; i >= 0; i--)
                    {
                        if (navigationStack[i].GetType() == registration.PageType)
                        {
                            targetIndex = i;
                            break;
                        }
                    }

                    if (targetIndex < 0)
                    {
                        throw new NavigationException(
                            $"Page \"{pageName}\" is not on the current navigation stack")
                        {
                            PageName = pageName
                        };
                    }

                    if (targetIndex == navigationStack.Count - 1)
                    {
                        return NavigationOutcome.Empty;
                    }

                    // Topmost first, which is the order the guards are evaluated and the pages are disposed in.
                    var removedPages = navigationStack.Skip(targetIndex + 1).Reverse().ToArray();
                    var targetPage = navigationStack[targetIndex];

                    await this.EnsureCanNavigateAsync(removedPages);
                    await this.context.LifecycleDispatcher.NavigatedFromAsync(navigationStack[^1], NavigationDirection.Back);

                    // The pages in between are removed silently; popping the topmost one animates the transition.
                    foreach (var removedPage in removedPages.Skip(1))
                    {
                        this.context.NavigationHost.RemovePage(navigationSurface, removedPage);
                    }

                    await this.context.NavigationHost.PopAsync(navigationSurface, animated);

                    return new NavigationOutcome
                    {
                        NavigatedToPage = targetPage,
                        NavigatedToDirection = NavigationDirection.Back,
                        DisposeBag = removedPages
                    };
                });
        }

        /// <inheritdoc />
        public async Task PopAllPopupsAsync(bool animated = true)
        {
            // Every dismissal is an operation of its own, so each popup runs through the whole pipeline
            // including its navigation guard. Whether a popup is left to close is decided inside the
            // pipeline, where the modal stack cannot change underneath the decision.
            while (await this.PopTopmostPopupAsync(animated))
            {
            }
        }

        /// <summary>
        /// Closes the topmost modal page if it is a popup, and reports whether it did.
        /// </summary>
        private async Task<bool> PopTopmostPopupAsync(bool animated)
        {
            var request = new NavigationInfo
            {
                Operation = NavigationOperation.PopModal,
                Direction = NavigationDirection.Back,
                Animated = animated
            };

            var popped = false;

            await this.ExecuteAsync(
                request,
                prepare: () => this.context.NavigationHost.GetModalStack().LastOrDefault() is PopupPage ? request : null,
                commitPhase: async () =>
                {
                    var outcome = await this.PopModalCoreAsync(result: null, animated);
                    popped = true;
                    return outcome;
                });

            return popped;
        }

        /// <inheritdoc />
        public Task PushAsRootAsync(string pageName, object? parameter = null, bool animated = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pageName);
            return this.PushAsRootInternalAsync(PageTarget.Named(pageName), parameter, animated);
        }

        /// <inheritdoc />
        public Task PushAsRootAsync<TViewModel>(object? parameter = null, bool animated = true)
            where TViewModel : class
        {
            return this.PushAsRootInternalAsync(PageTarget.ForViewModel<TViewModel>(), parameter, animated);
        }

        private Task PushAsRootInternalAsync(PageTarget target, object? parameter, bool animated)
        {
            var request = new NavigationInfo
            {
                Operation = NavigationOperation.Push,
                Direction = NavigationDirection.Forward,
                PageName = target.ToString(),
                Parameter = parameter,
                Animated = animated
            };

            string pageName = null!;

            return this.ExecuteAsync(
                request,
                prepare: () =>
                {
                    (_, pageName) = target.Resolve(this.context.NavigationRegistry);
                    return request.With(NavigationOperation.Push, pageName);
                },
                commitPhase: async () =>
                {
                    var navigationSurface = this.GetActiveNavigationSurfaceOrThrow();
                    var previousPages = this.context.NavigationHost.GetNavigationStack(navigationSurface)
                        .Reverse()
                        .ToArray();

                    var sourcePage = this.context.NavigationHost.GetActivePage();
                    var page = this.CreatePage(pageName, parameter, resultSource: null);

                    await this.EnsureCanNavigateAsync(previousPages);
                    await this.context.LifecycleDispatcher.InitializeAsync(page, parameter);

                    if (sourcePage != null)
                    {
                        await this.context.LifecycleDispatcher.NavigatedFromAsync(sourcePage, NavigationDirection.Forward);
                    }

                    // Pushed before the old pages are removed, so that the stack is never empty.
                    await this.context.NavigationHost.PushAsync(navigationSurface, page, animated);

                    foreach (var previousPage in previousPages)
                    {
                        this.context.NavigationHost.RemovePage(navigationSurface, previousPage);
                    }

                    return new NavigationOutcome
                    {
                        NavigatedToPage = page,
                        NavigatedToDirection = NavigationDirection.Forward,
                        NavigatedToParameter = parameter,
                        DisposeBag = previousPages
                    };
                });
        }

        /// <inheritdoc />
        public Task RemovePageAsync(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);

            var request = new NavigationInfo
            {
                Operation = NavigationOperation.RemovePage,
                Direction = NavigationDirection.Back,
                PageName = page.GetType().Name
            };

            return this.ExecuteAsync(request, async () =>
            {
                var navigationSurface = this.GetActiveNavigationSurfaceOrThrow();
                var navigationStack = this.context.NavigationHost.GetNavigationStack(navigationSurface);

                if (!navigationStack.Any(stackPage => ReferenceEquals(stackPage, page)))
                {
                    throw new NavigationException(
                        $"Page \"{page.GetType().Name}\" is not on the current navigation stack")
                    {
                        PageName = page.GetType().Name
                    };
                }

                if (ReferenceEquals(navigationStack[^1], page))
                {
                    throw new NavigationException(
                        $"Page \"{page.GetType().Name}\" is the visible page. " +
                        $"Use {nameof(this.PopAsync)} in order to navigate back from it.")
                    {
                        PageName = page.GetType().Name
                    };
                }

                var removedPages = CollectPages(page).Reverse().ToArray();
                await this.EnsureCanNavigateAsync(removedPages);

                this.context.NavigationHost.RemovePage(navigationSurface, page);

                // The visible page did not change, so nothing is navigated to.
                return new NavigationOutcome
                {
                    DisposeBag = removedPages
                };
            });
        }

        /// <inheritdoc />
        public Task SelectTabAsync(string tabName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tabName);
            return this.SelectTabInternalAsync(PageTarget.Named(tabName));
        }

        /// <inheritdoc />
        public Task SelectTabAsync<TViewModel>() where TViewModel : class
        {
            return this.SelectTabInternalAsync(PageTarget.ForViewModel<TViewModel>());
        }

        private Task SelectTabInternalAsync(PageTarget target)
        {
            var request = new NavigationInfo
            {
                Operation = NavigationOperation.SelectTab,
                Direction = NavigationDirection.Forward,
                PageName = target.ToString()
            };

            PageRegistration registration = null!;
            string tabName = null!;

            return this.ExecuteAsync(
                request,
                prepare: () =>
                {
                    (registration, tabName) = target.Resolve(this.context.NavigationRegistry);
                    return request.With(NavigationOperation.SelectTab, tabName);
                },
                commitPhase: async () =>
                {
                    var tabbedPage = this.FindActiveTabbedPage()
                        ?? throw new NavigationException("There is no tabbed page which the tab could be selected on")
                        {
                            PageName = tabName
                        };

                    var tab = tabbedPage.Children.FirstOrDefault(
                        child => CollectPages(child).Any(page => page.GetType() == registration.PageType));

                    if (tab == null)
                    {
                        throw new NavigationException($"No tab of the active tabbed page hosts the page \"{tabName}\"")
                        {
                            PageName = tabName
                        };
                    }

                    if (ReferenceEquals(tabbedPage.CurrentPage, tab))
                    {
                        return NavigationOutcome.Empty;
                    }

                    var previousTabPage = tabbedPage.CurrentPage is { } previousTab ? this.GetLeafPage(previousTab) : null;
                    var targetPage = this.GetLeafPage(tab);

                    if (previousTabPage != null)
                    {
                        await this.context.LifecycleDispatcher.NavigatedFromAsync(previousTabPage, NavigationDirection.Forward);
                    }

                    // Tabs which are declared in XAML are initialized when they are selected for the first time -
                    // before the tab is shown, which is what the initialization callback promises.
                    await this.context.LifecycleDispatcher.InitializeAsync(targetPage, parameter: null);

                    this.context.NavigationHost.SelectTab(tabbedPage, tab);

                    return new NavigationOutcome
                    {
                        NavigatedToPage = targetPage,
                        NavigatedToDirection = NavigationDirection.Forward
                    };
                });
        }

        /// <summary>
        /// Finds the tabbed page of the currently active branch, if any.
        /// </summary>
        private TabbedPage? FindActiveTabbedPage()
        {
            var page = this.context.NavigationHost.GetModalStack().LastOrDefault()
                ?? this.context.NavigationHost.RootPage;

            while (page != null)
            {
                if (page is TabbedPage tabbedPage)
                {
                    return tabbedPage;
                }

                var childPage = page switch
                {
                    FlyoutPage flyoutPage => flyoutPage.Detail,
                    NavigationPage navigationPage => navigationPage.CurrentPage,
                    _ => null
                };

                if (childPage == null || ReferenceEquals(childPage, page))
                {
                    return null;
                }

                page = childPage;
            }

            return null;
        }

        /// <inheritdoc />
        public Task NavigateAsync(string uri, bool animated = true)
        {
            return this.NavigateInternalAsync(uri, parameter: null, animated);
        }

        /// <inheritdoc />
        public Task NavigateAsync(string uri, object parameter, bool animated = true)
        {
            ArgumentNullException.ThrowIfNull(parameter);
            return this.NavigateInternalAsync(uri, parameter, animated);
        }

        /// <inheritdoc />
        public Window CreateWindow(string uri)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uri);

            // The same parser as NavigateAsync, so a start URI may carry query parameters too.
            var (segments, _) = ParseUri(uri);

            // The page tree is created outside the pipeline, because the window needs its root page right away.
            // The pages are tracked as they would be inside it, so that a segment which fails to resolve does
            // not leak the service scopes of the pages created before it.
            this.pagesCreatedInCommitPhase = new List<Page>();
            (Page RootPage, IReadOnlyList<Page> PagesToPush, Page FinalTarget) pageTree;
            try
            {
                pageTree = this.BuildPageTree(segments, parameter: null);
            }
            catch
            {
                _ = this.DisposeOrphanedPagesAsync();
                throw;
            }

            this.pagesCreatedInCommitPhase = null;

            var (rootPage, pagesToPush, finalTarget) = pageTree;
            var window = new Window(rootPage);

            void OnWindowCreated(object? sender, EventArgs e)
            {
                window.Created -= OnWindowCreated;
                _ = this.RunStartupNavigationAsync(pagesToPush, finalTarget);
            }

            window.Created += OnWindowCreated;
            return window;
        }

        /// <summary>
        /// Runs the lifecycle callbacks of the pages which were created for the application's start window.
        /// </summary>
        private async Task RunStartupNavigationAsync(IReadOnlyList<Page> pagesToPush, Page finalTarget)
        {
            var navigationInfo = new NavigationInfo
            {
                Operation = NavigationOperation.Navigate,
                Direction = NavigationDirection.Forward,
                PageName = finalTarget.GetType().Name
            };

            try
            {
                // The start navigation uses the same pipeline as every other navigation, so that it is
                // serialized against a navigation which an early lifecycle callback might trigger.
                await this.ExecuteAsync(navigationInfo, async () =>
                {
                    var rootPage = this.context.NavigationHost.RootPage
                        ?? throw new NavigationException("The window has no root page");

                    foreach (var page in CollectPages(rootPage).Concat(pagesToPush))
                    {
                        await this.context.LifecycleDispatcher.InitializeAsync(page, GetPendingParameter(page));
                    }

                    if (pagesToPush.Count > 0)
                    {
                        var navigationSurface = this.GetActiveNavigationSurfaceOrThrow();
                        foreach (var page in pagesToPush)
                        {
                            await this.context.NavigationHost.PushAsync(navigationSurface, page, animated: false);
                        }
                    }

                    var startupTarget = this.GetLeafPage(finalTarget);

                    return new NavigationOutcome
                    {
                        NavigatedToPage = startupTarget,
                        NavigatedToDirection = NavigationDirection.Forward,
                        NavigatedToParameter = GetPendingParameter(startupTarget)
                    };
                });
            }
            catch (Exception)
            {
                // The failure was reported by the pipeline. There is no caller which could observe it,
                // because the start navigation is triggered by a window event.
            }
        }

        /// <summary>
        /// Unsubscribes from the navigation host. The navigation service lives as long as the application,
        /// so this matters mostly for tests, which create and discard many instances.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="Dispose()"/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            this.context.NavigationHost.PageRemovedExternally -= this.OnPageRemovedExternally;
            this.context.NavigationHost.BackRequested -= this.OnBackRequested;
            this.context.NavigationHost.TabSelectedExternally -= this.OnTabSelectedExternally;
            this.context.NavigationHost.ApplicationResumed -= this.OnApplicationResumed;
            this.context.NavigationHost.ApplicationStopped -= this.OnApplicationStopped;

            // The semaphore is deliberately not disposed: a navigation which is still running would fail to
            // release it. It holds no unmanaged resource, so letting it be collected is safe.
        }

        /// <summary>
        /// Called before a navigation operation is performed. The base implementation raises
        /// <see cref="Navigating"/>. Override in order to intercept every navigation operation.
        /// </summary>
        protected virtual Task OnNavigatingAsync(NavigationInfo request)
        {
            this.Navigating?.Invoke(this, new NavigatingEventArgs(request));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called after a navigation operation was performed successfully. The base implementation raises
        /// <see cref="Navigated"/>.
        /// </summary>
        protected virtual Task OnNavigatedAsync(NavigationInfo request, TimeSpan duration)
        {
            this.Navigated?.Invoke(this, new NavigatedEventArgs(request, duration));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when a navigation operation failed or was vetoed by a navigation guard.
        /// The base implementation raises <see cref="NavigationFailed"/>.
        /// </summary>
        protected virtual Task OnNavigationFailedAsync(NavigationInfo request, Exception exception)
        {
            this.NavigationFailed?.Invoke(this, new NavigationFailedEventArgs(request, exception));
            return Task.CompletedTask;
        }

        private Task PushInternalAsync(PageTarget target, object? parameter, bool animated, NavigationResultSource? resultSource)
        {
            var request = new NavigationInfo
            {
                Operation = NavigationOperation.Push,
                Direction = NavigationDirection.Forward,
                PageName = target.ToString(),
                Parameter = parameter,
                Animated = animated
            };

            string pageName = null!;
            var isPopup = false;

            return this.ExecuteAsync(
                request,
                prepare: () =>
                {
                    PageRegistration registration;
                    (registration, pageName) = target.Resolve(this.context.NavigationRegistry);

                    // Popup pages live on the modal stack, no matter which push method opened them.
                    isPopup = typeof(PopupPage).IsAssignableFrom(registration.PageType);
                    return request.With(isPopup ? NavigationOperation.PushModal : NavigationOperation.Push, pageName);
                },
                commitPhase: async () =>
                {
                    if (isPopup)
                    {
                        var popupPage = this.CreatePage(pageName, parameter, resultSource);
                        return await this.PushModalCoreAsync(popupPage, parameter, animated, wrapInNavigationPage: false);
                    }

                    var navigationSurface = this.GetActiveNavigationSurfaceOrThrow();
                    var sourcePage = this.context.NavigationHost.GetActivePage();

                    var page = this.CreatePage(pageName, parameter, resultSource);

                    await this.context.LifecycleDispatcher.InitializeAsync(page, parameter);

                    if (sourcePage != null)
                    {
                        await this.context.LifecycleDispatcher.NavigatedFromAsync(sourcePage, NavigationDirection.Forward);
                    }

                    await this.context.NavigationHost.PushAsync(navigationSurface, page, animated);

                    return new NavigationOutcome
                    {
                        NavigatedToPage = page,
                        NavigatedToDirection = NavigationDirection.Forward,
                        NavigatedToParameter = parameter
                    };
                });
        }

        private Task PushModalInternalAsync(
            PageTarget target, object? parameter, bool animated, bool? wrapInNavigationPage, NavigationResultSource? resultSource)
        {
            var request = new NavigationInfo
            {
                Operation = NavigationOperation.PushModal,
                Direction = NavigationDirection.Forward,
                PageName = target.ToString(),
                Parameter = parameter,
                Animated = animated
            };

            string pageName = null!;

            return this.ExecuteAsync(
                request,
                prepare: () =>
                {
                    (_, pageName) = target.Resolve(this.context.NavigationRegistry);
                    return request.With(NavigationOperation.PushModal, pageName);
                },
                commitPhase: () =>
                {
                    var page = this.CreatePage(pageName, parameter, resultSource);
                    return this.PushModalCoreAsync(page, parameter, animated, wrapInNavigationPage);
                });
        }

        /// <summary>
        /// Presents an already created page on the modal stack. A <see cref="PopupPage"/> is presented as a
        /// popup, i.e. as a transparent modal page which animates its own content.
        /// </summary>
        private async Task<NavigationOutcome> PushModalCoreAsync(
            Page page, object? parameter, bool animated, bool? wrapInNavigationPage)
        {
            var sourcePage = this.context.NavigationHost.GetActivePage();

            await this.context.LifecycleDispatcher.InitializeAsync(page, parameter);

            if (sourcePage != null)
            {
                await this.context.LifecycleDispatcher.NavigatedFromAsync(sourcePage, NavigationDirection.Forward);
            }

            if (page is PopupPage popupPage)
            {
                if (animated)
                {
                    // Only prepare the animation when it is going to run: preparing hides the content, and
                    // without the appearing animation nothing would ever make it visible again.
                    popupPage.PrepareAnimation();
                }

                popupPage.BackgroundClicked += this.OnPopupBackgroundClicked;

                // The popup animates its own content, therefore the platform transition is suppressed.
                await this.context.NavigationHost.PushModalAsync(popupPage, animated: false);

                return new NavigationOutcome
                {
                    NavigatedToPage = page,
                    NavigatedToDirection = NavigationDirection.Forward,
                    NavigatedToParameter = parameter,
                    PostCommitAction = animated ? popupPage.PlayAppearingAnimationAsync : null
                };
            }

            var wrap = wrapInNavigationPage ?? this.context.NavigationOptions.WrapModalPagesInNavigationPage;
            var modalPage = wrap ? this.CreateModalNavigationPage(page) : page;

            if (this.context.NavigationOptions.PresentGuardedModalPagesFullScreen && NavigationGuardEvaluator.HasGuard(page))
            {
                // A modal page presented as an iOS sheet can be swiped away, and that gesture cannot be cancelled
                // once it started, so a navigation guard could not be honored. A guarded page is presented full
                // screen instead, which has no such gesture; its close button remains the way out.
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetModalPresentationStyle(
                    modalPage.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>(),
                    Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.FullScreen);
            }

            await this.context.NavigationHost.PushModalAsync(modalPage, animated);

            return new NavigationOutcome
            {
                NavigatedToPage = page,
                NavigatedToDirection = NavigationDirection.Forward,
                NavigatedToParameter = parameter
            };
        }

        private async Task<TResult?> PushForResultInternalAsync<TResult>(
            PageTarget target, object? parameter, bool animated, bool modal, bool? wrapInNavigationPage)
        {
            var resultSource = new NavigationResultSource { ExpectedResultType = typeof(TResult) };

            try
            {
                if (modal)
                {
                    await this.PushModalInternalAsync(target, parameter, animated, wrapInNavigationPage, resultSource);
                }
                else
                {
                    await this.PushInternalAsync(target, parameter, animated, resultSource);
                }
            }
            catch
            {
                // The failure reached this caller through the push. The pipeline faults the result source as
                // well, and a faulted task which nobody awaits is reported as an unobserved exception.
                _ = resultSource.Task.Exception;
                throw;
            }

            var result = await resultSource.Task;

            if (result == null)
            {
                return default;
            }

            if (result is not TResult typedResult)
            {
                throw new NavigationParameterException(
                    $"Page \"{target}\" returned a result of type \"{result.GetType().Name}\", " +
                    $"but \"{typeof(TResult).Name}\" was expected");
            }

            return typedResult;
        }

        private Task PopInternalAsync(object? result, bool animated)
        {
            var request = new NavigationInfo
            {
                Operation = NavigationOperation.Pop,
                Direction = NavigationDirection.Back,
                Parameter = result,
                Animated = animated
            };

            var isPopup = false;

            return this.ExecuteAsync(
                request,
                prepare: () =>
                {
                    // A popup is closed with the same call which navigates back from an ordinary page. Which
                    // of the two it is gets decided here, inside the lock: decided on the caller's thread before
                    // entering the pipeline, a pop queued behind the push of a popup would pop the page underneath.
                    isPopup = this.context.NavigationHost.GetActivePage() is PopupPage;
                    return isPopup ? request.With(NavigationOperation.PopModal, request.PageName) : request;
                },
                commitPhase: () => isPopup
                    ? this.PopModalCoreAsync(result, animated)
                    : this.PopPageCoreAsync(result, animated));
        }

        /// <summary>
        /// Pops the visible page of the active navigation stack.
        /// </summary>
        private async Task<NavigationOutcome> PopPageCoreAsync(object? result, bool animated)
        {
            var navigationSurface = this.GetActiveNavigationSurfaceOrThrow();
            var navigationStack = this.context.NavigationHost.GetNavigationStack(navigationSurface);

            if (navigationStack.Count <= 1)
            {
                throw new NavigationException("Cannot pop the root page of the navigation stack");
            }

            var poppedPage = navigationStack[^1];
            var revealedPage = navigationStack[^2];

            await this.EnsureCanNavigateAsync(new[] { poppedPage });
            this.EnsureResultIsAccepted(poppedPage, revealedPage, result);

            await this.context.LifecycleDispatcher.NavigatedFromAsync(poppedPage, NavigationDirection.Back);
            await this.context.NavigationHost.PopAsync(navigationSurface, animated);

            StoreResult(poppedPage, result);

            return new NavigationOutcome
            {
                NavigatedToPage = revealedPage,
                NavigatedToDirection = NavigationDirection.Back,
                NavigatedToParameter = result,
                DisposeBag = new[] { poppedPage }
            };
        }

        private Task PopModalInternalAsync(object? result, bool animated)
        {
            var request = new NavigationInfo
            {
                Operation = NavigationOperation.PopModal,
                Direction = NavigationDirection.Back,
                Parameter = result,
                Animated = animated
            };

            return this.ExecuteAsync(request, () => this.PopModalCoreAsync(result, animated));
        }

        /// <summary>
        /// Pops the topmost modal page: a popup, or a modal page which may be wrapped in a navigation page.
        /// </summary>
        private async Task<NavigationOutcome> PopModalCoreAsync(object? result, bool animated)
        {
            var modalStack = this.context.NavigationHost.GetModalStack();
            if (modalStack.Count == 0)
            {
                throw new NavigationException("The modal stack is empty");
            }

            var modalPage = modalStack[^1];

            // A modal page may be wrapped in a navigation page which holds a stack of its own.
            var modalPages = CollectPages(modalPage).Reverse().ToArray();
            var dismissedPage = this.GetLeafPage(modalPage);

            await this.EnsureCanNavigateAsync(modalPages);

            var revealedPage = modalStack.Count > 1
                ? this.GetLeafPage(modalStack[^2])
                : this.GetLeafPage(this.context.NavigationHost.RootPage);

            this.EnsureResultIsAccepted(dismissedPage, revealedPage, result);

            await this.context.LifecycleDispatcher.NavigatedFromAsync(dismissedPage, NavigationDirection.Back);

            if (modalPage is PopupPage popupPage)
            {
                if (animated)
                {
                    await popupPage.PlayDisappearingAnimationAsync();
                }

                await this.context.NavigationHost.PopModalAsync(animated: false);
            }
            else
            {
                await this.context.NavigationHost.PopModalAsync(animated);
            }

            StoreResult(dismissedPage, result);

            return new NavigationOutcome
            {
                NavigatedToPage = revealedPage,
                NavigatedToDirection = NavigationDirection.Back,
                NavigatedToParameter = result,
                DisposeBag = modalPages
            };
        }

        private Task NavigateInternalAsync(string uri, object? parameter, bool animated)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uri);

            var request = new NavigationInfo
            {
                Operation = NavigationOperation.Navigate,
                Direction = NavigationDirection.Forward,
                PageName = uri,
                Parameter = parameter,
                Animated = animated
            };

            return this.ExecuteAsync(request, async () =>
            {
                var (segments, isAbsolute) = ParseUri(uri);

                if (parameter != null && GetSegmentQuery(segments[^1]) != null)
                {
                    throw new NavigationException(
                        $"The last segment of navigation URI \"{uri}\" carries a query string and a navigation " +
                        $"parameter was passed as well. Pass either of them, but not both.");
                }

                return isAbsolute
                    ? await this.NavigateAbsoluteAsync(segments, parameter, animated)
                    : await this.NavigateRelativeAsync(segments, parameter, animated);
            });
        }

        /// <summary>
        /// Splits a navigation URI into its segments. Deep links which carry a scheme, e.g.
        /// <c>"myapp://host/DetailPage?id=42"</c>, are reduced to their path and query string.
        /// The query string of the last segment becomes a <see cref="NavigationQueryParameters"/> parameter.
        /// </summary>
        internal static (string[] Segments, bool IsAbsolute) ParseUri(string uri)
        {
            var path = uri.Trim();

            if (Uri.TryCreate(path, UriKind.Absolute, out var absoluteUri) && absoluteUri.Scheme != Uri.UriSchemeFile)
            {
                // A deep link such as "myapp://host/DetailPage?id=42" navigates to an absolute path.
                path = absoluteUri.AbsolutePath + absoluteUri.Query;
                if (!path.StartsWith('/'))
                {
                    path = "/" + path;
                }
            }

            var isAbsolute = path.StartsWith('/');

            // The segments keep their query string: every segment carries its own parameters,
            // so that "PageA?id=1/PageB?id=2" delivers to both pages rather than to the last one only.
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (segments.Length == 0)
            {
                throw new NavigationException($"Navigation URI \"{uri}\" does not contain any segment");
            }

            return (segments, isAbsolute);
        }

        /// <summary>
        /// Returns the parameter the given page was created with.
        /// </summary>
        private static object? GetPendingParameter(Page page)
        {
            return PageNavigationContext.TryGet(page)?.PendingParameter;
        }

        /// <summary>
        /// Indicates whether a URI segment removes a page from the navigation stack rather than naming one.
        /// </summary>
        private static bool IsRemovePageSegment(string segment)
        {
            return segment == RemovePageSegment;
        }

        /// <summary>
        /// Returns the navigation name of a URI segment, i.e. the text before its query string.
        /// </summary>
        internal static string GetSegmentName(string segment)
        {
            var queryIndex = segment.IndexOf('?');
            return queryIndex < 0 ? segment : segment[..queryIndex];
        }

        /// <summary>
        /// Returns the parameters of a URI segment's query string, or <c>null</c> if it carries none.
        /// </summary>
        internal static NavigationQueryParameters? GetSegmentQuery(string segment)
        {
            var queryIndex = segment.IndexOf('?');
            return queryIndex < 0 ? null : NavigationQueryParameters.Parse(segment[(queryIndex + 1)..]);
        }

        /// <summary>
        /// Creates the page of a URI segment. A segment which carries a query string receives those
        /// parameters; otherwise the segment receives <paramref name="fallbackParameter"/>.
        /// </summary>
        private Page CreatePageFromSegment(string segment, object? fallbackParameter, NavigationResultSource? resultSource)
        {
            var parameter = (object?)GetSegmentQuery(segment) ?? fallbackParameter;
            return this.CreatePage(GetSegmentName(segment), parameter, resultSource);
        }

        private async Task<NavigationOutcome> NavigateAbsoluteAsync(string[] segments, object? parameter, bool animated)
        {
            if (segments.FirstOrDefault(segment => this.IsPopupPage(GetSegmentName(segment))) is { } popupSegment)
            {
                throw new NavigationException(
                    $"Popup page \"{GetSegmentName(popupSegment)}\" cannot be part of an absolute navigation URI, " +
                    $"because a popup is an overlay over the page tree rather than a part of it.")
                {
                    PageName = GetSegmentName(popupSegment)
                };
            }

            var sourcePage = this.context.NavigationHost.GetActivePage();

            // The whole tree is replaced: the modal pages, the root page and the children of tabs which are not
            // selected. Every page in it is asked for confirmation, topmost first - a deep link arriving over
            // unsaved changes is exactly the case a guard exists for - and disposed once the new tree is in place.
            var removedPages = this.CollectLivePages().Reverse().ToArray();
            await this.EnsureCanNavigateAsync(removedPages);

            var (rootPage, pagesToPush, finalTarget) = this.BuildPageTree(segments, parameter);

            foreach (var page in CollectPages(rootPage).Concat(pagesToPush))
            {
                await this.context.LifecycleDispatcher.InitializeAsync(page, GetPendingParameter(page));
            }

            if (sourcePage != null)
            {
                await this.context.LifecycleDispatcher.NavigatedFromAsync(sourcePage, NavigationDirection.Forward);
            }

            // Modal pages must be dismissed before the root page is replaced, otherwise the platform
            // is left with a presented page whose parent no longer exists.
            while (this.context.NavigationHost.GetModalStack().Count > 0)
            {
                await this.context.NavigationHost.PopModalAsync(animated: false);
            }

            await this.context.NavigationHost.SetRootPageAsync(rootPage);

            if (pagesToPush.Count > 0)
            {
                var navigationSurface = this.GetActiveNavigationSurfaceOrThrow();
                foreach (var page in pagesToPush)
                {
                    await this.context.NavigationHost.PushAsync(navigationSurface, page, animated: false);
                }
            }

            return new NavigationOutcome
            {
                NavigatedToPage = this.GetLeafPage(finalTarget),
                NavigatedToDirection = NavigationDirection.Forward,
                NavigatedToParameter = GetPendingParameter(finalTarget),
                DisposeBag = removedPages
            };
        }

        private async Task<NavigationOutcome> NavigateRelativeAsync(string[] segments, object? parameter, bool animated)
        {
            // A popup page is an overlay rather than an entry of a navigation stack, so it is presented as a
            // popup here as well. Pushing it onto the navigation stack would show it as an ordinary page.
            // Removal segments are skipped here and below: they are not page names, and looking one up
            // makes the registry scan the types of every loaded assembly.
            if (segments.Any(segment => !IsRemovePageSegment(segment) && this.IsPopupPage(GetSegmentName(segment))))
            {
                if (segments.Length > 1)
                {
                    throw new NavigationException(
                        $"A popup page cannot be combined with other segments of a navigation URI, because it " +
                        $"is presented as an overlay. Navigate to \"{GetSegmentName(segments[^1])}\" on its own.")
                    {
                        PageName = GetSegmentName(segments[0])
                    };
                }

                var popupPage = this.CreatePageFromSegment(segments[0], parameter, resultSource: null);
                return await this.PushModalCoreAsync(
                    popupPage, GetPendingParameter(popupPage), animated, wrapInNavigationPage: false);
            }

            // A navigation page as the first segment of a relative navigation replaces the detail page
            // of a flyout page, which is how a flyout menu switches between sections.
            if (!IsRemovePageSegment(segments[0]) &&
                this.context.NavigationRegistry.TryGetRegistration(GetSegmentName(segments[0]), out var firstRegistration) &&
                typeof(NavigationPage).IsAssignableFrom(firstRegistration.PageType))
            {
                if (this.context.NavigationHost.RootPage is FlyoutPage flyoutPage)
                {
                    return await this.ReplaceFlyoutDetailAsync(flyoutPage, segments, parameter, animated);
                }

                // Pushing a navigation page onto a navigation stack is not supported by .NET MAUI,
                // and silently pushing an empty one would be worse than failing here.
                throw new NavigationException(
                    $"A \"{segments[0]}\" segment can only start an absolute navigation URI, or replace the detail " +
                    $"page of a flyout page. Drop the segment to push \"{string.Join("/", segments.Skip(1))}\" " +
                    $"onto the current navigation stack.")
                {
                    PageName = segments[0]
                };
            }

            var removeCount = segments.TakeWhile(IsRemovePageSegment).Count();
            if (removeCount > 0)
            {
                return await this.NavigateRemovingPagesAsync(segments, removeCount, parameter, animated);
            }

            var navigationSurface = this.GetActiveNavigationSurfaceOrThrow();
            var sourcePage = this.context.NavigationHost.GetActivePage();

            var pages = new List<Page>(segments.Length);
            for (var i = 0; i < segments.Length; i++)
            {
                var isLast = i == segments.Length - 1;
                pages.Add(this.CreatePageFromSegment(segments[i], isLast ? parameter : null, resultSource: null));
            }

            foreach (var page in pages)
            {
                await this.context.LifecycleDispatcher.InitializeAsync(page, GetPendingParameter(page));
            }

            if (sourcePage != null)
            {
                await this.context.LifecycleDispatcher.NavigatedFromAsync(sourcePage, NavigationDirection.Forward);
            }

            for (var i = 0; i < pages.Count; i++)
            {
                var isLast = i == pages.Count - 1;
                await this.context.NavigationHost.PushAsync(navigationSurface, pages[i], animated && isLast);
            }

            return new NavigationOutcome
            {
                NavigatedToPage = pages[^1],
                NavigatedToDirection = NavigationDirection.Forward,
                NavigatedToParameter = GetPendingParameter(pages[^1])
            };
        }

        /// <summary>
        /// Performs a relative navigation which starts with one or more <c>".."</c> segments:
        /// the pages they refer to are removed and the segments that follow are pushed.
        /// </summary>
        private async Task<NavigationOutcome> NavigateRemovingPagesAsync(
            string[] segments, int removeCount, object? parameter, bool animated)
        {
            var pageSegments = segments.Skip(removeCount).ToArray();
            if (pageSegments.Length == 0)
            {
                throw new NavigationException(
                    $"A relative navigation must not consist of \"{RemovePageSegment}\" segments only. " +
                    $"Use {nameof(this.PopAsync)} or {nameof(this.PopToRootAsync)} to navigate back.");
            }

            var navigationSurface = this.GetActiveNavigationSurfaceOrThrow();
            var navigationStack = this.context.NavigationHost.GetNavigationStack(navigationSurface);

            if (removeCount >= navigationStack.Count)
            {
                throw new NavigationException(
                    $"Cannot remove {removeCount} page(s) from a navigation stack which holds " +
                    $"{navigationStack.Count} page(s). The root page cannot be removed.");
            }

            // Topmost first, which is the order the guards are evaluated and the pages are disposed in.
            var removedPages = navigationStack.Reverse().Take(removeCount).ToArray();
            await this.EnsureCanNavigateAsync(removedPages);

            var pages = new List<Page>(pageSegments.Length);
            for (var i = 0; i < pageSegments.Length; i++)
            {
                var isLast = i == pageSegments.Length - 1;
                pages.Add(this.CreatePageFromSegment(pageSegments[i], isLast ? parameter : null, resultSource: null));
            }

            foreach (var page in pages)
            {
                await this.context.LifecycleDispatcher.InitializeAsync(page, GetPendingParameter(page));
            }

            await this.context.LifecycleDispatcher.NavigatedFromAsync(removedPages[0], NavigationDirection.Back);

            // The new pages are pushed before the old ones are removed, so that the user never sees
            // an intermediate page which is on its way out.
            for (var i = 0; i < pages.Count; i++)
            {
                var isLast = i == pages.Count - 1;
                await this.context.NavigationHost.PushAsync(navigationSurface, pages[i], animated && isLast);
            }

            foreach (var removedPage in removedPages)
            {
                this.context.NavigationHost.RemovePage(navigationSurface, removedPage);
            }

            return new NavigationOutcome
            {
                NavigatedToPage = pages[^1],
                NavigatedToDirection = NavigationDirection.Forward,
                NavigatedToParameter = GetPendingParameter(pages[^1]),
                DisposeBag = removedPages
            };
        }

        private async Task<NavigationOutcome> ReplaceFlyoutDetailAsync(
            FlyoutPage flyoutPage, string[] segments, object? parameter, bool animated)
        {
            // Modal pages cover the flyout page, so replacing its detail page dismisses them first. They are
            // asked for confirmation before the pages of the previous detail page, topmost first.
            var modalPages = this.context.NavigationHost.GetModalStack()
                .Reverse()
                .SelectMany(modalPage => CollectPages(modalPage).Reverse())
                .ToArray();

            var previousDetailPages = flyoutPage.Detail is { } previousDetail
                ? CollectPages(previousDetail).Reverse().ToArray()
                : Array.Empty<Page>();

            var removedPages = modalPages.Concat(previousDetailPages).ToArray();
            await this.EnsureCanNavigateAsync(removedPages);

            // The page the user actually sees, which is the leaf of the topmost modal page if one is open.
            var sourcePage = this.context.NavigationHost.GetActivePage();
            var (detailPage, pagesToPush, finalTarget) = this.BuildPageTree(segments, parameter);

            foreach (var page in CollectPages(detailPage).Concat(pagesToPush))
            {
                await this.context.LifecycleDispatcher.InitializeAsync(page, GetPendingParameter(page));
            }

            if (sourcePage != null)
            {
                await this.context.LifecycleDispatcher.NavigatedFromAsync(sourcePage, NavigationDirection.Forward);
            }

            while (this.context.NavigationHost.GetModalStack().Count > 0)
            {
                await this.context.NavigationHost.PopModalAsync(animated: false);
            }

            this.context.NavigationHost.SetFlyoutDetail(flyoutPage, detailPage);

            foreach (var page in pagesToPush)
            {
                await this.context.NavigationHost.PushAsync(detailPage, page, animated: false);
            }

            return new NavigationOutcome
            {
                NavigatedToPage = this.GetLeafPage(finalTarget),
                NavigatedToDirection = NavigationDirection.Forward,
                NavigatedToParameter = GetPendingParameter(finalTarget),
                DisposeBag = removedPages
            };
        }

        /// <summary>
        /// Builds the page tree of an absolute navigation.
        /// </summary>
        /// <returns>
        /// The new root page, the pages which have to be pushed onto the root page's navigation stack,
        /// and the page which receives the navigation parameter.
        /// </returns>
        private (Page RootPage, IReadOnlyList<Page> PagesToPush, Page FinalTarget) BuildPageTree(
            string[] segments, object? parameter)
        {
            var index = 0;
            var firstRegistration = this.context.NavigationRegistry.GetRegistration(GetSegmentName(segments[index]));

            if (typeof(FlyoutPage).IsAssignableFrom(firstRegistration.PageType) && segments.Length > 1)
            {
                // The first segment is a flyout page whose detail page is composed of the segments that follow.
                var flyoutPage = (FlyoutPage)this.CreatePageFromSegment(segments[index], fallbackParameter: null, resultSource: null);
                var (detailPage, detailPagesToPush, detailTarget) = this.BuildPageTree(segments.Skip(1).ToArray(), parameter);

                flyoutPage.Detail = detailPage;
                return (flyoutPage, detailPagesToPush, detailTarget);
            }

            if (!typeof(NavigationPage).IsAssignableFrom(firstRegistration.PageType))
            {
                var rootPage = this.CreatePageFromSegment(segments[index], segments.Length == 1 ? parameter : null, resultSource: null);
                index++;

                var pages = new List<Page>();
                for (; index < segments.Length; index++)
                {
                    var isLast = index == segments.Length - 1;
                    pages.Add(this.CreatePageFromSegment(segments[index], isLast ? parameter : null, resultSource: null));
                }

                return (rootPage, pages, pages.Count > 0 ? pages[^1] : rootPage);
            }

            // The first segment is a navigation page which wraps the pages that follow.
            index++;
            if (index >= segments.Length)
            {
                var emptyNavigationPage = this.CreateNavigationPage(firstRegistration, rootChild: null);
                return (emptyNavigationPage, Array.Empty<Page>(), emptyNavigationPage);
            }

            var childPages = new List<Page>();
            for (; index < segments.Length; index++)
            {
                var isLast = index == segments.Length - 1;
                childPages.Add(this.CreatePageFromSegment(segments[index], isLast ? parameter : null, resultSource: null));
            }

            var navigationPage = this.CreateNavigationPage(firstRegistration, childPages[0]);
            return (navigationPage, childPages.Skip(1).ToArray(), childPages[^1]);
        }

        /// <summary>
        /// Wraps a modal page in the registered navigation page rather than in a plain one, so that a back
        /// navigation inside the modal context is routed through the navigation service and its guards,
        /// exactly like a back navigation on the root stack.
        /// </summary>
        private Page CreateModalNavigationPage(Page rootChild)
        {
            if (this.context.NavigationRegistry.TryGetRegistration(NavigationPageSegment, out var registration))
            {
                return this.CreateNavigationPage(registration, rootChild);
            }

            return new NavigationPage(rootChild);
        }

        private Page CreateNavigationPage(PageRegistration registration, Page? rootChild)
        {
            if (rootChild == null)
            {
                return this.context.PageFactory.CreatePage(registration);
            }

            if (registration.PageType == typeof(NavigationPage))
            {
                return new NavigationPage(rootChild);
            }

            try
            {
                return (Page)ActivatorUtilities.CreateInstance(this.context.ServiceProvider, registration.PageType, rootChild);
            }
            catch (Exception ex)
            {
                throw new NavigationException(
                    $"Failed to create navigation page \"{registration.PageType.Name}\". " +
                    $"A navigation page used as a URI segment needs a constructor which takes the root page.", ex)
                {
                    PageName = registration.Name
                };
            }
        }

        private Page CreatePage(string pageName, object? parameter, NavigationResultSource? resultSource)
        {
            var page = this.context.PageFactory.CreatePage(pageName);

            // Until the operation completes, the page belongs to it: if anything below fails, the pipeline
            // disposes it, so a failed navigation never leaves a service scope behind.
            this.pagesCreatedInCommitPhase?.Add(page);

            if (parameter != null && !this.context.LifecycleDispatcher.CanAcceptParameter(page, parameter))
            {
                throw new NavigationParameterException(
                    $"Page \"{pageName}\" cannot receive a navigation parameter of type \"{parameter.GetType().Name}\". " +
                    $"Implement {nameof(IInitialize)}<{parameter.GetType().Name}> or " +
                    $"{nameof(INavigatedTo)}<{parameter.GetType().Name}> on the page or its view model.")
                {
                    PageName = pageName
                };
            }

            var pageNavigationContext = PageNavigationContext.TryGet(page);
            if (pageNavigationContext != null)
            {
                pageNavigationContext.PendingParameter = parameter;

                if (resultSource != null)
                {
                    pageNavigationContext.ResultSource = resultSource;
                }
            }

            if (this.context.NavigationOptions.DisableSwipeBackOnGuardedPages && NavigationGuardEvaluator.HasGuard(page))
            {
                // The iOS swipe back gesture cannot be cancelled once it started, so a navigation guard could
                // not be honored. Guarded pages are therefore left with the back button as the only way back.
                Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetSwipeBackEnabled(page, false);
            }

            return page;
        }

        /// <summary>
        /// Rejects a back navigation result which nobody can receive - while the page is still on the stack,
        /// so that the navigation is cancelled rather than half-applied.
        /// </summary>
        private void EnsureResultIsAccepted(Page poppedPage, Page revealedPage, object? result)
        {
            if (result == null)
            {
                return;
            }

            if (PageNavigationContext.TryGet(poppedPage)?.ResultSource is { } resultSource)
            {
                if (resultSource.ExpectedResultType is { } expectedResultType &&
                    !expectedResultType.IsInstanceOfType(result))
                {
                    throw new NavigationParameterException(
                        $"Page \"{poppedPage.GetType().Name}\" returned a result of type " +
                        $"\"{result.GetType().Name}\", but \"{expectedResultType.Name}\" is awaited.");
                }

                return;
            }

            if (!this.context.LifecycleDispatcher.CanAcceptParameter(revealedPage, result))
            {
                throw new NavigationParameterException(
                    $"Page \"{revealedPage.GetType().Name}\" cannot receive a result of type \"{result.GetType().Name}\". " +
                    $"Implement {nameof(INavigatedTo)}<{result.GetType().Name}> on the page or its view model.");
            }
        }

        /// <summary>
        /// Hands a back navigation result to the caller awaiting it. Called once the page actually left the
        /// stack: a result stored before a pop which then failed would be delivered by the next pop, which
        /// may not carry one of its own.
        /// </summary>
        private static void StoreResult(Page poppedPage, object? result)
        {
            if (result != null && PageNavigationContext.TryGet(poppedPage)?.ResultSource is { } resultSource)
            {
                resultSource.Result = result;
            }
        }

        private async Task EnsureCanNavigateAsync(IReadOnlyList<Page> pagesToRemove)
        {
            if (!await this.context.NavigationGuardEvaluator.CanNavigateAsync(pagesToRemove))
            {
                throw new NavigationCancelledException("The navigation was cancelled by a navigation guard");
            }
        }

        private Page GetActiveNavigationSurfaceOrThrow()
        {
            return this.context.NavigationHost.GetActiveNavigationSurface()
                ?? throw new NavigationException(
                    "There is no navigation page which the operation could be applied to. " +
                    "Make sure the root page of the window is a NavigationPage.");
        }

        private Page GetLeafPage(Page? page)
        {
            return page == null
                ? throw new NavigationException("Failed to resolve the current page")
                : CollectPages(page).Last();
        }

        /// <summary>
        /// Enumerates a page and all pages it contains, outermost first.
        /// </summary>
        /// <param name="page">The page to start at.</param>
        /// <param name="includeInactiveTabs">
        /// Includes the children of a tabbed page which are not currently selected.
        /// Those pages are alive, so they take part in the application lifecycle, but they are not
        /// part of the active branch and therefore never take part in a navigation.
        /// </param>
        private static IEnumerable<Page> CollectPages(Page page, bool includeInactiveTabs = false)
        {
            yield return page;

            switch (page)
            {
                case NavigationPage navigationPage:
                    foreach (var stackPage in navigationPage.Navigation.NavigationStack.ToArray())
                    {
                        if (stackPage != null && !ReferenceEquals(stackPage, page))
                        {
                            foreach (var nested in CollectPages(stackPage, includeInactiveTabs))
                            {
                                yield return nested;
                            }
                        }
                    }

                    break;

                case FlyoutPage { Detail: { } detail }:
                    foreach (var nested in CollectPages(detail, includeInactiveTabs))
                    {
                        yield return nested;
                    }

                    break;

                case TabbedPage tabbedPage:
                    if (includeInactiveTabs)
                    {
                        foreach (var child in tabbedPage.Children.ToArray())
                        {
                            foreach (var nested in CollectPages(child, includeInactiveTabs))
                            {
                                yield return nested;
                            }
                        }
                    }
                    else if (tabbedPage.CurrentPage is { } currentPage)
                    {
                        foreach (var nested in CollectPages(currentPage))
                        {
                            yield return nested;
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Disposes the pages which the running operation created but did not manage to put into the page
        /// tree. Pages which did make it are left alone, so that a partially applied navigation does not
        /// dispose a page the user is looking at.
        /// </summary>
        private async Task DisposeOrphanedPagesAsync()
        {
            if (this.pagesCreatedInCommitPhase is not { Count: > 0 } createdPages)
            {
                return;
            }

            this.pagesCreatedInCommitPhase = null;

            var livePages = new HashSet<Page>(this.CollectLivePages());
            var orphanedPages = createdPages.Where(page => !livePages.Contains(page)).Reverse().ToArray();

            await this.DisposeRemovedPagesAsync(orphanedPages);
        }

        private Task ExecuteAsync(NavigationInfo request, Func<Task<NavigationOutcome>> commitPhase)
        {
            return this.ExecuteAsync(request, prepare: null, commitPhase);
        }

        /// <summary>
        /// Runs an operation through the navigation pipeline: marshalled to the main thread, serialized behind
        /// the running operation, and with every failure logged and raised through <see cref="NavigationFailed"/>.
        /// </summary>
        /// <param name="request">Describes the operation as it was requested.</param>
        /// <param name="prepare">
        /// Runs inside the navigation lock, before the navigating event. It decides whatever has to be decided
        /// against the page tree as it is at that moment - which page a pop applies to, which name a view model
        /// type resolves to - and returns the request which describes the operation accordingly. Returning
        /// <c>null</c> declares the operation moot, e.g. because there is no popup left to close: nothing is
        /// committed and no event is raised. Anything which can fail before the commit belongs here rather
        /// than in the public method, so that it is reported like every other failure.
        /// </param>
        /// <param name="commitPhase">Applies the operation to the page tree.</param>
        private async Task ExecuteAsync(NavigationInfo request, Func<NavigationInfo?>? prepare, Func<Task<NavigationOutcome>> commitPhase)
        {
            if (!this.context.MainThread.IsMainThread)
            {
                await this.context.MainThread.InvokeOnMainThreadAsync(() => this.ExecuteAsync(request, prepare, commitPhase));
                return;
            }

            if (IsInNavigationPhase.Value)
            {
                throw new NavigationException(
                    $"A navigation is already in progress ({request}). Navigating from a navigation guard, " +
                    $"from {nameof(IInitialize)} or from {nameof(INavigatedFrom)} is not supported. " +
                    $"Navigate from {nameof(INavigatedTo)} instead, or dispatch the navigation call.");
            }

            NavigationOutcome outcome;

            await this.semaphore.WaitAsync();

            // Started once the operation actually runs, so that the duration reported with the navigated
            // event does not include the time the operation spent queued behind another one.
            var stopwatch = Stopwatch.StartNew();
            try
            {
                IsInNavigationPhase.Value = true;
                this.pagesCreatedInCommitPhase = new List<Page>();

                if (prepare != null)
                {
                    if (prepare() is not { } preparedRequest)
                    {
                        return;
                    }

                    request = preparedRequest;
                }

                await this.OnNavigatingAsync(request);
                outcome = await commitPhase();
            }
            catch (Exception ex)
            {
                // The operation did not complete, so the pages it created never became part of the page tree
                // and nothing else would ever dispose their service scopes.
                await this.DisposeOrphanedPagesAsync();

                await this.RaiseNavigationFailedAsync(request, ex);
                throw;
            }
            finally
            {
                this.pagesCreatedInCommitPhase = null;
                IsInNavigationPhase.Value = false;
                this.semaphore.Release();
            }

            // Everything below runs outside of the navigation lock, so that a navigation which is triggered
            // from INavigatedTo is queued instead of being rejected as a re-entrant call.
            try
            {
                // The transition runs first, so that INavigatedTo is invoked once the page is actually
                // visible - which is what the interface promises. For a popup this matters twice over:
                // its content is hidden until the animation ran, so a slow or failing callback would
                // otherwise leave the user looking at an empty dimmed screen.
                if (outcome.PostCommitAction is { } postCommitAction)
                {
                    await postCommitAction();
                }

                if (outcome.NavigatedToPage is { } navigatedToPage)
                {
                    await this.context.LifecycleDispatcher.NavigatedToAsync(
                        navigatedToPage, outcome.NavigatedToDirection, outcome.NavigatedToParameter);
                }

                // The navigation is complete at this point. The event is raised before the pages which left
                // the page tree are disposed, so that a subscriber still sees them intact.
                await this.OnNavigatedAsync(request, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                // A caller awaiting a result must observe the failure rather than a default value.
                if (outcome.NavigatedToPage is { } failedPage &&
                    PageNavigationContext.TryGet(failedPage)?.ResultSource is { } resultSource)
                {
                    resultSource.TryCompleteWithException(ex);
                }

                await this.RaiseNavigationFailedAsync(request, ex);
                throw;
            }
            finally
            {
                // The pages which left the page tree are disposed even if a lifecycle callback threw,
                // because they are gone either way and nothing else would release their service scopes.
                await this.DisposeRemovedPagesAsync(outcome.DisposeBag);
            }
        }

        /// <summary>
        /// Disposes pages which left the page tree, after detaching what the navigation service subscribed
        /// to on them.
        /// </summary>
        private async Task DisposeRemovedPagesAsync(IReadOnlyList<Page> pages)
        {
            if (pages.Count == 0)
            {
                return;
            }

            foreach (var page in pages)
            {
                if (page is PopupPage popupPage)
                {
                    // Subscribed when the popup was presented. A popup which is removed by another
                    // operation, e.g. by a root page replacement, would keep the subscription otherwise.
                    popupPage.BackgroundClicked -= this.OnPopupBackgroundClicked;
                }
            }

            await this.context.NavigationCleanup.DisposePagesAsync(pages);
        }

        private async Task RaiseNavigationFailedAsync(NavigationInfo request, Exception exception)
        {
            if (exception is NavigationCancelledException)
            {
                this.context.Logger.LogDebug("{Request} was cancelled by a navigation guard", request);
            }
            else
            {
                this.context.Logger.LogError(exception, "{Request} failed with exception", request);
            }

            try
            {
                await this.OnNavigationFailedAsync(request, exception);
            }
            catch (Exception ex)
            {
                this.context.Logger.LogError(ex, "OnNavigationFailedAsync failed with exception");
            }
        }

        /// <summary>
        /// Vetoes a back navigation which the user triggered, if one of the affected pages has a navigation guard.
        /// The guard itself cannot be evaluated here, because the platform does not wait for an asynchronous
        /// decision. Therefore the platform operation is cancelled and repeated through the navigation service,
        /// which evaluates the guard properly.
        /// </summary>
        private void OnBackRequested(object? sender, BackRequestedEventArgs e)
        {
            var modalStack = this.context.NavigationHost.GetModalStack();
            var affectedPages = CollectPages(e.Page).ToArray();
            var hasGuard = affectedPages.Any(NavigationGuardEvaluator.HasGuard);

            // The platform reports a modal dismissal with the modal page itself.
            if (modalStack.Any(p => ReferenceEquals(p, e.Page)))
            {
                this.context.Logger.LogDebug(
                    "Back requested on modal page {PageType} (guarded: {HasGuard})", e.Page.GetType().Name, hasGuard);

                if (!hasGuard)
                {
                    // An unguarded modal page may be dismissed by the platform itself; the removal is
                    // compensated afterwards, which keeps the lifecycle and the service scope correct.
                    return;
                }

                e.Cancel = true;
                _ = this.PopModalGuardedAsync();
                return;
            }

            // Otherwise the request came from a navigation page, which reports the page it displays.
            // That page is popped from its own stack unless it is the root of it.
            var navigationSurface = this.context.NavigationHost.GetActiveNavigationSurface();
            var navigationStackCount = navigationSurface == null
                ? 0
                : this.context.NavigationHost.GetNavigationStack(navigationSurface).Count;
            var canPopWithinStack = navigationStackCount > 1;

            this.context.Logger.LogDebug(
                "Back requested on {PageType} (guarded: {HasGuard}, navigation surface: {SurfaceType}, stack: {StackCount})",
                e.Page.GetType().Name, hasGuard, navigationSurface?.GetType().Name, navigationStackCount);

            if (canPopWithinStack)
            {
                e.Cancel = true;
                _ = this.PopGuardedAsync();
                return;
            }

            // The page is the root of its navigation stack. If that stack belongs to a modal page, the
            // back navigation dismisses that modal page rather than doing nothing.
            if (navigationSurface != null && modalStack.Any(p => ReferenceEquals(p, navigationSurface)))
            {
                if (!hasGuard)
                {
                    return;
                }

                e.Cancel = true;
                _ = this.PopModalGuardedAsync();
                return;
            }

            // There is nothing to navigate back to, so the platform default applies: it sends the
            // application to the background or closes it. A guarded page still has to be asked first.
            if (!hasGuard)
            {
                return;
            }

            e.Cancel = true;
            _ = this.ConfirmBackFromRootPageAsync(affectedPages);
        }

        /// <summary>
        /// Evaluates the navigation guards of the root page, which the user tried to navigate back from.
        /// </summary>
        private async Task ConfirmBackFromRootPageAsync(IReadOnlyList<Page> affectedPages)
        {
            if (this.isConfirmingBackFromRootPage)
            {
                // A second back press while the guard is still asking would run a second guard, and possibly
                // show a second dialog, concurrently with the first.
                return;
            }

            this.isConfirmingBackFromRootPage = true;

            try
            {
                if (await this.context.NavigationGuardEvaluator.CanNavigateAsync(affectedPages))
                {
                    this.context.Logger.LogDebug("Back from the root page was confirmed by its guard; leaving the application");
                    await this.OnBackFromRootPageConfirmedAsync();
                }
            }
            catch (Exception ex)
            {
                this.context.Logger.LogError(ex, "The navigation guard of the root page failed with exception");
            }
            finally
            {
                this.isConfirmingBackFromRootPage = false;
            }
        }

        /// <summary>
        /// Called when the user navigated back from the root page and the navigation guards of that page
        /// permitted it. Since there is no page left to navigate to, the base implementation closes the
        /// application, which is what the platform would have done if the guards had not been evaluated.
        /// Override in order to do something else instead.
        /// </summary>
        protected virtual Task OnBackFromRootPageConfirmedAsync()
        {
#if ANDROID
            // What the platform would have done had the back press not been intercepted for the guard: the
            // activity finishes. Quitting the application instead ends the process, which is more than a
            // back press means.
            Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Finish();
#else
            Application.Current?.Quit();
#endif
            return Task.CompletedTask;
        }

        private async Task PopGuardedAsync()
        {
            try
            {
                await this.PopAsync();
            }
            catch (NavigationCancelledException)
            {
                // A navigation guard vetoed the back navigation.
            }
            catch (Exception ex)
            {
                this.context.Logger.LogError(ex, "Back navigation failed with exception");
            }
        }

        private async Task PopModalGuardedAsync()
        {
            try
            {
                await this.PopModalAsync();
            }
            catch (NavigationCancelledException)
            {
                // A navigation guard vetoed the dismissal.
            }
            catch (Exception ex)
            {
                this.context.Logger.LogError(ex, "Modal dismissal failed with exception");
            }
        }

        /// <summary>
        /// Runs the lifecycle callbacks for a tab which the user selected.
        /// </summary>
        private async void OnTabSelectedExternally(object? sender, TabSelectedEventArgs e)
        {
            var navigationInfo = new NavigationInfo
            {
                Operation = NavigationOperation.SelectTab,
                Direction = NavigationDirection.Forward,
                PageName = e.TabbedPage.CurrentPage?.GetType().Name
            };

            try
            {
                // The same pipeline as the programmatic tab selection, so that a tab tap which arrives while
                // a navigation is running is queued rather than interleaved with it.
                await this.ExecuteAsync(navigationInfo, async () =>
                {
                    if (e.PreviousTab is { } previousTab)
                    {
                        await this.context.LifecycleDispatcher.NavigatedFromAsync(
                            this.GetLeafPage(previousTab), NavigationDirection.Forward);
                    }

                    if (e.TabbedPage.CurrentPage is not { } currentTab)
                    {
                        return NavigationOutcome.Empty;
                    }

                    var targetPage = this.GetLeafPage(currentTab);

                    // Tabs which are declared in XAML are initialized when they are selected for the first time.
                    await this.context.LifecycleDispatcher.InitializeAsync(targetPage, parameter: null);

                    return new NavigationOutcome
                    {
                        NavigatedToPage = targetPage,
                        NavigatedToDirection = NavigationDirection.Forward
                    };
                });
            }
            catch (Exception)
            {
                // The failure was reported by the pipeline; a platform event has no caller to observe it.
            }
        }

        /// <summary>
        /// Closes a popup whose background the user tapped. The dismissal runs through the navigation service,
        /// so that navigation guards, lifecycle callbacks and the service scope are honored.
        /// </summary>
        private async void OnPopupBackgroundClicked(object? sender, EventArgs e)
        {
            if (sender is not PopupPage { CloseWhenBackgroundIsClicked: true } popupPage)
            {
                return;
            }

            // Only the topmost page can be dismissed. A popup which is covered by another page did not
            // receive the tap in the first place, so reacting to it would close the wrong page.
            if (!ReferenceEquals(this.context.NavigationHost.GetModalStack().LastOrDefault(), popupPage))
            {
                this.context.Logger.LogDebug(
                    "Ignoring the background tap of {PageType} because it is not the topmost page",
                    popupPage.GetType().Name);
                return;
            }

            await this.PopModalGuardedAsync();
        }

        private bool IsPopupPage(string pageName)
        {
            return this.context.NavigationRegistry.TryGetRegistration(pageName, out var registration) &&
                   typeof(PopupPage).IsAssignableFrom(registration.PageType);
        }

        private async void OnApplicationResumed(object? sender, EventArgs e)
        {
            await this.DispatchApplicationLifecycleAsync(resumed: true);
        }

        private async void OnApplicationStopped(object? sender, EventArgs e)
        {
            await this.DispatchApplicationLifecycleAsync(resumed: false);
        }

        /// <summary>
        /// Delivers the application lifecycle callbacks to every page which is currently alive.
        /// A page below the visible one can hold a timer, a subscription or hardware just as well,
        /// so restricting this to the visible page would leak those across a background cycle.
        /// </summary>
        /// <remarks>
        /// The callbacks run through the navigation pipeline as an operation of their own, so that they are
        /// serialized behind a navigation which may be mutating the page tree they walk, and so that a
        /// failure is logged and raised through <see cref="NavigationFailed"/> instead of going unobserved.
        /// </remarks>
        private async Task DispatchApplicationLifecycleAsync(bool resumed)
        {
            var request = new NavigationInfo
            {
                Operation = resumed ? NavigationOperation.ApplicationResumed : NavigationOperation.ApplicationStopped,
                Direction = NavigationDirection.None,
                Animated = false
            };

            try
            {
                await this.ExecuteAsync(request, async () =>
                {
                    foreach (var page in this.CollectLivePages())
                    {
                        await this.context.LifecycleDispatcher.ApplicationLifecycleAsync(page, resumed);
                    }

                    return NavigationOutcome.Empty;
                });
            }
            catch (Exception)
            {
                // The failure was reported by the pipeline; a platform event has no caller to observe it.
            }
        }

        /// <summary>
        /// Enumerates every page of the current window: the root page tree and all modal pages,
        /// including the children of tabbed pages which are not currently selected.
        /// </summary>
        private IReadOnlyList<Page> CollectLivePages()
        {
            var pages = new List<Page>();

            if (this.context.NavigationHost.RootPage is { } rootPage)
            {
                pages.AddRange(CollectPages(rootPage, includeInactiveTabs: true));
            }

            foreach (var modalPage in this.context.NavigationHost.GetModalStack())
            {
                pages.AddRange(CollectPages(modalPage, includeInactiveTabs: true));
            }

            return pages;
        }

        /// <summary>
        /// Compensates for pages which were removed without the navigation service being involved,
        /// e.g. by a hardware back navigation or an iOS swipe back gesture. Without this, such pages
        /// would neither receive their lifecycle callbacks nor have their service scope disposed.
        /// </summary>
        private async void OnPageRemovedExternally(object? sender, PageRemovedEventArgs e)
        {
            var request = new NavigationInfo
            {
                Operation = NavigationOperation.SystemBack,
                Direction = NavigationDirection.Back,
                PageName = e.Page.GetType().Name
            };

            try
            {
                // The compensation uses the same pipeline as a programmatic pop, so that it is serialized
                // against a navigation which may be running while the platform removes the page.
                await this.ExecuteAsync(request, async () =>
                {
                    var removedPages = CollectPages(e.Page).Reverse().ToArray();

                    await this.context.LifecycleDispatcher.NavigatedFromAsync(
                        this.GetLeafPage(e.Page), NavigationDirection.Back);

                    return new NavigationOutcome
                    {
                        NavigatedToPage = this.context.NavigationHost.GetActivePage(),
                        NavigatedToDirection = NavigationDirection.Back,
                        DisposeBag = removedPages
                    };
                });
            }
            catch (Exception)
            {
                // The failure was reported by the pipeline; a platform event has no caller to observe it.
            }
        }

        /// <summary>
        /// The target of a navigation as the caller expressed it: by navigation name, or by the view model
        /// type the page was registered with. It is resolved inside the pipeline, so that an unregistered
        /// name or type is reported through the returned task, the log and the failure event, rather than
        /// thrown out of the call.
        /// </summary>
        private readonly struct PageTarget
        {
            private readonly string? pageName;
            private readonly Type? viewModelType;

            private PageTarget(string? pageName, Type? viewModelType)
            {
                this.pageName = pageName;
                this.viewModelType = viewModelType;
            }

            public static PageTarget Named(string pageName)
            {
                return new PageTarget(pageName, viewModelType: null);
            }

            public static PageTarget ForViewModel<TViewModel>() where TViewModel : class
            {
                return new PageTarget(pageName: null, typeof(TViewModel));
            }

            /// <summary>
            /// Resolves the registration, and the navigation name under which the operation is reported.
            /// </summary>
            public (PageRegistration Registration, string PageName) Resolve(INavigationRegistry navigationRegistry)
            {
                if (this.pageName != null)
                {
                    return (navigationRegistry.GetRegistration(this.pageName), this.pageName);
                }

                var registration = navigationRegistry.GetRegistration(this.viewModelType!);
                return (registration, registration.Name ?? registration.PageType.Name);
            }

            /// <summary>
            /// The target as the caller gave it, which is what a request reports until it is resolved.
            /// </summary>
            public override string ToString()
            {
                return this.pageName ?? this.viewModelType!.Name;
            }
        }

        private sealed class NavigationOutcome
        {
            public static readonly NavigationOutcome Empty = new NavigationOutcome();

            public Page? NavigatedToPage { get; init; }

            public NavigationDirection NavigatedToDirection { get; init; }

            public object? NavigatedToParameter { get; init; }

            public IReadOnlyList<Page> DisposeBag { get; init; } = Array.Empty<Page>();

            /// <summary>
            /// An action which runs after the navigation was committed and before the navigated-to callback,
            /// e.g. the appearing animation of a popup.
            /// </summary>
            public Func<Task>? PostCommitAction { get; init; }
        }
    }
}
