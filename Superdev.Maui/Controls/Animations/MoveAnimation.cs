namespace Superdev.Maui.Controls.Animations
{
    /// <summary>
    /// Moves the popup content in from <see cref="PopupAnimationBase.PositionIn"/>
    /// and out to <see cref="PopupAnimationBase.PositionOut"/>, e.g. as a sheet which slides up from the bottom.
    /// </summary>
    public class MoveAnimation : PopupAnimationBase
    {
        /// <inheritdoc cref="MoveAnimation"/>
        public MoveAnimation()
        {
        }

        /// <inheritdoc cref="MoveAnimation"/>
        public MoveAnimation(PopupAnimationPosition positionIn, PopupAnimationPosition positionOut)
        {
            this.PositionIn = positionIn;
            this.PositionOut = positionOut;
        }
    }
}
