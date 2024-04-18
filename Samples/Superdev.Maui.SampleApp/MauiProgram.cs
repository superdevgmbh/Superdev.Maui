using Microsoft.Extensions.Logging;
using SampleApp.Services;
using Superdev.Maui.Localization;
using Superdev.Maui.SampleApp.Services;
using Superdev.Maui.SampleApp.Services.Validation;
using Superdev.Maui.SampleApp.Translations;
using Superdev.Maui.SampleApp.ViewModels;
using Superdev.Maui.SampleApp.Views;

namespace Superdev.Maui.SampleApp
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
            builder.Services.AddTransient<PickersPage>();
            builder.Services.AddTransient<PickersViewModel>();
            builder.Services.AddTransient<ListViewDemoPage>();
            builder.Services.AddTransient<ListViewDemoViewModel>();
            builder.Services.AddTransient<CustomTabbedPageDemoPage>();
            builder.Services.AddTransient<PreferencesDemoPage>();
            builder.Services.AddTransient<PreferencesDemoViewModel>();

            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<IDisplayService, DisplayService>();
            builder.Services.AddSingleton<ICountryService, CountryService>();
            builder.Services.AddSingleton<IValidationService, ValidationService>();
            builder.Services.AddSingleton<IEmailService, EmailService>();

            return builder.Build();
        }
    }
}
