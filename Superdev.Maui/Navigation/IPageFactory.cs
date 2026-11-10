namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Creates pages for navigation.
    /// </summary>
    /// <remarks>
    /// Every page is created within its own service scope. The page, its view model and all scoped services
    /// they depend on are resolved from that single scope, which the navigation service disposes as soon as
    /// the page leaves the navigation stack.
    /// </remarks>
    public interface IPageFactory
    {
        /// <summary>
        /// Creates the page registered under the navigation name <paramref name="pageName"/>.
        /// </summary>
        /// <exception cref="PageResolveException">If the page cannot be resolved or created.</exception>
        Page CreatePage(string pageName);

        /// <summary>
        /// Creates the page of the given <paramref name="registration"/>.
        /// </summary>
        /// <exception cref="PageResolveException">If the page cannot be created.</exception>
        Page CreatePage(PageRegistration registration);
    }
}
