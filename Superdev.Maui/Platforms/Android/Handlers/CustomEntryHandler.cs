using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;
using View = Android.Views.View;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<CustomEntry, CustomEntryHandler>;

    public class CustomEntryHandler : EntryHandler
    {
        private static readonly string[] AutofillHintOneTimeCode = { "otp", "one-time-code" };
        private static readonly string[] AutofillHintFirstName = { "firstname", "first-name", "givenname", "given-name", "cc-given-name" };
        private static readonly string[] AutofillHintLastName = { "lastname", "last-name", "familyname", "family-name", "cc-family-name" };
        private static readonly string[] AutofillHintUsername = { View.AutofillHintUsername };
        private static readonly string[] AutofillHintEmailAddress = { View.AutofillHintEmailAddress, "email" };
        private static readonly string[] AutofillHintPhone = { "phoneNumber", View.AutofillHintPhone, "tel" };
        private static readonly string[] AutofillHintPassword = { View.AutofillHintPassword };
        private static readonly string[] AutofillHintNewPassword = { "new-password" };

        public new static readonly PM Mapper = new PM(EntryHandler.Mapper)
        {
            [nameof(CustomEntry.TextContentType)] = MapTextContentType,
        };

        public CustomEntryHandler() : base(Mapper)
        {
        }

        private static void MapTextContentType(CustomEntryHandler handler, CustomEntry customEntry)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var textView = handler.PlatformView;
                if (customEntry.TextContentType == TextContentType.Default)
                {
                    textView.SetAutofillHints(autofillHints: null);
                    textView.ImportantForAutofill = ImportantForAutofill.No;
                }
                else
                {
                    var autofillHints = MapTextContentType(customEntry.TextContentType);
                    textView.SetAutofillHints(autofillHints);
                    textView.ImportantForAutofill = ImportantForAutofill.Yes;
                }
            }
        }

        private static string[] MapTextContentType(TextContentType textContentType)
        {
            // Some mapping names are taken von Android's View constants while others come from here:
            // https://developer.mozilla.org/en-US/docs/Web/HTML/Attributes/autocomplete

            if (textContentType == TextContentType.OneTimeCode)
            {
                return AutofillHintOneTimeCode;
            }
            else if (textContentType == TextContentType.FirstName)
            {
                return AutofillHintFirstName;
            }
            else if (textContentType == TextContentType.LastName)
            {
                return AutofillHintLastName;
            }
            else if (textContentType == TextContentType.Username)
            {
                return AutofillHintUsername;
            }
            else if (textContentType == TextContentType.EmailAddress)
            {
                return AutofillHintEmailAddress;
            }
            else if (textContentType == TextContentType.PhoneNumber)
            {
                return AutofillHintPhone;
            }
            else if (textContentType == TextContentType.Password)
            {
                return AutofillHintPassword;
            }
            else if (textContentType == TextContentType.NewPassword)
            {
                return AutofillHintNewPassword;
            }

            return new[] { string.Empty };
        }
    }
}