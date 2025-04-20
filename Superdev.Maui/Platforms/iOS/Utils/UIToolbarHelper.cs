using CoreGraphics;
using Superdev.Maui.Controls;
using UIKit;
using Superdev.Maui.Extensions;

namespace Superdev.Maui.Platforms.iOS.Utils
{
    public static class UIToolbarHelper
    {
        public static UIToolbar CreateUIToolbar(UIBarButtonItem[] items)
        {
            var width = UIScreen.MainScreen.Bounds.Width;
            var toolbar = new UIToolbar(new CGRect(0f, 0f, width, 44f))
            {
                BarStyle = UIBarStyle.Default,
                Translucent = true,
                Items = items
            };

            return toolbar;
        }

        public static void ReplaceDoneButton(UIView inputAccessoryView, UIBarButtonItem newDoneButton)
        {
            if (inputAccessoryView is UIToolbar toolbar)
            {
                lock (toolbar)
                {
                    var doneButton = toolbar.Items.LastOrDefault(i => i.Style == UIBarButtonItemStyle.Done);
                    if (doneButton != null)
                    {
                        // Redirect the old done button to the new one
                        newDoneButton.Target = doneButton.Target;
                        toolbar.Items = toolbar.Items.Replace(doneButton, newDoneButton);
                        toolbar.SetNeedsDisplay();
                    }
                    else
                    {
                        toolbar.Items = toolbar.Items.Append(newDoneButton).ToArray();
                        toolbar.SetNeedsDisplay();
                    }
                }
            }
        }

        public static UIBarButtonItem CreateDoneButton(BindableObject element, EventHandler eventHandler)
        {
            if (DialogExtensions.GetDoneButtonText(element) is string doneButtonText)
            {
                return new UIBarButtonItem(doneButtonText, UIBarButtonItemStyle.Done, eventHandler);
            }

            return new UIBarButtonItem(UIBarButtonSystemItem.Done, eventHandler);
        }
    }
}