using Microsoft.Maui.Controls.Platform;
using Superdev.Maui.Effects;
using UIKit;

namespace Superdev.Maui.Platforms.Effects
{
    /// <summary>
    /// Raises <see cref="LongPressEffect" /> when the user long-presses the element.
    /// </summary>
    public class LongPressPlatformEffect : PlatformEffect
    {
        private UILongPressGestureRecognizer? longPressGestureRecognizer;
        private bool? previousUserInteractionEnabled;

        protected override void OnAttached()
        {
            if (this.Container is not UIView container)
            {
                return;
            }

            // A label does not receive touches by default, so the gesture would never fire on one.
            this.previousUserInteractionEnabled = container.UserInteractionEnabled;
            container.UserInteractionEnabled = true;

            this.longPressGestureRecognizer = new UILongPressGestureRecognizer(this.OnLongPress);
            container.AddGestureRecognizer(this.longPressGestureRecognizer);
        }

        protected override void OnDetached()
        {
            if (this.Container is UIView container)
            {
                if (this.longPressGestureRecognizer != null)
                {
                    container.RemoveGestureRecognizer(this.longPressGestureRecognizer);
                }

                if (this.previousUserInteractionEnabled is { } userInteractionEnabled)
                {
                    container.UserInteractionEnabled = userInteractionEnabled;
                }
            }

            this.longPressGestureRecognizer?.Dispose();
            this.longPressGestureRecognizer = null;
            this.previousUserInteractionEnabled = null;
        }

        private void OnLongPress(UILongPressGestureRecognizer gestureRecognizer)
        {
            // The recognizer reports every phase of the gesture; the command runs once, when it starts.
            if (gestureRecognizer.State == UIGestureRecognizerState.Began)
            {
                LongPressEffect.RaiseLongPress(this.Element);
            }
        }
    }
}
