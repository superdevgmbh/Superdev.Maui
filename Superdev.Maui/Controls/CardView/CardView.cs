using Superdev.Maui.Styles;

namespace Superdev.Maui.Controls.CardView
{
    public class CardView : Frame
    {
        public CardView()
        {
            this.Padding = 0;
            this.BorderColor = Colors.Transparent;
            this.IsClippedToBounds = true;

            if (Device.RuntimePlatform == Device.iOS)
            {
                this.HasShadow = false;
                this.CornerRadius = 0;
                this.BackgroundColor = Colors.Transparent;
            }
            else
            {
                this.HasShadow = true;
                //this.BorderColor = Colors.White; // BUG: If this is not set, HasShadow has no effect
                this.CornerRadius = 6;

                // Hack: OnPlatform lacks of support for DynamicResource bindings!
                this.SetDynamicResource(BackgroundColorProperty, ThemeConstants.CardViewStyle.BackgroundColor);
                this.SetDynamicResource(BorderColorProperty, ThemeConstants.CardViewStyle.BackgroundColor);
            }
        }
    }
}

