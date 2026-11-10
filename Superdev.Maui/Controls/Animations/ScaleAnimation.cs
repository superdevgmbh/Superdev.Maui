using Superdev.Maui.Internals;

namespace Superdev.Maui.Controls.Animations
{
    /// <summary>
    /// Scales and fades the popup content in and out, e.g. as a card which grows into view.
    /// </summary>
    public class ScaleAnimation : PopupAnimationBase
    {
        /// <inheritdoc cref="ScaleAnimation"/>
        public ScaleAnimation()
        {
        }

        /// <inheritdoc cref="ScaleAnimation"/>
        public ScaleAnimation(PopupAnimationPosition positionIn, PopupAnimationPosition positionOut)
        {
            this.PositionIn = positionIn;
            this.PositionOut = positionOut;
        }

        /// <summary>
        /// The scale the content grows from. Default: <c>0.8</c>.
        /// </summary>
        public double ScaleIn { get; set; } = 0.8;

        /// <summary>
        /// The scale the content shrinks to. Default: <c>0.8</c>.
        /// </summary>
        public double ScaleOut { get; set; } = 0.8;

        /// <inheritdoc />
        protected override void OnPreparingAppearance(View content)
        {
            content.Scale = this.ScaleIn;
        }

        /// <inheritdoc />
        protected override IEnumerable<Task> GetAppearingAnimations(View content, uint duration, Easing easing)
        {
            foreach (var animation in base.GetAppearingAnimations(content, duration, easing))
            {
                yield return animation;
            }

            yield return content.ScaleToCompatAsync(1, duration, easing);
        }

        /// <inheritdoc />
        protected override IEnumerable<Task> GetDisappearingAnimations(
            View content, double translationX, double translationY, uint duration, Easing easing)
        {
            foreach (var animation in base.GetDisappearingAnimations(content, translationX, translationY, duration, easing))
            {
                yield return animation;
            }

            yield return content.ScaleToCompatAsync(this.ScaleOut, duration, easing);
        }
    }
}
