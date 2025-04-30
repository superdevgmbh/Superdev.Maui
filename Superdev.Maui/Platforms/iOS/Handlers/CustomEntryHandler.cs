using Foundation;
using Superdev.Maui.Controls;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<CustomEntry, CustomEntryHandler>;

    public class CustomEntryHandler : EntryHandler
    {
        public new static readonly PM Mapper = new PM(EntryHandler.Mapper)
        {
            [nameof(CustomEntry.TextContentType)] = MapTextContentType
        };

        public CustomEntryHandler() : base(Mapper)
        {
        }

        private static void MapTextContentType(CustomEntryHandler handler, CustomEntry customEntry)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                var (textContentType, keyboardType, autocapitalizationType, autocorrectionType) = MapTextContentType(customEntry.TextContentType);
                handler.PlatformView.TextContentType = textContentType;

                if (keyboardType != null)
                {
                    handler.PlatformView.KeyboardType = keyboardType.Value;
                }

                if (autocapitalizationType != null)
                {
                    handler.PlatformView.AutocapitalizationType = autocapitalizationType.Value;
                }

                if (autocorrectionType != null)
                {
                    handler.PlatformView.AutocorrectionType = autocorrectionType.Value;
                }
            }
        }

        private static (NSString, UIKeyboardType?, UITextAutocapitalizationType?, UITextAutocorrectionType?) MapTextContentType(
            TextContentType textContentType)
        {
            if (textContentType == TextContentType.Default)
            {
                return (new NSString(), null, null, null);
            }
            else if (textContentType == TextContentType.OneTimeCode)
            {
                return (UITextContentType.OneTimeCode, UIKeyboardType.NumberPad, UITextAutocapitalizationType.None,
                    UITextAutocorrectionType.No);
            }
            else if (textContentType == TextContentType.FirstName)
            {
                return (UITextContentType.GivenName, null, null, null);
            }
            else if (textContentType == TextContentType.LastName)
            {
                return (UITextContentType.FamilyName, null, null, null);
            }
            else if (textContentType == TextContentType.Username)
            {
                return (UITextContentType.Username, null, UITextAutocapitalizationType.None, UITextAutocorrectionType.No);
            }
            else if (textContentType == TextContentType.EmailAddress)
            {
                return (UITextContentType.EmailAddress, null, UITextAutocapitalizationType.None, UITextAutocorrectionType.No);
            }
            else if (textContentType == TextContentType.PhoneNumber)
            {
                return (UITextContentType.TelephoneNumber, UIKeyboardType.NumberPad, UITextAutocapitalizationType.None,
                    UITextAutocorrectionType.No);
            }
            else if (textContentType == TextContentType.Password)
            {
                return (UITextContentType.Password, null, UITextAutocapitalizationType.None, UITextAutocorrectionType.No);
            }
            else if (textContentType == TextContentType.NewPassword)
            {
                return (UITextContentType.NewPassword, null, UITextAutocapitalizationType.None, UITextAutocorrectionType.No);
            }

            return (null, null, null, null);
        }
    }
}