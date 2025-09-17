using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Utils;
using Superdev.Maui.Controls;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<CustomPicker, CustomPickerHandler>;

    public class CustomPickerHandler : PickerHandler
    {
        private Color originalTextColor;
        private bool isUpdatingPlaceholderColor;

        public new static readonly PM Mapper = new PM(PickerHandler.Mapper)
        {
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

        protected override void ConnectHandler(MauiPicker platformView)
        {
            base.ConnectHandler(platformView);
            this.VirtualView.AddCleanUpEvent();

            var customPicker = (CustomPicker)this.VirtualView;
            this.originalTextColor = customPicker.TextColor;

            customPicker.SelectedIndexChanged += this.OnSelectedIndexChanged;
            customPicker.PropertyChanged += this.OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Debug.WriteLine($"CustomPicker.OnPropertyChanged: PropertyName={e.PropertyName}");

            if (e.PropertyName == nameof(Picker.TextColor))
            {
                if (!this.isUpdatingPlaceholderColor)
                {
                    var customPicker = (CustomPicker)sender;
                    this.originalTextColor = customPicker.TextColor;
                    this.UpdatePlaceholderColor(customPicker.PlaceholderColor);
                }
            }
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var customPicker = (CustomPicker)sender;
            this.UpdatePlaceholderColor(customPicker.PlaceholderColor);
        }

        private static void MapPlaceholderColor(CustomPickerHandler customPickerHandler, CustomPicker customPicker)
        {
            customPickerHandler.UpdatePlaceholderColor(customPicker.PlaceholderColor);
        }

        private void UpdatePlaceholderColor(Color placeholderColor)
        {
            if (this.originalTextColor == null)
            {
                return;
            }

            try
            {
                this.isUpdatingPlaceholderColor = true;

                var customPicker = (CustomPicker)this.VirtualView;
                if (customPicker.SelectedItem == null)
                {
                    Debug.WriteLine($"CustomPicker.UpdatePlaceholderColor: SelectedItem={customPicker.SelectedItem} --> placeholderColor");
                    customPicker.TextColor = placeholderColor;
                }
                else
                {
                    Debug.WriteLine($"CustomPicker.UpdatePlaceholderColor: SelectedItem={customPicker.SelectedItem} --> _originalTextColor");
                    customPicker.TextColor = this.originalTextColor;
                }
            }
            finally
            {
                this.isUpdatingPlaceholderColor = false;
            }
        }

        protected override void DisconnectHandler(MauiPicker platformView)
        {
            var customPicker = (CustomPicker)this.VirtualView;
            customPicker.SelectedIndexChanged -= this.OnSelectedIndexChanged;

            this.originalTextColor = null;

            base.DisconnectHandler(platformView);
        }
    }
}