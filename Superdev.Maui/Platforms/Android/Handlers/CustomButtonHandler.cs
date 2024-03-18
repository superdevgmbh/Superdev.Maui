using Google.Android.Material.Button;
using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;

namespace Superdev.Maui.Platforms.Android.Handlers;

public class CustomButtonHandler : ButtonHandler
{
    protected override void ConnectHandler(MaterialButton platformView)
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
        if (this.VirtualView is CustomButton customButton)
        {
            var horizontalFlag = customButton.HorizontalTextAlignment switch
            {
                TextAlignment.Start => global::Android.Views.GravityFlags.Left,
                TextAlignment.Center => global::Android.Views.GravityFlags.CenterHorizontal,
                TextAlignment.End => global::Android.Views.GravityFlags.Right,
                _ => global::Android.Views.GravityFlags.Center
            };
            var verticalFlag = customButton.VerticalTextAlignment switch
            {
                TextAlignment.Start => global::Android.Views.GravityFlags.Top,
                TextAlignment.Center => global::Android.Views.GravityFlags.CenterVertical,
                TextAlignment.End => global::Android.Views.GravityFlags.Bottom,
                _ => global::Android.Views.GravityFlags.Center
            };
            this.PlatformView.Gravity = horizontalFlag | verticalFlag;
        }
    }
}