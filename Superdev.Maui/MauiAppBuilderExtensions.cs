#if ANDROID
#elif IOS
using Superdev.Maui.Platforms.Handlers.MauiFix;
#endif

#if ANDROID || IOS
using Superdev.Maui.Handlers;
using Superdev.Maui.Platforms.Handlers;
using Superdev.Maui.Platforms.Effects;
using Superdev.Maui.Platforms.Services;
using DatePickerHandler = Superdev.Maui.Platforms.Handlers.DatePickerHandler;
using PickerHandler = Superdev.Maui.Platforms.Handlers.PickerHandler;
using TimePickerHandler = Superdev.Maui.Platforms.Handlers.TimePickerHandler;
#endif

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Superdev.Maui.Controls;
using Superdev.Maui.Effects;
using Superdev.Maui.Localization;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;
using Superdev.Maui.Navigation.Hosts;
using Superdev.Maui.Navigation.Internals;
using Superdev.Maui.Resources.Styles;
using Superdev.Maui.Services;
using Superdev.Maui.Validation;
using IBrowser = Superdev.Maui.Services.IBrowser;

namespace Superdev.Maui
{
    public static class MauiAppBuilderExtensions
    {
        public static MauiAppBuilder UseSuperdevMaui(this MauiAppBuilder builder, Action<SuperdevMauiOptions>? options = null)
        {
            var o = new SuperdevMauiOptions();
            options?.Invoke(o);

#if ANDROID || IOS
            builder.ConfigureMauiHandlers(handlers =>
            {
                if (o.IgnoreSafeArea)
                {
                    handlers.AddHandler(typeof(Layout), typeof(CustomLayoutHandler));
                }

                handlers.AddHandler(typeof(ContentPage), typeof(ContentPageHandler));

                handlers.AddHandler(typeof(CustomLabel), typeof(CustomLabelHandler));
                handlers.AddHandler(typeof(CustomEntry), typeof(CustomEntryHandler));
                handlers.AddHandler(typeof(CustomEditor), typeof(CustomEditorHandler));

                handlers.AddHandler(typeof(CustomScrollView), typeof(CustomScrollViewHandler));
                handlers.AddHandler(typeof(CustomButton), typeof(CustomButtonHandler));
                handlers.AddHandler(typeof(CustomViewCell), typeof(CustomViewCellHandler));
                handlers.AddHandler(typeof(CustomTabbedPage), typeof(CustomTabbedPageHandler));

                handlers.AddHandler(typeof(Picker), typeof(PickerHandler));
                handlers.AddHandler(typeof(CustomPicker), typeof(CustomPickerHandler));

                handlers.AddHandler(typeof(DatePicker), typeof(DatePickerHandler));
                handlers.AddHandler(typeof(NullableDatePicker), typeof(NullableDatePickerHandler));

                handlers.AddHandler(typeof(TimePicker), typeof(TimePickerHandler));
                handlers.AddHandler(typeof(NullableTimePicker), typeof(NullableTimePickerHandler));

                handlers.AddHandler(typeof(CustomWebView), typeof(CustomWebViewHandler));
#if ANDROID
                // Handlers for Android only
                handlers.AddHandler(typeof(CustomSlider), typeof(CustomSliderHandler));
#elif IOS
                // Handlers for iOS only
                handlers.AddHandler(typeof(Entry), typeof(EntryHandler));
                handlers.AddHandler(typeof(Editor), typeof(EditorHandler));
                handlers.AddHandler(typeof(ScrollView), typeof(ScrollViewFixHandler));
                handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
                handlers.AddHandler(typeof(SearchBar), typeof(SearchBarHandler));
#endif
            })
            .ConfigureEffects(effects =>
            {
                effects.Add(typeof(TintImageEffect), typeof(TintImagePlatformEffect));
                effects.Add(typeof(LongPressEffect), typeof(LongPressPlatformEffect));
                effects.Add(typeof(EntryLineColorEffect), typeof(EntryLineColorPlatformEffect));
                effects.Add(typeof(EditorLineColorEffect), typeof(EditorLineColorPlatformEffect));
                effects.Add(typeof(PickerLineColorEffect), typeof(PickerLineColorPlatformEffect));
                effects.Add(typeof(DatePickerLineColorEffect), typeof(DatePickerLineColorPlatformEffect));
                effects.Add(typeof(TimePickerLineColorEffect), typeof(TimePickerLineColorPlatformEffect));
#if IOS
                effects.Add(typeof(SafeAreaPaddingEffect), typeof(SafeAreaPaddingPlatformEffect));
                effects.Add(typeof(SafeAreaTopPaddingEffect), typeof(SafeAreaTopPaddingPlatformEffect));
                effects.Add(typeof(SafeAreaBottomPaddingEffect), typeof(SafeAreaBottomPaddingPlatformEffect));
#endif
            })
            .ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(a => a
                    .OnResume(_ => IStatusBarService.Current.OnResume())
                    .OnResume(_ => IThemeHelper.Current.OnResume())
                    .OnResume(_ => IActivityIndicatorService.Current.OnResume()));
#endif
            });
#endif

#if ANDROID || IOS
            builder.Services.TryAddSingleton<IToastService>(_ => IToastService.Current);
            builder.Services.TryAddSingleton<IGeolocationSettings, GeolocationSettings>();
            builder.Services.TryAddSingleton<IStatusBarService>(_ => IStatusBarService.Current);
            builder.Services.TryAddSingleton<IActivityIndicatorService>(_ => IActivityIndicatorService.Current);
#endif
            builder.Services.AddSingleton(o);

            // TryAdd: a registration the app made before this call wins, a registration made afterwards wins too.

            // Microsoft.Maui
            builder.Services.TryAddSingleton<Superdev.Maui.Services.IDeviceInfo>(_ => Superdev.Maui.Services.DeviceInfo.Current);
            builder.Services.TryAddSingleton<Microsoft.Maui.Devices.IDeviceInfo>(_ => Microsoft.Maui.Devices.DeviceInfo.Current);

            builder.Services.TryAddSingleton<IDateTime>(_ => SystemDateTime.Current);
            builder.Services.TryAddSingleton<IDialogService>(_ => IDialogService.Current);
            builder.Services.TryAddSingleton<ILocalizer>(_ => ILocalizer.Current);
            builder.Services.TryAddSingleton<IPreferences>(_ => IPreferences.Current);
            builder.Services.TryAddSingleton<ITranslationProvider>(_ => o.TranslationProvider);
            builder.Services.TryAddSingleton<IMainThread>(_ => IMainThread.Current);
            builder.Services.TryAddSingleton<IDeveloperMode, DeveloperMode>();
            builder.Services.TryAddSingleton<IKeyboardService>(_ => IKeyboardService.Current);
            builder.Services.TryAddSingleton<IViewModelErrorRegistry>(_ => IViewModelErrorRegistry.Current);
            builder.Services.TryAddSingleton<IViewModelErrorHandler>(_ => IViewModelErrorHandler.Current);
            builder.Services.TryAddSingleton<IThemeHelper>(_ => IThemeHelper.Current);
            builder.Services.TryAddSingleton<IFontConverter>(_ => IFontConverter.Current);
            builder.Services.TryAddSingleton<IBrowser>(_ => IBrowser.Current);

            // Navigation
            builder.Services.TryAddSingleton(o.Navigation);
            builder.Services.RegisterForNavigation<CustomNavigationPage>(nameof(NavigationPage));
            builder.Services.TryAddSingleton<INavigationHost, MauiNavigationHost>();
            builder.Services.TryAddSingleton<INavigationRegistry, NavigationRegistry>();
            builder.Services.TryAddSingleton<IPageFactory, PageFactory>();
            builder.Services.TryAddSingleton<LifecycleDispatcher>();
            builder.Services.TryAddSingleton<NavigationGuardEvaluator>();
            builder.Services.TryAddSingleton<NavigationCleanup>();
            builder.Services.TryAddSingleton(sp => new NavigationServiceContext(
                sp.GetRequiredService<ILogger<NavigationService>>(),
                sp,
                sp.GetRequiredService<IMainThread>(),
                sp.GetRequiredService<INavigationHost>(),
                sp.GetRequiredService<INavigationRegistry>(),
                sp.GetRequiredService<IPageFactory>(),
                sp.GetRequiredService<LifecycleDispatcher>(),
                sp.GetRequiredService<NavigationGuardEvaluator>(),
                sp.GetRequiredService<NavigationCleanup>(),
                sp.GetRequiredService<NavigationOptions>()));
            builder.Services.TryAddSingleton<INavigationService, NavigationService>();

            TranslateExtension.Init(Localizer.Current, o.TranslationProvider);
            ViewModelValidation.DefaultMessageProvider = new DefaultValidationMessageProvider(o.TranslationProvider);

            return builder;
        }
    }
}