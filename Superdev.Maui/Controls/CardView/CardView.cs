using Microsoft.Maui.Controls.Shapes;
using Superdev.Maui.Resources.Styles;

namespace Superdev.Maui.Controls.CardView
{
    public class CardView : Border
    {
        public CardView()
        {
            this.Padding = 0;

            if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                this.Stroke = Brush.Transparent;
                this.StrokeThickness = 0d;
                this.StrokeShape = new RoundRectangle
                {
                    CornerRadius = 0
                };
            }
            else
            {
                this.SetDynamicResource(StrokeProperty, ThemeConstants.CardViewStyle.HeaderDividerColor);
                this.StrokeThickness = 1d;
                this.StrokeShape = new RoundRectangle
                {
                    CornerRadius = 6
                };

                this.Shadow = new Shadow
                {
                    Brush = Brush.Gray,
                    Offset = new Point(0, 0),
                    Radius = 10f,
                    Opacity = 0.2f,
                };

                // Hack: OnPlatform lacks of support for DynamicResource bindings!
                this.SetDynamicResource(BackgroundColorProperty, ThemeConstants.CardViewStyle.BackgroundColor);
            }
        }
    }
}

