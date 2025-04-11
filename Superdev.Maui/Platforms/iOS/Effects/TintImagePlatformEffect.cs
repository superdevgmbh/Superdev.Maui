using System.ComponentModel;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Superdev.Maui.Effects;
using UIKit;

namespace Superdev.Maui.Platform.Effects
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

        private readonly NSObject tintLock = new();

        static TintImagePlatformEffect()
        {
            Logger = LoggingEnabled
                ? IPlatformApplication.Current.Services.GetService<ILogger<TintImagePlatformEffect>>()
                : new NullLogger<TintImagePlatformEffect>();
        }

        protected override void OnAttached()
        {
            this.Log("TintImageEffect.OnAttached");

            this.UpdateTintColor();
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == TintImageEffect.TintColorProperty.PropertyName ||
                args.PropertyName == Image.IsLoadingProperty.PropertyName ||
                args.PropertyName == Image.SourceProperty.PropertyName ||
                args.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
            {
                this.UpdateTintColor();
            }
        }

        private void UpdateTintColor()
        {
            try
            {
                if (!(this.Control is UIImageView imageView))
                {
                    return;
                }

                this.tintLock.InvokeOnMainThread(() =>
                {
                    if (!(imageView.Image is UIImage image))
                    {
                        return;
                    }

                    lock (this.tintLock)
                    {
                        if (TintImageEffect.GetTintColor(this.Element) is Color tintColor)
                        {
                            // If rendering mode is already set, we skip this step
                            if (image.RenderingMode != UIImageRenderingMode.AlwaysTemplate)
                            {
                                imageView.Image = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                            }
                            else
                            {
                                this.Log("UpdateTintColor RenderingMode is already set to AlwaysTemplate");
                            }

                            var oldTintColorString = $"{imageView.TintColor}";
                            var newTintColor = tintColor.ToPlatform();
                            imageView.TintColor = newTintColor;

                            this.Log($"UpdateTintColor successful with TintColor {oldTintColorString} >> {newTintColor}");
                        }
                        else
                        {
                            // If TintColor is null, we reset the rendering mode to automatic
                            imageView.Image = image.ImageWithRenderingMode(UIImageRenderingMode.Automatic);
                            imageView.TintColor = null;
                        }
                    }
                });
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

        private void Log(string message)
        {
            if (LoggingEnabled)
            {
                var automationId = !string.IsNullOrEmpty(this.Element.AutomationId) ? $"{this.Element.AutomationId}: " : "";
                Logger.LogDebug($"{automationId}{message}");
            }
        }
    }
}