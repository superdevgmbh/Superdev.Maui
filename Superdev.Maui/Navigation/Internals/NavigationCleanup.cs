using Microsoft.Extensions.Logging;

namespace Superdev.Maui.Navigation.Internals
{
    /// <summary>
    /// Tears down pages which left the navigation stack.
    /// </summary>
    /// <remarks>
    /// The teardown order matters: behaviors, triggers and effects hold references into the view model,
    /// so the handlers are disconnected (which is what runs
    /// <see cref="SuperdevMauiOptions.AutoCleanupPage"/>) before the service scope disposes the view model.
    /// Finally the binding context is detached, so that a page which the platform still holds on to
    /// no longer keeps its view model and its service scope alive.
    /// </remarks>
    internal sealed class NavigationCleanup
    {
        private readonly ILogger logger;

        public NavigationCleanup(ILogger<NavigationCleanup> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Tears down the given <paramref name="pages"/>, which must be ordered topmost first.
        /// </summary>
        public async Task DisposePagesAsync(IEnumerable<Page> pages)
        {
            foreach (var page in pages)
            {
                await this.DisposePageAsync(page);
            }
        }

        private async Task DisposePageAsync(Page page)
        {
            try
            {
                page.DisconnectHandlers();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to disconnect the handlers of page {PageType}", page.GetType().Name);
            }

            var context = PageNavigationContext.TryGet(page);
            if (context != null)
            {
                try
                {
                    await context.DisposeAsync();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to dispose the service scope of page {PageType}", page.GetType().Name);
                }

                PageNavigationContext.Detach(page);
            }

            try
            {
                page.BindingContext = null;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to detach the binding context of page {PageType}", page.GetType().Name);
            }

            this.logger.LogDebug("Disposed page {PageType}", page.GetType().Name);
        }
    }
}
