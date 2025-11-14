using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Superdev.Maui;
using Superdev.Maui.Localization;
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
                .UseSuperdevMaui()
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

            var localize = ILocalizer.Current;
            localize.LanguageChanging += (_, e) =>
            {
                Strings.Culture = e.CultureInfo;
            };

            var translationProvider = ResxSingleTranslationProvider.Current;
            translationProvider.Init(Strings.ResourceManager);

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<LabelDemoPage>();
            builder.Services.AddTransient<LabelDemoViewModel>();
            builder.Services.AddTransient<CardViewDemoPage>();
            builder.Services.AddTransient<CardViewDemoViewModel>();
            builder.Services.AddTransient<DrilldownButtonListPage>();
            builder.Services.AddTransient<DrilldownButtonListViewModel>();
            builder.Services.AddTransient<EntryDemoPage>();
            builder.Services.AddTransient<EntryDemoViewModel>();
            builder.Services.AddTransient<ServiceDemoPage>();
            builder.Services.AddTransient<ServiceDemoViewModel>();
            builder.Services.AddTransient<NavigationDemoPage>();
            builder.Services.AddTransient<NavigationDemoViewModel>();
            builder.Services.AddTransient<ViewModelErrorDemoPage>();
            builder.Services.AddTransient<ViewModelErrorDemoViewModel>();
            builder.Services.AddTransient<ActivityIndicatorDemoPage>();
            builder.Services.AddTransient<ActivityIndicatorDemoViewModel>();
            builder.Services.AddTransient<PickerDemoPage>();
            builder.Services.AddTransient<PickerDemoViewModel>();
            builder.Services.AddTransient<DatePickerDemoPage>();
            builder.Services.AddTransient<DatePickerDemoViewModel>();
            builder.Services.AddTransient<TimePickerDemoPage>();
            builder.Services.AddTransient<TimePickerDemoViewModel>();
            builder.Services.AddTransient<EditorDemoPage>();
            builder.Services.AddTransient<EditorDemoViewModel>();
            builder.Services.AddTransient<KeyboardDemoPage>();
            builder.Services.AddTransient<KeyboardDemoViewModel>();
            builder.Services.AddTransient<SwitchDemoPage>();
            builder.Services.AddTransient<SwitchDemoViewModel>();
            builder.Services.AddTransient<CheckBoxDemoPage>();
            builder.Services.AddTransient<CheckBoxDemoViewModel>();
            builder.Services.AddTransient<ButtonDemoPage>();
            builder.Services.AddTransient<ButtonDemoViewModel>();
            builder.Services.AddTransient<ListViewDemoPage>();
            builder.Services.AddTransient<ListViewDemoViewModel>();
            builder.Services.AddTransient<CustomTabbedPageDemoPage>();
            builder.Services.AddTransient<PreferencesDemoPage>();
            builder.Services.AddTransient<PreferencesDemoViewModel>();
            builder.Services.AddTransient<StylesDemoPage>();
            builder.Services.AddTransient<StylesDemoViewModel>();
            builder.Services.AddTransient<SliderDemoPage>();
            builder.Services.AddTransient<WebViewDemoPage>();
            builder.Services.AddTransient<WebViewDemoViewModel>();
            builder.Services.AddTransient<SearchBarDemoPage>();
            builder.Services.AddTransient<SpacingDemoPage>();
            builder.Services.AddTransient<RadioButtonDemoPage>();
            builder.Services.AddTransient<ProgressBarDemoPage>();
            builder.Services.AddTransient<BehaviorDemoPage>();
            builder.Services.AddTransient<BehaviorDemoViewModel>();
            builder.Services.AddTransient<MarkupExtensionsDemoPage>();
            builder.Services.AddTransient<BindableItemsSourceDemoPage>();
            builder.Services.AddTransient<BindableItemsSourceDemoViewModel>();

            builder.Services.AddSingleton<ICountryService, CountryService>();
            builder.Services.AddSingleton<IValidationService, ValidationService>();
            builder.Services.AddSingleton<IClipboard>(_ => Clipboard.Default);
            builder.Services.AddSingleton<IEmail>(_ => Email.Default);

            return builder.Build();
        }
    }
}
