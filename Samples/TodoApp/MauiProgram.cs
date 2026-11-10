using Superdev.Maui;
using TodoApp.ViewModels;
using TodoApp.ViewModels.Todos;
using TodoApp.Views;
using TodoApp.Views.Todos;

namespace TodoApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSuperdevMaui()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddLogging(b =>
            {
                b.SetMinimumLevel(LogLevel.Trace);
                b.AddDebug();
#if ANDROID
                // The debug logger reaches an attached debugger only. Logcat shows the log without one:
                // adb logcat -s TodoApp
                b.AddProvider(new LogcatLoggerProvider());
#elif IOS
                // Likewise: xcrun simctl spawn booted log stream --predicate 'process == "TodoApp"'
                b.AddProvider(new ConsoleLoggerProvider());
#endif
            });

            RegisterServices(builder.Services);
            RegisterPages(builder.Services);

            return builder.Build();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ITodoService, TodoService>();

            services.AddSingleton<IAppInfo>(_ => AppInfo.Current);
        }

        /// <summary>
        /// Registers the pages of this app together with their view models. The navigation service
        /// resolves a page by the name it is registered with, which defaults to the type name.
        /// </summary>
        private static void RegisterPages(IServiceCollection services)
        {
            services.RegisterForNavigation<AppStartPage, AppStartViewModel>();
            services.RegisterForNavigation<TabbedMainPage>(App.Pages.TabbedMainPage);
            services.RegisterForNavigation<MainPage, MainViewModel>();
            services.RegisterForNavigation<AboutPage, AboutViewModel>();
            services.RegisterForNavigation<AddTodoPage, AddTodoViewModel>();
            services.RegisterForNavigation<TodoDetailPage, TodoDetailViewModel>();
        }
    }
}
