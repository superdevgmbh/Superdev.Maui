using AndroidX.Annotations;
using Google.Android.Material.Button;
using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<CustomButton, CustomButtonHandler>;

    public class CustomButtonHandler : ButtonHandler
    {
        public new static readonly PM Mapper = new PM(ButtonHandler.Mapper)
        {
            [nameof(CustomButton.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
            [nameof(CustomButton.VerticalTextAlignment)] = MapVerticalTextAlignment,
            [nameof(CustomButton.AllCaps)] = MapAllCaps,
        };

        public CustomButtonHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public CustomButtonHandler()
            : base(Mapper)
        {
        }

        protected override void ConnectHandler(MaterialButton platformView)
        {
            base.ConnectHandler(platformView);

            var customButton = (CustomButton)this.VirtualView;
            UpdateHorizontalTextAlignment(customButton, platformView);
        }

        public override void UpdateValue(string? property)
        {
            base.UpdateValue(property);

            if (property == CustomButton.HorizontalTextAlignmentProperty.PropertyName ||
                property == CustomButton.VerticalTextAlignmentProperty.PropertyName)
            {
                if (this.VirtualView is CustomButton customButton && this.PlatformView is MaterialButton platformView)
                {
                    UpdateHorizontalTextAlignment(customButton, platformView);
                }
            }
        }

        private static void MapVerticalTextAlignment(CustomButtonHandler customButtonHandler, CustomButton customButton)
        {
            UpdateHorizontalTextAlignment(customButton, customButtonHandler.PlatformView);
        }

        private static void MapHorizontalTextAlignment(CustomButtonHandler customButtonHandler, CustomButton customButton)
        {
            UpdateHorizontalTextAlignment(customButton, customButtonHandler.PlatformView);
        }

        private static void UpdateHorizontalTextAlignment(CustomButton customButton, MaterialButton platformView)
        {
            var horizontalFlag = customButton.HorizontalTextAlignment switch
            {
                TextAlignment.Center => global::Android.Views.GravityFlags.CenterHorizontal,
                TextAlignment.End => global::Android.Views.GravityFlags.Right,
                TextAlignment.Start => global::Android.Views.GravityFlags.Left,
                _ => global::Android.Views.GravityFlags.Center
            };

            var verticalFlag = customButton.VerticalTextAlignment switch
            {
                TextAlignment.Center => global::Android.Views.GravityFlags.CenterVertical,
                TextAlignment.End => global::Android.Views.GravityFlags.Bottom,
                TextAlignment.Start => global::Android.Views.GravityFlags.Top,
                _ => global::Android.Views.GravityFlags.Center
            };

            platformView.Gravity = verticalFlag | horizontalFlag;
        }

        private static void MapAllCaps(CustomButtonHandler customButtonHandler, CustomButton customButton)
        {
            customButtonHandler.PlatformView.SetAllCaps(customButton.AllCaps);
        }
    }
}