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

using Microsoft.Maui.LifecycleEvents;
using Superdev.Maui.Controls;
using Superdev.Maui.Effects;
using Superdev.Maui.Localization;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;
using Superdev.Maui.Resources.Styles;
using Superdev.Maui.Services;
using IBrowser = Superdev.Maui.Services.IBrowser;

namespace Superdev.Maui
{
    public static class MauiAppBuilderExtensions
    {
        public static MauiAppBuilder UseSuperdevMaui(this MauiAppBuilder builder, Action<SuperdevMauiOptions> options = null)
        {
            var o = new SuperdevMauiOptions();
            if (options != null)
            {
                options(o);
            }

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
                    // .OnStart(activity => IStatusBarService.Current.OnStart(activity))
                    .OnResume(_ => IStatusBarService.Current.OnResume())
                    .OnResume(_ => IThemeHelper.Current.OnResume())
                    .OnResume(_ => IActivityIndicatorService.Current.OnResume()));
#endif
            });
#endif

#if ANDROID || IOS
            builder.Services.AddSingleton<IToastService>(_ => IToastService.Current);
            builder.Services.AddSingleton<IGeolocationSettings, GeolocationSettings>();
            builder.Services.AddSingleton<IStatusBarService>(_ => IStatusBarService.Current);
            builder.Services.AddSingleton<IActivityIndicatorService>(_ => IActivityIndicatorService.Current);
#endif
            builder.Services.AddSingleton(o);

            // Microsoft.Maui
            builder.Services.AddSingleton<Superdev.Maui.Services.IDeviceInfo>(_ => Superdev.Maui.Services.DeviceInfo.Current);
            builder.Services.AddSingleton<Microsoft.Maui.Devices.IDeviceInfo>(_ => Microsoft.Maui.Devices.DeviceInfo.Current);

            builder.Services.AddSingleton<IDateTime>(_ => SystemDateTime.Current);
            builder.Services.AddSingleton<IDialogService>(_ => IDialogService.Current);
            builder.Services.AddSingleton<ILocalizer>(_ => ILocalizer.Current);
            builder.Services.AddSingleton<IPreferences>(_ => IPreferences.Current);
            builder.Services.AddSingleton<ITranslationProvider>(_ => ResxSingleTranslationProvider.Current);
            builder.Services.AddSingleton<IMainThread>(_ => IMainThread.Current);
            builder.Services.AddSingleton<IDeveloperMode, DeveloperMode>();
            builder.Services.AddSingleton<IKeyboardService>(_ => IKeyboardService.Current);
            builder.Services.AddSingleton<IViewModelErrorRegistry>(_ => IViewModelErrorRegistry.Current);
            builder.Services.AddSingleton<IViewModelErrorHandler>(_ => IViewModelErrorHandler.Current);
            builder.Services.AddSingleton<IThemeHelper>(_ => IThemeHelper.Current);
            builder.Services.AddSingleton<IFontConverter>(_ => IFontConverter.Current);
            builder.Services.AddSingleton<IBrowser>(_ => IBrowser.Current);
            builder.Services.AddSingleton<INavigationService>(_ => INavigationService.Current);
            builder.Services.AddSingleton<IPageResolver>(_ => IPageResolver.Current);

            TranslateExtension.Init(Localizer.Current, ResxSingleTranslationProvider.Current);

            return builder;
        }
    }
}