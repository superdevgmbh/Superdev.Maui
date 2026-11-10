using System.ComponentModel;
using Superdev.Maui.Internals;

namespace Superdev.Maui.Controls.Animations
{
    /// <summary>
    /// The base class of the popup animations. It fades the content in and out and
    /// moves it in from and out to <see cref="PositionIn"/> and <see cref="PositionOut"/>.
    /// </summary>
    public abstract class PopupAnimationBase : IPopupAnimation
    {
        /// <summary>
        /// The duration of the appearing animation, in milliseconds. Default: <c>200</c>.
        /// </summary>
        public uint DurationIn { get; set; } = 200;

        /// <summary>
        /// The duration of the disappearing animation, in milliseconds. Default: <c>200</c>.
        /// </summary>
        public uint DurationOut { get; set; } = 200;

        /// <summary>
        /// The easing of the appearing animation. Default: <see cref="Easing.Linear"/>.
        /// In XAML, use the name of the easing, e.g. <c>EasingIn="SinOut"</c>.
        /// </summary>
        [TypeConverter(typeof(EasingTypeConverter))]
        public Easing EasingIn { get; set; } = Easing.Linear;

        /// <summary>
        /// The easing of the disappearing animation. Default: <see cref="Easing.Linear"/>.
        /// In XAML, use the name of the easing, e.g. <c>EasingOut="SinIn"</c>.
        /// </summary>
        [TypeConverter(typeof(EasingTypeConverter))]
        public Easing EasingOut { get; set; } = Easing.Linear;

        /// <summary>
        /// The position the content moves in from. Default: <see cref="PopupAnimationPosition.Center"/>.
        /// </summary>
        public PopupAnimationPosition PositionIn { get; set; } = PopupAnimationPosition.Center;

        /// <summary>
        /// The position the content moves out to. Default: <see cref="PopupAnimationPosition.Center"/>.
        /// </summary>
        public PopupAnimationPosition PositionOut { get; set; } = PopupAnimationPosition.Center;

        /// <inheritdoc />
        public virtual void Prepare(View content)
        {
            content.Opacity = 0;
        }

        /// <inheritdoc />
        public virtual void Reset(View content)
        {
            content.Opacity = 1;
            content.Scale = 1;
            content.TranslationX = 0;
            content.TranslationY = 0;
        }

        /// <inheritdoc />
        public virtual async Task AppearingAsync(View content, Page page)
        {
            var (translationX, translationY) = GetTranslation(this.PositionIn, page);

            content.TranslationX = translationX;
            content.TranslationY = translationY;
            this.OnPreparingAppearance(content);

            await Task.WhenAll(this.GetAppearingAnimations(content, this.DurationIn, this.EasingIn));
        }

        /// <inheritdoc />
        public virtual async Task DisappearingAsync(View content, Page page)
        {
            var (translationX, translationY) = GetTranslation(this.PositionOut, page);

            await Task.WhenAll(
                this.GetDisappearingAnimations(content, translationX, translationY, this.DurationOut, this.EasingOut));
        }

        /// <summary>
        /// Applies the initial state which is specific to the derived animation.
        /// </summary>
        protected virtual void OnPreparingAppearance(View content)
        {
        }

        /// <summary>
        /// Returns the animations which run while the content appears.
        /// </summary>
        protected virtual IEnumerable<Task> GetAppearingAnimations(View content, uint duration, Easing easing)
        {
            yield return content.FadeToCompatAsync(1, duration, easing);
            yield return content.TranslateToCompatAsync(0, 0, duration, easing);
        }

        /// <summary>
        /// Returns the animations which run while the content disappears.
        /// </summary>
        protected virtual IEnumerable<Task> GetDisappearingAnimations(
            View content, double translationX, double translationY, uint duration, Easing easing)
        {
            yield return content.FadeToCompatAsync(0, duration, easing);
            yield return content.TranslateToCompatAsync(translationX, translationY, duration, easing);
        }

        /// <summary>
        /// Returns the translation which moves the content to the given position, outside of the page.
        /// </summary>
        private static (double TranslationX, double TranslationY) GetTranslation(
            PopupAnimationPosition position, Page page)
        {
            var width = page.Width > 0 ? page.Width : 0;
            var height = page.Height > 0 ? page.Height : 0;

            return position switch
            {
                PopupAnimationPosition.Left => (-width, 0d),
                PopupAnimationPosition.Right => (width, 0d),
                PopupAnimationPosition.Top => (0d, -height),
                PopupAnimationPosition.Bottom => (0d, height),
                _ => (0d, 0d)
            };
        }
    }
}
