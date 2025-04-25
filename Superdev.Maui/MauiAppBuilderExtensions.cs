#if ANDROID
using Superdev.Maui.Platforms.Android.Handlers;
using Superdev.Maui.Platforms.Android.Services;
#elif IOS
using Superdev.Maui.Platforms.iOS.Handlers;
using Superdev.Maui.Platforms.iOS.Handlers.MauiFix;
using Superdev.Maui.Platforms.iOS.Services;
#endif

#if ANDROID || IOS
using Superdev.Maui.Platform.Effects;
#endif

using Superdev.Maui.Controls;
using Superdev.Maui.Effects;
using Superdev.Maui.Localization;
using Superdev.Maui.Services;

namespace Superdev.Maui
{
    public static class MauiAppBuilderExtensions
    {
        public static MauiAppBuilder UseSuperdevMaui(this MauiAppBuilder builder)
        {
#if ANDROID || IOS
            builder.ConfigureMauiHandlers(handlers =>
                {
                    handlers.AddHandler(typeof(CustomEntry), typeof(CustomEntryHandler));
                    handlers.AddHandler(typeof(CustomEditor), typeof(CustomEditorHandler));

                    handlers.AddHandler(typeof(CustomScrollView), typeof(CustomScrollViewHandler));
                    handlers.AddHandler(typeof(CustomButton), typeof(CustomButtonHandler));
                    handlers.AddHandler(typeof(CustomViewCell), typeof(CustomViewCellHandler));
                    handlers.AddHandler(typeof(CustomTabbedPage), typeof(CustomTabbedPageHandler));

                    handlers.AddHandler(typeof(Picker), typeof(PickerHandler));
                    handlers.AddHandler(typeof(CustomPicker), typeof(CustomPickerHandler));

                    handlers.AddHandler(typeof(DatePicker), typeof(DatePickerHandler));
                    handlers.AddHandler(typeof(CustomDatePicker), typeof(CustomDatePickerHandler));
                    handlers.AddHandler(typeof(NullableDatePicker), typeof(NullableDatePickerHandler));

                    handlers.AddHandler(typeof(TimePicker), typeof(TimePickerHandler));
#if ANDROID
                    // Handlers for Android only
#elif IOS
                    // Handlers for iOS only
                    handlers.AddHandler<ScrollView, ScrollViewFixHandler>();
#endif
                })
                .ConfigureEffects(effects =>
                {
                    effects.Add(typeof(TintImageEffect), typeof(TintImagePlatformEffect));
#if IOS
                    effects.Add(typeof(SafeAreaPaddingEffect), typeof(SafeAreaPaddingPlatformEffect));
                    effects.Add(typeof(SafeAreaTopPaddingEffect), typeof(SafeAreaTopPaddingPlatformEffect));
                    effects.Add(typeof(SafeAreaBottomPaddingEffect), typeof(SafeAreaBottomPaddingPlatformEffect));

                    effects.Add(typeof(EntryLineColorEffect), typeof(EntryLineColorPlatformEffect));
                    effects.Add(typeof(EditorLineColorEffect), typeof(EditorLineColorPlatformEffect));
                    effects.Add(typeof(PickerLineColorEffect), typeof(PickerLineColorPlatformEffect));
                    effects.Add(typeof(DatePickerLineColorEffect), typeof(DatePickerLineColorPlatformEffect));
                    effects.Add(typeof(TimePickerLineColorEffect), typeof(TimePickerLineColorPlatformEffect));
#endif
                });
#endif

#if ANDROID || IOS
            builder.Services.AddSingleton<IToastService, ToastService>();
            builder.Services.AddSingleton<IAppHandler, AppHandler>();
            builder.Services.AddSingleton<IClipboardService, ClipboardService>();
            builder.Services.AddSingleton<IStatusBarService>(_ => StatusBarService.Current);
#endif

            builder.Services.AddSingleton<ILocalizer>(_ => Localizer.Current);
            builder.Services.AddSingleton<IPreferences>(_ => Superdev.Maui.Services.Preferences.Current);
            builder.Services.AddSingleton<ITranslationProvider>(_ => ResxSingleTranslationProvider.Current);
            builder.Services.AddSingleton<IMainThread, MauiMainThread>();

            TranslateExtension.Init(Localizer.Current, ResxSingleTranslationProvider.Current);

            return builder;
        }
    }
}