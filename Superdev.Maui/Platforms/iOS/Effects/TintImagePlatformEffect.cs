using System.ComponentModel;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Superdev.Maui.Effects;
using UIKit;

namespace Superdev.Maui.Platforms.Effects
{
    public class TintImagePlatformEffect : PlatformEffect
    {
        private static readonly ILogger Logger;

        private readonly NSObject tintLock = new();

        static TintImagePlatformEffect()
        {
            Logger = IPlatformApplication.Current?.Services.GetService<ILogger<TintImagePlatformEffect>>()
                ?? NullLogger<TintImagePlatformEffect>.Instance;
        }

        /// <summary>
        /// Debug messages are written only while <see cref="TintImageEffect.LoggingEnabled"/> is on.
        /// </summary>
        private static bool IsDebugLogging => TintImageEffect.LoggingEnabled && Logger.IsEnabled(LogLevel.Debug);

        private string? ElementName => this.Element?.AutomationId is { Length: > 0 } automationId ? automationId : this.Element?.GetType().Name;

        protected override void OnAttached()
        {
            if (IsDebugLogging)
            {
                Logger.LogDebug("TintImageEffect attached to {Element}", this.ElementName);
            }

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

                            var previousTintColor = imageView.TintColor;
                            var newTintColor = tintColor.ToPlatform();
                            imageView.TintColor = newTintColor;

                            if (IsDebugLogging)
                            {
                                Logger.LogDebug("TintImageEffect on {Element} changed the tint from {PreviousTintColor} to {TintColor}",
                                    this.ElementName, previousTintColor, newTintColor);
                            }
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
                Logger.LogError(ex, "TintImageEffect on {Element} failed to update the tint", this.ElementName);
            }
        }

        protected override void OnDetached()
        {
            if (IsDebugLogging)
            {
                Logger.LogDebug("TintImageEffect detached from {Element}", this.ElementName);
            }
        }
    }
}
