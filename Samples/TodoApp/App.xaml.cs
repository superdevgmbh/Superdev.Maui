using Superdev.Maui.Resources.Styles;

namespace TodoApp
{
    public partial class App : Application
    {
        private readonly INavigationService navigationService;

        public App(INavigationService navigationService)
        {
            this.navigationService = navigationService;

            this.InitializeComponent();

            IThemeHelper.Current.ApplyTheme("TodoApp.Theme.Light");
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // The navigation service creates the start page, so that it gets its service scope and its
            // lifecycle callbacks just like any page which is navigated to later on.
            return this.navigationService.CreateWindow($"/{Pages.AppStartPage}");
        }
    }
}
