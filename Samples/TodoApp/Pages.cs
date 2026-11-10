namespace TodoApp
{
    public partial class App
    {
        /// <summary>
        /// The navigation names of all pages of this app. The navigation service resolves a page by its
        /// name, so these constants keep the names in one place instead of spreading magic strings.
        /// </summary>
        public static class Pages
        {
            public const string NavigationPage = nameof(Microsoft.Maui.Controls.NavigationPage);
            public const string AppStartPage = nameof(Views.AppStartPage);
            public const string TabbedMainPage = nameof(Views.TabbedMainPage);
            public const string MainPage = nameof(Views.MainPage);
            public const string AboutPage = nameof(Views.AboutPage);
            public const string AddTodoPage = nameof(Views.Todos.AddTodoPage);
            public const string TodoDetailPage = nameof(Views.Todos.TodoDetailPage);
        }
    }
}
