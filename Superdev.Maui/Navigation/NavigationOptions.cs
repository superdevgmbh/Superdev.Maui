namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Configures the navigation behavior of <see cref="INavigationService"/>.
    /// </summary>
    public class NavigationOptions
    {
        /// <summary>
        /// Wraps pages which are pushed modally in a <see cref="NavigationPage"/>,
        /// so that further pages can be pushed within the modal context.
        /// Can be overridden per navigation call.
        /// Default: <c>true</c>.
        /// </summary>
        public bool WrapModalPagesInNavigationPage { get; set; } = true;

        /// <summary>
        /// Resolves navigation names which have no explicit registration by scanning the loaded assemblies
        /// for a matching page type and its <c>{Name}Page</c> to <c>{Name}ViewModel</c> counterpart.
        /// Disable this for trimmed or AOT-compiled apps and register all pages explicitly.
        /// Default: <c>true</c>.
        /// </summary>
        /// <seealso cref="ServiceCollectionExtensions.RegisterForNavigation{TView, TViewModel}(IServiceCollection, string)"/>
        /// <remarks>
        /// The fallback scans the loaded assemblies by type name, which a trimmed or ahead-of-time compiled
        /// build cannot follow: types which are only referenced by name may be removed. Register every page
        /// explicitly and switch this off for such builds.
        /// </remarks>
        public bool EnableConventionBasedResolution { get; set; } = true;

        /// <summary>
        /// Disables the interactive swipe back gesture on iOS for pages whose view or view model
        /// implements <see cref="IConfirmNavigation"/>.
        /// The gesture cannot be cancelled once it started, so a guard could not be honored otherwise.
        /// Default: <c>true</c>.
        /// </summary>
        public bool DisableSwipeBackOnGuardedPages { get; set; } = true;

        /// <summary>
        /// Presents modal pages whose view or view model implements <see cref="IConfirmNavigation"/> full screen
        /// on iOS, whatever presentation style the page asked for. A sheet can be dismissed with a swipe which
        /// cannot be cancelled once it started, so a guard could not be honored on one.
        /// Default: <c>true</c>.
        /// </summary>
        public bool PresentGuardedModalPagesFullScreen { get; set; } = true;

        /// <summary>
        /// Closes the flyout after its detail page was replaced, which is what a menu selection usually
        /// implies. Set to <c>false</c> in order to leave the flyout open.
        /// </summary>
        public bool CloseFlyoutAfterNavigation { get; set; } = true;
    }
}
