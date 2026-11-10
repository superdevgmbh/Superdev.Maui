using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Superdev.Maui;
using Superdev.Maui.Localization;
using Superdev.Maui.Navigation;
using SuperdevMauiDemoApp.Services;
using SuperdevMauiDemoApp.Services.Validation;
using SuperdevMauiDemoApp.Translations;
using SuperdevMauiDemoApp.ViewModels;
using SuperdevMauiDemoApp.ViewModels.Navigation;
using SuperdevMauiDemoApp.Views;
using SuperdevMauiDemoApp.Views.Navigation;

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
                    o.TranslationProvider = new CompositeResxTranslationProvider(new[]
                    {
                        Strings.ResourceManager,
                        ValidationStrings.ResourceManager,
                    });
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
            localizer.LanguageChanging += (_, e) =>
            {
                Strings.Culture = e.CultureInfo;
                ValidationStrings.Culture = e.CultureInfo;
            };

            builder.Services.AddSingleton<NavigationDebugLog>();

            builder.Services.RegisterForNavigation<MainPage, MainViewModel>("MainPage");
            builder.Services.RegisterForNavigation<LabelDemoPage, LabelDemoViewModel>("LabelDemoPage");
            builder.Services.RegisterForNavigation<CardViewDemoPage, CardViewDemoViewModel>();
            builder.Services.RegisterForNavigation<DrilldownButtonListPage, DrilldownButtonListViewModel>();
            builder.Services.RegisterForNavigation<EntryDemoPage, EntryDemoViewModel>();
            builder.Services.RegisterForNavigation<InputValidationDemoPage, InputValidationDemoViewModel>();
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
            builder.Services.RegisterForNavigation<CollectionViewDemoPage, CollectionViewDemoViewModel>();
            builder.Services.RegisterForNavigation<CustomTabbedPageDemoPage>();
            builder.Services.RegisterForNavigation<PreferencesDemoPage, PreferencesDemoViewModel>();
            builder.Services.RegisterForNavigation<StylesDemoPage, StylesDemoViewModel>();
            builder.Services.RegisterForNavigation<WebViewDemoPage, WebViewDemoViewModel>();
            builder.Services.RegisterForNavigation<SearchBarDemoPage>();
            builder.Services.RegisterForNavigation<SpacingDemoPage>();
            builder.Services.RegisterForNavigation<ScrollViewDemoPage>();
            builder.Services.RegisterForNavigation<RadioButtonDemoPage>();
            builder.Services.RegisterForNavigation<ProgressBarDemoPage>();
            builder.Services.RegisterForNavigation<MarkupExtensionsDemoPage>();
            builder.Services.RegisterForNavigation<BindableItemsSourceDemoPage, BindableItemsSourceDemoViewModel>();

            // Navigation demos
            builder.Services.RegisterForNavigation<LifecycleDemoPage, LifecycleDemoViewModel>();
            builder.Services.RegisterForNavigation<LifecycleChildPage, LifecycleChildViewModel>();
            builder.Services.RegisterForNavigation<GuardDemoPage, GuardDemoViewModel>();
            builder.Services.RegisterForNavigation<ParameterDemoPage, ParameterDemoViewModel>();
            builder.Services.RegisterForNavigation<ParameterDetailPage, ParameterDetailViewModel>();
            builder.Services.RegisterForNavigation<ColorPickerPage, ColorPickerViewModel>();
            builder.Services.RegisterForNavigation<PopupDemoPage, PopupDemoViewModel>();
            builder.Services.RegisterForNavigation<ContextMenuPopupPage, ContextMenuPopupViewModel>();
            builder.Services.RegisterForNavigation<TooltipPopupPage>();
            builder.Services.RegisterForNavigation<FormPopupPage, FormPopupViewModel>();
            builder.Services.RegisterForNavigation<UriDemoPage, UriDemoViewModel>();
            builder.Services.RegisterForNavigation<DeepLinkDetailPage, DeepLinkDetailViewModel>();
            builder.Services.RegisterForNavigation<TabsDemoPage, TabsDemoViewModel>();
            builder.Services.RegisterForNavigation<TabsFirstPage>();
            builder.Services.RegisterForNavigation<TabsSecondPage>();
            builder.Services.RegisterForNavigation<FlyoutDemoPage, FlyoutDemoViewModel>();
            builder.Services.RegisterForNavigation<FlyoutHomePage, FlyoutHomeViewModel>();
            builder.Services.RegisterForNavigation<FlyoutDetailPage, FlyoutDetailViewModel>();
            builder.Services.RegisterForNavigation<LeakDemoPage, LeakDemoViewModel>();
            builder.Services.RegisterForNavigation<HeavyPage, HeavyViewModel>();

            // Demo: Transient page/viewmodel registration (instead of RegisterForNavigation)
            builder.Services.AddTransient<BehaviorDemoPage>();
            builder.Services.AddTransient<BehaviorDemoViewModel>();

            builder.Services.AddSingleton<ICountryService, CountryService>();
            builder.Services.AddSingleton<IValidationDemoService, ValidationDemoService>();
            builder.Services.AddSingleton<IClipboard>(_ => Clipboard.Default);
            builder.Services.AddSingleton<IEmail>(_ => Email.Default);

            return builder.Build();
        }
    }
}