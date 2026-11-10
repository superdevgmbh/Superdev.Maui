using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Superdev.Maui.Controls;
using Superdev.Maui.Navigation;
using Superdev.Maui.Navigation.Internals;
using Superdev.Maui.Services;

namespace Superdev.Maui.Tests.Navigation.TestDoubles
{
    /// <summary>
    /// Builds a <see cref="NavigationService"/> on top of a real dependency injection container
    /// and an in-memory navigation host.
    /// </summary>
    internal sealed class NavigationServiceFixture : IDisposable
    {
        private readonly ServiceProvider serviceProvider;

        private NavigationServiceFixture(
            ServiceProvider serviceProvider, NavigationOptions navigationOptions, IMainThread mainThread)
        {
            this.serviceProvider = serviceProvider;
            this.MainThread = mainThread;

            this.NavigationRegistry = new NavigationRegistry(
                NullLogger<NavigationRegistry>.Instance, serviceProvider, navigationOptions);

            this.PageFactory = new PageFactory(
                NullLogger<PageFactory>.Instance,
                serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                this.NavigationRegistry);

            this.Host = new TestNavigationHost();

            var context = new NavigationServiceContext(
                NullLogger<NavigationService>.Instance,
                serviceProvider,
                mainThread,
                this.Host,
                this.NavigationRegistry,
                this.PageFactory,
                new LifecycleDispatcher(NullLogger<LifecycleDispatcher>.Instance),
                new NavigationGuardEvaluator(NullLogger<NavigationGuardEvaluator>.Instance),
                new NavigationCleanup(NullLogger<NavigationCleanup>.Instance),
                navigationOptions);

            this.NavigationService = new TestNavigationService(context);
        }

        public TestNavigationService NavigationService { get; }

        public IMainThread MainThread { get; }

        public TestNavigationHost Host { get; }

        public INavigationRegistry NavigationRegistry { get; }

        public IPageFactory PageFactory { get; }

        public NavigationCallLog CallLog => this.serviceProvider.GetRequiredService<NavigationCallLog>();

        public T GetService<T>() where T : notnull
        {
            return this.serviceProvider.GetRequiredService<T>();
        }

        public static NavigationServiceFixture Create(
            Action<IServiceCollection>? configureServices = null,
            bool conventionBasedResolution = false,
            IMainThread? mainThread = null,
            Action<NavigationOptions>? configureOptions = null)
        {
            TestDispatcherProvider.EnsureInstalled();

            var navigationOptions = new NavigationOptions
            {
                // Tests register everything explicitly, so the assembly scan must not mask a missing registration.
                EnableConventionBasedResolution = conventionBasedResolution
            };

            configureOptions?.Invoke(navigationOptions);

            var services = new ServiceCollection();
            services.AddSingleton<NavigationCallLog>();
            services.AddSingleton<NavigationHook>();
            services.AddScoped<ScopedService>();
            services.AddSingleton<NavigationGate>();

            // Mirrors the registration of UseSuperdevMaui.
            services.RegisterForNavigation<CustomNavigationPage>(nameof(NavigationPage));
            services.RegisterForNavigation<HomePage, HomeViewModel>();
            services.RegisterForNavigation<DetailPage, DetailViewModel>();
            services.RegisterForNavigation<GuardedPage, GuardedViewModel>();
            services.RegisterForNavigation<ResultPage, ResultViewModel>();
            services.RegisterForNavigation<RedirectPage, RedirectViewModel>();
            services.RegisterForNavigation<BaseViewModelBackedPage, BaseViewModelBackedViewModel>();
            services.RegisterForNavigation<LifecycleAwarePage, LifecycleAwareViewModel>();
            services.RegisterForNavigation<ShellFlyoutPage>();
            services.RegisterForNavigation<DeepLinkPage, DeepLinkViewModel>();
            services.RegisterForNavigation<DeepLinkChildPage, DeepLinkChildViewModel>();
            services.RegisterForNavigation<StaticTabbedPage>();
            services.RegisterForNavigation<ContextMenuPopupPage, ContextMenuViewModel>();
            services.RegisterForNavigation<ModalPopupPage, GuardedViewModel>();
            services.RegisterForNavigation<ThrowingInitializePage, ThrowingInitializeViewModel>();
            services.RegisterForNavigation<ThrowingNavigatedToPage, ThrowingNavigatedToViewModel>();
            services.RegisterForNavigation<GatedPage, GatedViewModel>();
            services.RegisterForNavigation<ThrowingTypedInitializePage, ThrowingTypedInitializeViewModel>();
            services.RegisterForNavigation<SheetPage, SheetViewModel>();
            services.RegisterForNavigation<GuardedSheetPage, GuardedViewModel>();

            configureServices?.Invoke(services);

            var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                // The same validation a debug build of an application gets: a page which cannot be
                // constructed, or a scoped service captured by a singleton, fails here rather than on device.
                ValidateScopes = true,
                ValidateOnBuild = true
            });

            return new NavigationServiceFixture(serviceProvider, navigationOptions, mainThread ?? new StaticMainThread());
        }

        /// <summary>
        /// Sets up a navigation page with the given page as its root page, and returns that root page.
        /// </summary>
        public async Task<Page> SetupRootAsync(string pageName = "HomePage")
        {
            await this.NavigationService.NavigateAsync($"/NavigationPage/{pageName}");
            this.CallLog.Entries.Clear();
            this.Host.Operations.Clear();
            return this.NavigationService.CurrentPage!;
        }

        public void Dispose()
        {
            this.serviceProvider.Dispose();
        }
    }
}
