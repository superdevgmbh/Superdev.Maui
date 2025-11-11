using Microsoft.Extensions.Logging;
using Superdev.Maui.Services;

namespace Superdev.Maui.Navigation
{
    public class MauiNavigationService : INavigationService
    {
        private static readonly Lazy<INavigationService> Implementation = new Lazy<INavigationService>(CreateInstance, LazyThreadSafetyMode.PublicationOnly);

        public static INavigationService Current => Implementation.Value;

        private static INavigationService CreateInstance()
        {
            var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<MauiNavigationService>>();
            return new MauiNavigationService(logger, IPageResolver.Current);
        }

        private readonly ILogger logger;
        private readonly IPageResolver pageResolver;

        internal MauiNavigationService(
            ILogger<MauiNavigationService> logger,
            IPageResolver pageResolver)
        {
            this.logger = logger;
            this.pageResolver = pageResolver;
        }

        public Task PushAsync(string pageName, bool animated = true)
        {
            return this.PushAsync<object>(pageName, null, animated);
        }

        public async Task PushAsync<T>(string pageName, T parameter, bool animated = true)
        {
            try
            {
                var page = this.pageResolver.ResolvePage(pageName);
                var navigation = GetNavigation();
                await navigation.PushAsync(page, animated);

                if (parameter == null)
                {
                    await PageUtilities.InvokeViewAndViewModelActionAsync<INavigatedTo>(page, p => p.NavigatedToAsync());
                }
                else
                {
                    await PageUtilities.InvokeViewAndViewModelActionAsync<INavigatedTo<T>>(page, p => p.NavigatedToAsync(parameter));
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PushAsync failed with exception");
                throw;
            }
        }

        public Task PushModalAsync(string pageName, bool animated = true)
        {
            return this.PushModalAsync<object>(pageName, null, animated);
        }

        public async Task PushModalAsync<T>(string pageName, T parameter, bool animated = true)
        {
            try
            {
                var page = this.pageResolver.ResolvePage(pageName);
                var navigation = GetNavigation();
                await navigation.PushModalAsync(new NavigationPage(page), animated);

                if (parameter == null)
                {
                    await PageUtilities.InvokeViewAndViewModelActionAsync<INavigatedTo>(page, p => p.NavigatedToAsync());
                }
                else
                {
                    await PageUtilities.InvokeViewAndViewModelActionAsync<INavigatedTo<T>>(page, p => p.NavigatedToAsync(parameter));
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PushModalAsync failed with exception");
                throw;
            }
        }

        private static INavigation GetNavigation()
        {
            if (Shell.Current != null)
            {
                throw new NotSupportedException(
                    $"{nameof(MauiNavigationService)} does currently not support AppShell navigation");
            }

            if (Application.Current?.MainPage is not Page page)
            {
                throw new PageNavigationException("Application.Current.MainPage is not set");
            }

            var targetPage = GetTarget(page);
            var navigation = targetPage.Navigation;

            if (navigation.ModalStack.Count > 0)
            {
                var modalPage = GetTarget(navigation.ModalStack.Last());
                var modalNavigation = modalPage.Navigation;
                return modalNavigation;
            }

            return navigation;
        }

        private static Page GetTarget(Page target)
        {
            return target switch
            {
                FlyoutPage flyout => GetTarget(flyout.Detail),
                TabbedPage tabbed => GetTarget(tabbed.CurrentPage),
                NavigationPage navigation => GetTarget(navigation.CurrentPage) ?? navigation,
                ContentPage page => page,
                null => null,
                _ => throw new NotSupportedException($"The page type '{target.GetType().FullName}' is not supported.")
            };
        }

        public async Task PopAsync(bool animated = true)
        {
            try
            {
                var navigation = GetNavigation();
                await navigation.PopAsync(animated);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PopAsync failed with exception");
                throw;
            }
        }

        public async Task PopToRootAsync(bool recursive = true, bool acrossModals = false, bool animated = true)
        {
            try
            {
                var navigation = GetNavigation();

                var hasModalStack = navigation.ModalStack.Any();

                if (recursive)
                {
                    foreach (var page in navigation.NavigationStack.ToArray().Skip(hasModalStack ? 0 : 1).Reverse())
                    {
                        if (hasModalStack && navigation.NavigationStack.FirstOrDefault() == page)
                        {
                            await page.Navigation.PopModalAsync(animated);
                        }
                        else
                        {
                            await page.Navigation.PopAsync(animated);
                        }
                    }
                }
                else
                {
                    await navigation.PopToRootAsync(animated);
                }

                if (acrossModals && hasModalStack)
                {
                    if (!recursive)
                    {
                        await this.PopModalAsync(animated);
                    }

                    await this.PopToRootAsync(recursive, acrossModals, animated);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PopToRootAsync failed with exception");
                throw;
            }
        }

        public async Task PopModalAsync(bool animated = true)
        {
            try
            {
                var navigation = GetNavigation();
                await navigation.PopModalAsync(animated);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PopModalAsync failed with exception");
                throw;
            }
        }

        public async Task NavigateAsync(string pageName, bool animated = true)
        {
            try
            {
                var path = pageName.Trim();
                var isAbsolute = path.StartsWith('/');
                var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var pages = this.ResolvePagesForSegments(segments.First(), segments.Skip(1)).ToArray();

                var navigation = GetNavigation();

                if (isAbsolute)
                {
                    var rootPage = pages.First();
                    Application.Current.MainPage = rootPage;
                }

                foreach (var page in pages.Skip(isAbsolute ? 1 : 0))
                {
                    await navigation.PushAsync(page, animated);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "NavigateAsync failed with exception");
                throw;
            }
        }

        private IEnumerable<Page> ResolvePagesForSegments(string firstSegment, IEnumerable<string> segments)
        {
            if (firstSegment == nameof(NavigationPage))
            {
                if (segments.Any())
                {
                    var pages = this.ResolvePagesForSegments(segments.First(), segments.Skip(1));
                    var firstPage = pages.First();
                    yield return new NavigationPage(firstPage);
                    foreach (var childPage in pages.Skip(1))
                    {
                        yield return childPage;
                    }
                }
                else
                {
                    yield return new NavigationPage();
                }

                yield break;
            }

            var page = this.pageResolver.ResolvePage(firstSegment);
            yield return page;

            {
                if (segments.Any())
                {
                    var pages = this.ResolvePagesForSegments(segments.First(), segments.Skip(1));
                    foreach (var childPage in pages)
                    {
                        yield return childPage;
                    }
                }
            }
        }

        public INavigation Navigation
        {
            get
            {
                return Application.Current?.Windows[0].Page?.Navigation ?? throw new InvalidOperationException($"{nameof(Page.Navigation)} not found");
            }
        }
    }
}