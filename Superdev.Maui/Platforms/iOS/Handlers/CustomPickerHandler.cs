using Foundation;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<CustomPicker, CustomPickerHandler>;

    public class CustomPickerHandler : PickerHandler
    {
        static CustomPickerHandler()
        {
            Mapper.AppendToMapping(Picker.ItemsSourceProperty.PropertyName, MapItemsSource);
            Mapper.AppendToMapping(Picker.TitleProperty.PropertyName, MapTitle);
            Mapper.AppendToMapping(Picker.SelectedIndexProperty.PropertyName, MapSelectedIndex);
        }

        public new static readonly PM Mapper = new PM(PickerHandler.Mapper)
        {
            [nameof(CustomPicker.PlaceholderColor)] = MapPlaceholderColor,
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

        private static void MapItemsSource(CustomPickerHandler customPickerHandler, CustomPicker customPicker)
        {
            Debug.WriteLine("MapItemsSource");
            customPickerHandler.UpdatePlaceholderColor(customPicker.PlaceholderColor);
        }

        private static void MapTitle(CustomPickerHandler customPickerHandler, CustomPicker customPicker)
        {
            Debug.WriteLine("MapTitle");
            customPickerHandler.UpdatePlaceholderColor(customPicker.PlaceholderColor);
        }

        private static void MapSelectedIndex(CustomPickerHandler customPickerHandler, CustomPicker customPicker)
        {
            Debug.WriteLine("MapSelectedIndex");
            customPickerHandler.UpdatePlaceholderColor(customPicker.PlaceholderColor);
        }

        private static void MapPlaceholderColor(CustomPickerHandler customPickerHandler, CustomPicker customPicker)
        {
            Debug.WriteLine("MapPlaceholderColor");
            customPickerHandler.UpdatePlaceholderColor(customPicker.PlaceholderColor);
        }

        private void UpdatePlaceholderColor(Color placeholderColor)
        {
            var customPicker = this.VirtualView;

            var title = customPicker.Title;
            if (string.IsNullOrEmpty(title))
            {
                return;
            }

            if (placeholderColor == null)
            {
                return;
            }

            var control = this.PlatformView;
            control.AttributedPlaceholder = new NSAttributedString(title, null, placeholderColor.ToPlatform());
        }
    }
}