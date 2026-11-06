using System.ComponentModel;
using Android.Graphics;
using Android.Widget;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Superdev.Maui.Effects;
using Color = Microsoft.Maui.Graphics.Color;

namespace Superdev.Maui.Platforms.Effects
{
    public class TintImagePlatformEffect : PlatformEffect
    {
        private static readonly ILogger Logger;

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
                if (!(this.Control is ImageView imageView))
                {
                    return;
                }

                if (TintImageEffect.GetTintColor(this.Element) is Color tintColor)
                {
                    var newTintColor = tintColor.ToPlatform();
                    var filter = new PorterDuffColorFilter(newTintColor, PorterDuff.Mode.SrcIn);
                    imageView.SetColorFilter(filter);

                    if (IsDebugLogging)
                    {
                        Logger.LogDebug("TintImageEffect on {Element} set the tint to {TintColor}", this.ElementName, tintColor);
                    }
                }
                else
                {
                    imageView.SetColorFilter(null);
                }
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
