using Microsoft.Maui.Platform;
using Social;
using UIKit;

namespace Superdev.Maui.Resources.Styles
{
    public partial class ThemeHelper
    {
        private void ApplyThemePlatformResources(ITheme theme)
        {
            var primaryColor = theme.ColorConfiguration.Primary.ToPlatform();
            var textColor = theme.ColorConfiguration.TextColor.ToPlatform();

            // Text input
            UITextField.Appearance.TintColor = textColor;
            UITextView.Appearance.TintColor = textColor;

            // UIView
            UIView.AppearanceWhenContainedIn(typeof(UIAlertController)).TintColor = primaryColor;
            UIView.AppearanceWhenContainedIn(typeof(UIActivityViewController)).TintColor = primaryColor;
            UIView.AppearanceWhenContainedIn(typeof(SLComposeViewController)).TintColor = primaryColor;

            // UIToolbar
            UIBarButtonItem.AppearanceWhenContainedIn(typeof(UIToolbar)).TintColor = primaryColor;

            // TODO: Find more relevant appearance attributes here:
            // https://github.com/nachocove/NachoClientX/blob/86cfa30a9b411c01941a61fd879c0bd6ce669a69/NachoClient.iOS/NachoUI.iOS/Support/Theme.cs
        }
    }
}