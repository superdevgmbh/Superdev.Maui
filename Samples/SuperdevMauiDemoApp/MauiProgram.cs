using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Superdev.Maui;
using Superdev.Maui.Localization;
using Superdev.Maui.Navigation;
using SuperdevMauiDemoApp.Services;
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
                .UseSuperdevMaui(o =>
                {
                    o.TranslationProvider = new ResxTranslationProvider(Strings.ResourceManager);
                })
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddLogging(b =>
            {
                b.ClearProviders();
                b.SetMinimumLevel(LogLevel.Trace);
                b.AddDebug();
                b.AddSimpleConsole();
            });

            var localizer = ILocalizer.Current;
            localizer.PreferencesKey = "SuperdevMauiDemoApp_AppLanguage";
            localizer.SupportedLanguages = SupportedLanguages.GetAll().ToArray();
            localizer.LanguageChanging += (_, e) => Strings.Culture = e.CultureInfo;

            builder.Services.RegisterForNavigation<MainPage, MainViewModel>("MainPage");
            builder.Services.RegisterForNavigation<LabelDemoPage, LabelDemoViewModel>("LabelDemoPage");
            builder.Services.RegisterForNavigation<CardViewDemoPage, CardViewDemoViewModel>();
            builder.Services.RegisterForNavigation<DrilldownButtonListPage, DrilldownButtonListViewModel>();
            builder.Services.RegisterForNavigation<EntryDemoPage, EntryDemoViewModel>();
            builder.Services.RegisterForNavigation<ServiceDemoPage, ServiceDemoViewModel>();
            builder.Services.RegisterForNavigation<NavigationDemoPage, NavigationDemoViewModel>("NavigationDemoPageKey");
            builder.Services.RegisterForNavigation<ViewModelErrorDemoPage, ViewModelErrorDemoViewModel>();
            builder.Services.RegisterForNavigation<SliderDemoPage>();
            builder.Services.RegisterForNavigation<ActivityIndicatorDemoPage, ActivityIndicatorDemoViewModel>();
            builder.Services.RegisterForNavigation<PickerDemoPage, PickerDemoViewModel>();
            builder.Services.RegisterForNavigation<DatePickerDemoPage, DatePickerDemoViewModel>();
            builder.Services.RegisterForNavigation<TimePickerDemoPage, TimePickerDemoViewModel>();
            builder.Services.RegisterForNavigation<EditorDemoPage, EditorDemoViewModel>();
            builder.Services.RegisterForNavigation<KeyboardDemoPage, KeyboardDemoViewModel>();
            builder.Services.RegisterForNavigation<SwitchDemoPage, SwitchDemoViewModel>();
            builder.Services.RegisterForNavigation<CheckBoxDemoPage, CheckBoxDemoViewModel>();
            builder.Services.RegisterForNavigation<ButtonDemoPage, ButtonDemoViewModel>();
            builder.Services.RegisterForNavigation<ListViewDemoPage, ListViewDemoViewModel>();
            builder.Services.RegisterForNavigation<CustomTabbedPageDemoPage>();
            builder.Services.RegisterForNavigation<PreferencesDemoPage, PreferencesDemoViewModel>();
            builder.Services.RegisterForNavigation<StylesDemoPage, StylesDemoViewModel>();
            builder.Services.RegisterForNavigation<WebViewDemoPage, WebViewDemoViewModel>();
            builder.Services.RegisterForNavigation<SearchBarDemoPage>();
            builder.Services.RegisterForNavigation<SpacingDemoPage>();
            builder.Services.RegisterForNavigation<RadioButtonDemoPage>();
            builder.Services.RegisterForNavigation<ProgressBarDemoPage>();
            builder.Services.RegisterForNavigation<MarkupExtensionsDemoPage>();
            builder.Services.RegisterForNavigation<BindableItemsSourceDemoPage, BindableItemsSourceDemoViewModel>();

            // Demo: Transient page/viewmodel registration (instead of RegisterForNavigation)
            builder.Services.AddTransient<BehaviorDemoPage>();
            builder.Services.AddTransient<BehaviorDemoViewModel>();

            builder.Services.AddSingleton<ICountryService, CountryService>();
            builder.Services.AddSingleton<IValidationService, ValidationService>();
            builder.Services.AddSingleton<IClipboard>(_ => Clipboard.Default);
            builder.Services.AddSingleton<IEmail>(_ => Email.Default);

            return builder.Build();
        }
    }
}