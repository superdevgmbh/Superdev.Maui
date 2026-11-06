using Microsoft.Extensions.Logging;

namespace Superdev.Maui.Navigation.Internals
{
    /// <summary>
    /// Evaluates the <see cref="IConfirmNavigation"/> guards of the pages a navigation would remove.
    /// </summary>
    internal sealed class NavigationGuardEvaluator
    {
        private readonly ILogger logger;

        public NavigationGuardEvaluator(ILogger<NavigationGuardEvaluator> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Determines whether all given <paramref name="pagesToRemove"/> (ordered topmost first)
        /// allow the navigation. Evaluation stops at the first veto.
        /// </summary>
        public async Task<bool> CanNavigateAsync(IEnumerable<Page> pagesToRemove)
        {
            foreach (var page in pagesToRemove)
            {
                foreach (var target in GetGuards(page))
                {
                    if (!await target.CanNavigateAsync())
                    {
                        this.logger.LogDebug(
                            "Navigation was cancelled by the navigation guard of {PageType}", page.GetType().Name);
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Indicates whether the given page or its view model implements a navigation guard.
        /// </summary>
        public static bool HasGuard(Page page)
        {
            return GetGuards(page).Any();
        }

        private static IEnumerable<IConfirmNavigation> GetGuards(Page page)
        {
            if (page is IConfirmNavigation pageGuard)
            {
                yield return pageGuard;
            }

            if (page.BindingContext is IConfirmNavigation viewModelGuard &&
                !ReferenceEquals(page.BindingContext, page))
            {
                yield return viewModelGuard;
            }
        }
    }
}
