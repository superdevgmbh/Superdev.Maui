using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<CustomButton, CustomButtonHandler>;

    public class CustomButtonHandler : ButtonHandler
    {
        public new static readonly PM Mapper = new PM(ButtonHandler.Mapper)
        {
            // [nameof(CustomButton.HorizontalTextAlignment)] = MapHorizontalTextAlignment, // TODO --> Check if it works without this
            // [nameof(CustomButton.VerticalTextAlignment)] = MapVerticalTextAlignment,     // TODO --> Check if it works without this
        };

        public CustomButtonHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public CustomButtonHandler()
            : base(Mapper)
        {
        }

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
            if (this.VirtualView is CustomButton customButton)
            {
                this.PlatformView.HorizontalAlignment = customButton.HorizontalTextAlignment switch
                {
                    TextAlignment.Start => UIKit.UIControlContentHorizontalAlignment.Left,
                    TextAlignment.Center => UIKit.UIControlContentHorizontalAlignment.Center,
                    TextAlignment.End => UIKit.UIControlContentHorizontalAlignment.Right,
                    _ => UIKit.UIControlContentHorizontalAlignment.Center,
                };

                this.PlatformView.VerticalAlignment = customButton.HorizontalTextAlignment switch
                {
                    TextAlignment.Start => UIKit.UIControlContentVerticalAlignment.Top,
                    TextAlignment.Center => UIKit.UIControlContentVerticalAlignment.Center,
                    TextAlignment.End => UIKit.UIControlContentVerticalAlignment.Bottom,
                    _ => UIKit.UIControlContentVerticalAlignment.Center,
                };
            }
        }
    }
}