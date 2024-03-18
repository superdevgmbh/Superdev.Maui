using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;
using UIKit;

namespace Superdev.Maui.Platforms.iOS.Handlers;

public class CustomButtonHandler : ButtonHandler
{
    protected override void ConnectHandler(UIButton platformView)
    {
        base.ConnectHandler(platformView);

        this.UpdateHorizontalTextAlignment();
    }

    public override void UpdateValue(string property)
    {
        base.UpdateValue(property);

        if (property == CustomButton.HorizontalTextAlignmentProperty.PropertyName ||
            property == CustomButton.VerticalTextAlignmentProperty.PropertyName)
        {
            this.UpdateHorizontalTextAlignment();
        }
    }

    private void UpdateHorizontalTextAlignment()
    {
        if (this.VirtualView is CustomButton virtualButton)
        {
            this.PlatformView.HorizontalAlignment = virtualButton.HorizontalTextAlignment switch
            {
                TextAlignment.Start => UIKit.UIControlContentHorizontalAlignment.Left,
                TextAlignment.Center => UIKit.UIControlContentHorizontalAlignment.Center,
                TextAlignment.End => UIKit.UIControlContentHorizontalAlignment.Right,
                _ => UIKit.UIControlContentHorizontalAlignment.Center,
            };
            this.PlatformView.VerticalAlignment = virtualButton.HorizontalTextAlignment switch
            {
                TextAlignment.Start => UIKit.UIControlContentVerticalAlignment.Top,
                TextAlignment.Center => UIKit.UIControlContentVerticalAlignment.Center,
                TextAlignment.End => UIKit.UIControlContentVerticalAlignment.Bottom,
                _ => UIKit.UIControlContentVerticalAlignment.Center,
            };
        }
    }
}