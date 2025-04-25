using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SampleApp.Services;
using Superdev.Maui;
using Superdev.Maui.Localization;
using SuperdevMauiDemoApp.Services;
using SuperdevMauiDemoApp.Services.Navigation;
using SuperdevMauiDemoApp.Services.Validation;
using SuperdevMauiDemoApp.Translations;
using SuperdevMauiDemoApp.ViewModels;
using SuperdevMauiDemoApp.Views;

namespace SuperdevMauiDemoApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSuperdevMaui()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var translationProvider = ResxSingleTranslationProvider.Current;
            translationProvider.Init(Strings.ResourceManager);

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<LabelDemoPage>();
            builder.Services.AddTransient<CardViewPage>();
            builder.Services.AddTransient<CardViewViewModel>();
            builder.Services.AddTransient<DrilldownButtonListPage>();
            builder.Services.AddTransient<DrilldownButtonListViewModel>();
            builder.Services.AddTransient<EntryPage>();
            builder.Services.AddTransient<EntryViewModel>();
            builder.Services.AddTransient<StatusBarServicePage>();
            builder.Services.AddTransient<StatusBarServiceViewModel>();
            builder.Services.AddTransient<PickersPage>();
            builder.Services.AddTransient<PickersViewModel>();
            builder.Services.AddTransient<ListViewDemoPage>();
            builder.Services.AddTransient<ListViewDemoViewModel>();
            builder.Services.AddTransient<CustomTabbedPageDemoPage>();
            builder.Services.AddTransient<PreferencesDemoPage>();
            builder.Services.AddTransient<PreferencesDemoViewModel>();

            builder.Services.AddSingleton<INavigationService, MauiNavigationService>();
            builder.Services.AddSingleton<IDisplayService, DisplayService>();
            builder.Services.AddSingleton<ICountryService, CountryService>();
            builder.Services.AddSingleton<IValidationService, ValidationService>();
            builder.Services.AddSingleton<IEmailService, EmailService>();

            return builder.Build();
        }
    }
}
