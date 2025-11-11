using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<CustomPicker, CustomPickerHandler>;

    public class CustomPickerHandler : PickerHandler
    {
        public new static readonly PM Mapper = new PM(PickerHandler.Mapper)
        {
            [nameof(Picker.TitleColor)] = MapTitleColor,
            [nameof(CustomPicker.PlaceholderColor)] = MapPlaceholderColor
        };

        public CustomPickerHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public CustomPickerHandler()
            : base(Mapper)
        {
        }

        private new CustomPicker VirtualView => (CustomPicker)base.VirtualView;

        protected override void ConnectHandler(MauiPicker mauiPicker)
        {
            base.ConnectHandler(mauiPicker);
            UpdatePlaceholderColor(this.VirtualView, mauiPicker);
        }

        private static void MapPlaceholderColor(CustomPickerHandler customPickerHandler, CustomPicker customPicker)
        {
            UpdatePlaceholderColor(customPicker, customPickerHandler.PlatformView);
        }

        private static void UpdatePlaceholderColor(CustomPicker customPicker, MauiPicker mauiPicker)
        {
            if (customPicker.PlaceholderColor is Color placeholderColor)
            {
                mauiPicker.SetHintTextColor(placeholderColor.ToPlatform());
            }
        }

        public new static void MapTitleColor(IPickerHandler handler, IPicker picker)
        {
            // Ignore
        }
    }
}