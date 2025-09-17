using Android.OS;
using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<CustomLabel, CustomLabelHandler>;

    public class CustomLabelHandler : LabelHandler
    {
        public new static readonly PM Mapper = new PM(LabelHandler.Mapper)
        {
            [nameof(CustomLabel.RemovePadding)] = MapRemovePadding,
            [nameof(CustomLabel.JustifyText)] = MapJustifyText,
        };

        public CustomLabelHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public CustomLabelHandler()
            : base(Mapper)
        {
        }

        private static void MapRemovePadding(CustomLabelHandler customLabelHandler, CustomLabel customLabel)
        {
            var textView = customLabelHandler.PlatformView;
            textView.SetPadding(0, 0, 0, 0);
            textView.SetIncludeFontPadding(false);
        }

        private static void MapJustifyText(CustomLabelHandler customLabelHandler, CustomLabel customLabel)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var textView = customLabelHandler.PlatformView;

            if (customLabel.JustifyText)
            {
                textView.JustificationMode = global::Android.Text.JustificationMode.InterWord;
            }
            else
            {
                textView.JustificationMode = global::Android.Text.JustificationMode.None;
            }
        }
    }
}