namespace Superdev.Maui.Navigation.Internals
{
    /// <summary>
    /// Holds the navigation state of a single page: its service scope, the lifecycle balancing flags
    /// and the pending result of a <c>PushForResultAsync</c> navigation.
    /// </summary>
    /// <remarks>
    /// The context is attached to the page instance, so its lifetime is exactly the page's lifetime.
    /// It is never held statically; per-page state must not be ambient.
    /// </remarks>
    internal sealed class PageNavigationContext : IAsyncDisposable
    {
        private static readonly BindableProperty ContextProperty = BindableProperty.CreateAttached(
            "NavigationContext",
            typeof(PageNavigationContext),
            typeof(PageNavigationContext),
            defaultValue: null);

        private readonly IReadOnlyList<object> ownedInstances;

        private AsyncServiceScope? serviceScope;

        /// <param name="serviceScope">The service scope of the page, or <c>null</c> for a page which was not
        /// created by the page factory, e.g. a tab child declared in XAML.</param>
        /// <param name="ownedInstances">
        /// Instances which were created outside the service container and are therefore not tracked by the
        /// scope. This context disposes them, because nothing else would.
        /// </param>
        public PageNavigationContext(AsyncServiceScope? serviceScope, IReadOnlyList<object>? ownedInstances = null)
        {
            this.serviceScope = serviceScope;
            this.ownedInstances = ownedInstances ?? Array.Empty<object>();
        }

        /// <summary>
        /// Indicates whether the one-shot initialization callback was already invoked.
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// Indicates whether the page is currently the active page.
        /// This flag makes <see cref="INavigatedTo"/> and <see cref="INavigatedFrom"/> strictly alternate,
        /// no matter how the page was navigated to or removed.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the result source of a <c>PushForResultAsync</c> navigation, if any.
        /// </summary>
        public NavigationResultSource? ResultSource { get; set; }

        /// <summary>
        /// The navigation parameter this page was created with. It is resolved once, when the page is created,
        /// so that the initialization and the navigated-to callback deliver the very same value - which matters
        /// for a multi-segment navigation, where every segment can carry its own query parameters.
        /// </summary>
        public object? PendingParameter { get; set; }

        /// <summary>
        /// Indicates whether this context was disposed already.
        /// </summary>
        public bool IsDisposed { get; private set; }

        public static void Attach(Page page, PageNavigationContext context)
        {
            page.SetValue(ContextProperty, context);
        }

        public static PageNavigationContext? TryGet(Page page)
        {
            return page.GetValue(ContextProperty) as PageNavigationContext;
        }

        /// <summary>
        /// Returns the context of the given page and attaches one if the page has none yet.
        /// Pages which the navigation service did not create - e.g. the statically declared children of a
        /// tabbed page - have no service scope, but they still take part in the lifecycle balancing.
        /// </summary>
        public static PageNavigationContext GetOrAttach(Page page)
        {
            if (TryGet(page) is { } context)
            {
                return context;
            }

            var newContext = new PageNavigationContext(serviceScope: null);
            Attach(page, newContext);
            return newContext;
        }

        public static void Detach(Page page)
        {
            page.SetValue(ContextProperty, null);
        }

        public async ValueTask DisposeAsync()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsDisposed = true;
            this.IsActive = false;

            try
            {
                try
                {
                    foreach (var ownedInstance in this.ownedInstances)
                    {
                        switch (ownedInstance)
                        {
                            case IAsyncDisposable asyncDisposable:
                                await asyncDisposable.DisposeAsync();
                                break;

                            case IDisposable disposable:
                                disposable.Dispose();
                                break;
                        }
                    }
                }
                finally
                {
                    // The scope is released even if an owned instance failed to dispose.
                    if (this.serviceScope is { } scope)
                    {
                        this.serviceScope = null;
                        await scope.DisposeAsync();
                    }
                }
            }
            finally
            {
                // Never leave a caller of PushForResultAsync waiting, no matter how the page was removed - and
                // never resume it before the page's scope is gone, which is what the result source promises.
                var resultSource = this.ResultSource;
                this.ResultSource = null;
                resultSource?.TryComplete();
            }
        }
    }
}
