using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;
using UIKit;

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
        }

        private static void MapJustifyText(CustomLabelHandler customLabelHandler, CustomLabel customLabel)
        {
            var mauiLabel = customLabelHandler.PlatformView;

            if (customLabel.JustifyText)
            {
                mauiLabel.TextAlignment = UITextAlignment.Justified;
            }
            else
            {
                mauiLabel.TextAlignment = UITextAlignment.Left;
            }
        }
    }
}