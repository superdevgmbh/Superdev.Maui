using Microsoft.Maui.Controls.Platform;
using Superdev.Maui.Effects;

namespace Superdev.Maui.Platforms.Effects
{
    /// <summary>
    /// Raises <see cref="LongPressEffect" /> when the user long-presses the element.
    /// </summary>
    public class LongPressPlatformEffect : PlatformEffect
    {
        private AView? platformView;
        private bool previousLongClickable;

        protected override void OnAttached()
        {
            this.platformView = this.Control ?? this.Container;
            if (this.platformView == null)
            {
                return;
            }

            this.previousLongClickable = this.platformView.LongClickable;
            this.platformView.LongClickable = true;
            this.platformView.LongClick += this.OnLongClick;
        }

        protected override void OnDetached()
        {
            if (this.platformView == null)
            {
                return;
            }

            this.platformView.LongClick -= this.OnLongClick;
            this.platformView.LongClickable = this.previousLongClickable;
            this.platformView = null;
        }

        private void OnLongClick(object? sender, AView.LongClickEventArgs e)
        {
            LongPressEffect.RaiseLongPress(this.Element);
            e.Handled = true;
        }
    }
}
