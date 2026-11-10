using Microsoft.Extensions.Logging;
using Superdev.Maui.Navigation.Hosts;
using Superdev.Maui.Navigation.Internals;
using Superdev.Maui.Services;

namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Bundles the dependencies of <see cref="NavigationService"/>.
    /// </summary>
    /// <remarks>
    /// Deriving from <see cref="NavigationService"/> only requires forwarding this single parameter,
    /// so that additional dependencies can be added without breaking existing subclasses.
    /// </remarks>
    public sealed class NavigationServiceContext
    {
        internal NavigationServiceContext(
            ILogger<NavigationService> logger,
            IServiceProvider serviceProvider,
            IMainThread mainThread,
            INavigationHost navigationHost,
            INavigationRegistry navigationRegistry,
            IPageFactory pageFactory,
            LifecycleDispatcher lifecycleDispatcher,
            NavigationGuardEvaluator navigationGuardEvaluator,
            NavigationCleanup navigationCleanup,
            NavigationOptions navigationOptions)
        {
            this.Logger = logger;
            this.ServiceProvider = serviceProvider;
            this.MainThread = mainThread;
            this.NavigationHost = navigationHost;
            this.NavigationRegistry = navigationRegistry;
            this.PageFactory = pageFactory;
            this.LifecycleDispatcher = lifecycleDispatcher;
            this.NavigationGuardEvaluator = navigationGuardEvaluator;
            this.NavigationCleanup = navigationCleanup;
            this.NavigationOptions = navigationOptions;
        }

        internal ILogger Logger { get; }

        internal IServiceProvider ServiceProvider { get; }

        internal IMainThread MainThread { get; }

        internal INavigationHost NavigationHost { get; }

        internal INavigationRegistry NavigationRegistry { get; }

        internal IPageFactory PageFactory { get; }

        internal LifecycleDispatcher LifecycleDispatcher { get; }

        internal NavigationGuardEvaluator NavigationGuardEvaluator { get; }

        internal NavigationCleanup NavigationCleanup { get; }

        internal NavigationOptions NavigationOptions { get; }
    }
}
