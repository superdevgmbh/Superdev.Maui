using TodoApp.ViewModels;

namespace TodoApp.Views
{
    public partial class TabbedMainPage : TabbedPage
    {
        /// <summary>
        /// The tabs are declared in XAML, which means the navigation service does not create them and
        /// cannot assign their view models. They are resolved from the service scope of this page instead,
        /// so they are disposed together with it. Every other page of this app gets its view model from
        /// its <c>RegisterForNavigation</c> registration.
        /// </summary>
        public TabbedMainPage(MainViewModel mainViewModel, AboutViewModel aboutViewModel)
        {
            this.InitializeComponent();

            this.TodosTab.RootPage.BindingContext = mainViewModel;
            this.AboutTab.RootPage.BindingContext = aboutViewModel;
        }
    }
}
