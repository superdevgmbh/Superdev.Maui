using System.ComponentModel;
using System.Diagnostics;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Superdev.Maui.Effects;
using UIKit;

namespace Superdev.Maui.Platforms.Effects
{
    public abstract class LineColorPlatformEffectBase : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                this.UpdateLineColor();
            }
            catch (Exception ex)
            {
                this.Log($"Cannot set property on attached control. Error: {ex.Message}");
            }
        }

        protected override void OnDetached()
        {
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(args);

            if (args.PropertyName == LineColorEffect.LineColorProperty.PropertyName ||
                args.PropertyName == "Height" ||
                args.PropertyName == "Width")
            {
                this.UpdateLineColor();
            }
        }

        private void UpdateLineColor()
        {
            if (this.Control is not UITextField control)
            {
                return;
            }

            var lineLayer = control.Layer.Sublayers
                .OfType<BorderLineLayer>()
                .SingleOrDefault();

            if (lineLayer == null)
            {
                lineLayer = new BorderLineLayer { MasksToBounds = true, BorderWidth = 1.0f };
                control.Layer.AddSublayer(lineLayer);
                control.BorderStyle = UITextBorderStyle.None;
            }

            if (this.Element is VisualElement { Height: > 0, Width: > 0 } visualElement)
            {
                var lineY = visualElement.Height - 1d;
                lineLayer.Frame = new CGRect(0d, lineY, visualElement.Width, 1d);

                var lineColor = LineColorEffect.GetLineColor(visualElement);
                if (lineColor != null)
                {
                    lineLayer.BorderColor = lineColor.ToCGColor();
                    control.TintColor = control.TextColor;
                }

                this.Log($"{visualElement.GetType().Name}:{Environment.NewLine}" +
                         $"> Height={visualElement.Height} Width={visualElement.Width}{Environment.NewLine}" +
                         $"> lineLayer: Frame.Y={lineLayer.Frame.Y}{Environment.NewLine}" +
                         $"> lineColor: {lineColor?.ToHex() ?? "null"}");
            }
        }

        [Conditional("DEBUG")]
        private void Log(string message)
        {
            Debug.WriteLine($"{this.GetType().Name}: {message}");
        }

        private class BorderLineLayer : CALayer
        {
        }
    }
}