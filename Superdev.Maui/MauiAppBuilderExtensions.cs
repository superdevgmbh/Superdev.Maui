#if ANDROID
#elif IOS
using Superdev.Maui.Platforms.Handlers.MauiFix;
#endif

#if ANDROID || IOS
using Microsoft.Maui.Handlers;
using Superdev.Maui.Handlers;
using Superdev.Maui.Platforms.Handlers;
using Superdev.Maui.Platforms.Effects;
using Superdev.Maui.Platforms.Services;
#endif

using Superdev.Maui.Controls;
using Superdev.Maui.Effects;
using Superdev.Maui.Localization;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using DatePickerHandler = Superdev.Maui.Platforms.Handlers.DatePickerHandler;
using EditorHandler = Superdev.Maui.Platforms.Handlers.EditorHandler;
using EntryHandler = Superdev.Maui.Platforms.Handlers.EntryHandler;
using PickerHandler = Superdev.Maui.Platforms.Handlers.PickerHandler;
using TimePickerHandler = Superdev.Maui.Platforms.Handlers.TimePickerHandler;

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
                handlers.AddHandler(typeof(NavigationPage), typeof(NavigationPageHandler));
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
            });
#endif

#if ANDROID || IOS
            builder.Services.AddSingleton<IToastService>(_ => IToastService.Current);
            builder.Services.AddSingleton<IAppHandler, AppHandler>();
            builder.Services.AddSingleton<IStatusBarService>(_ => IStatusBarService.Current);
            builder.Services.AddSingleton<IActivityIndicatorService>(_ => IActivityIndicatorService.Current);
#endif

            builder.Services.AddSingleton<IDateTime>(_ => SystemDateTime.Current);
            builder.Services.AddSingleton<IDialogService>(_ => IDialogService.Current);
            builder.Services.AddSingleton<ILocalizer>(_ => Localizer.Current);
            builder.Services.AddSingleton<IPreferences>(_ => Superdev.Maui.Services.Preferences.Current);
            builder.Services.AddSingleton<IDeviceInfo>(_ => DeviceInfo.Current);
            builder.Services.AddSingleton<ITranslationProvider>(_ => ResxSingleTranslationProvider.Current);
            builder.Services.AddSingleton<IMainThread>(_ => IMainThread.Current);
            builder.Services.AddSingleton<IDeveloperMode, DeveloperMode>();
            builder.Services.AddSingleton<IKeyboardService, KeyboardService>();
            builder.Services.AddSingleton<IViewModelErrorRegistry>(_ => IViewModelErrorRegistry.Current);
            builder.Services.AddSingleton<IViewModelErrorHandler>(_ => IViewModelErrorHandler.Current);

            TranslateExtension.Init(Localizer.Current, ResxSingleTranslationProvider.Current);

            return builder;
        }
    }
}