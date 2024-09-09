using System.Reflection;

namespace SuperdevMauiDemoApp.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task PushAsync(string pageName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var pageType = assembly
                .GetTypes()
                .SingleOrDefault(t => t.Name == pageName);

            if (this.serviceProvider.GetRequiredService(pageType) is not Page page)
            {
                throw new InvalidOperationException($"Unable to resolve page {pageName}");
            }

            var viewModelName = pageName.Substring(0, pageName.LastIndexOf("Page")) + "ViewModel";
            var viewModelType = assembly
                .GetTypes()
                .SingleOrDefault(t => t.Name == viewModelName);

            if (viewModelType != null)
            {
                var viewModel = this.serviceProvider.GetService(viewModelType);
                page.BindingContext = viewModel;
            }

            return Application.Current.MainPage.Navigation.PushAsync(page);
        }
    }
}
