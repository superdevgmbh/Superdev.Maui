﻿#if ANDROID
using Superdev.Maui.Platforms.Android.Handlers;
#elif IOS
using Superdev.Maui.Platforms.iOS.Handlers;
#endif

using Superdev.Maui.Controls;
using Superdev.Maui.Localization;

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
            });
#endif

            builder.Services.AddSingleton<ILocalizer>(_ => Localizer.Current);
            builder.Services.AddSingleton<ITranslationProvider>(_ => ResxSingleTranslationProvider.Current);

            return builder;
        }
    }
}