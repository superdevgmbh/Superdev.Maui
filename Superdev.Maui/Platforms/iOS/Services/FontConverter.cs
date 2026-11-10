using Foundation;
using Superdev.Maui.Services;
using UIKit;

namespace Superdev.Maui.Platforms.Services
{
    /// <summary>
    /// Leaves font sizes unscaled, like the Android converter: MAUI already applies the iOS text size (Dynamic Type)
    /// to every control whose <c>FontAutoScalingEnabled</c> is on, which is the default, so scaling the theme's sizes
    /// here as well would apply it twice. <see cref="IFontConverter.FontScalingChanged"/> is still raised when the
    /// user changes the text size.
    /// </summary>
    public class FontConverter : FontConverterBase, IDisposable
    {
        private static readonly Lazy<IFontConverter> Implementation = new Lazy<IFontConverter>(CreateInstance, LazyThreadSafetyMode.PublicationOnly);

        public static IFontConverter Current => Implementation.Value;

        private static IFontConverter CreateInstance()
        {
            return new FontConverter();
        }

        private NSObject? contentSizeCategoryObserver;

        private FontConverter()
        {
            this.contentSizeCategoryObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                UIApplication.ContentSizeCategoryChangedNotification,
                _ => this.RaiseFontScalingChangedEvent());
        }

        public override void Dispose()
        {
            if (this.contentSizeCategoryObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(this.contentSizeCategoryObserver);
                this.contentSizeCategoryObserver = null;
            }

            base.Dispose();
        }
    }
}
