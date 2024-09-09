using System.ComponentModel;
using Android.Graphics;
using Android.Widget;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Color = Microsoft.Maui.Graphics.Color;

namespace Superdev.Maui.Platforms.Effects
{
    public class TintImagePlatformEffect : PlatformEffect
    {
        private const bool LoggingEnabled
#if DEBUG
            = false;
#else
            = false;
#endif

        private static readonly ILogger Logger;

        static TintImagePlatformEffect()
        {
            Logger = LoggingEnabled
                ? IPlatformApplication.Current.Services.GetService<ILogger<TintImagePlatformEffect>>()
                : new NullLogger<TintImagePlatformEffect>();
        }

        protected override void OnAttached()
        {
            UpdateTintColor();
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == TintImageEffect.TintColorProperty.PropertyName ||
                args.PropertyName == Image.IsLoadingProperty.PropertyName ||
                args.PropertyName == Image.SourceProperty.PropertyName ||
                args.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
            {
                UpdateTintColor();
            }
        }

        private void UpdateTintColor()
        {
            try
            {
                if (!(Control is ImageView imageView))
                {
                    return;
                }

                if (TintImageEffect.GetTintColor(Element) is Color tintColor)
                {
                    var newTintColor = tintColor.ToPlatform();
                    var filter = new PorterDuffColorFilter(newTintColor, PorterDuff.Mode.SrcIn);
                    imageView.SetColorFilter(filter);
                }
                else
                {
                    imageView.SetColorFilter(null);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "UpdateTintColor failed with exception");
            }
        }

        protected override void OnDetached()
        {
            this.Log("TintImageEffect.OnDetached");
        }
    }
}