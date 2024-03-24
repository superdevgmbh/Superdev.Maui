#if ANDROID
using Superdev.Maui.Platforms.Android.Handlers;
using Superdev.Maui.Platforms.Android.Services;
#elif IOS
using Superdev.Maui.Platforms.iOS.Handlers;
using Superdev.Maui.Platforms.iOS.Services;
#endif

using Superdev.Maui.Controls;
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
                handlers.AddHandler(typeof(CustomScrollView), typeof(CustomScrollViewHandler));
                handlers.AddHandler(typeof(CustomButton), typeof(CustomButtonHandler));
                //handlers.AddHandler(typeof(CustomPicker), typeof(CustomPickerHandler));
            })
            .ConfigureEffects(effects =>
            {
                //effects.Add<FocusRoutingEffect, FocusPlatformEffect>();
            });
#endif

#if ANDROID || IOS
            builder.Services.AddSingleton<IToastService, ToastService>();
            builder.Services.AddSingleton<IAppHandler, AppHandler>();
            builder.Services.AddSingleton<IClipboardService, ClipboardService>();
#endif

            builder.Services.AddSingleton<ILocalizer>(_ => Localizer.Current);
            builder.Services.AddSingleton<ITranslationProvider>(_ => ResxSingleTranslationProvider.Current);


            TranslateExtension.Init(Localizer.Current, ResxSingleTranslationProvider.Current);

            return builder;
        }
    }
}