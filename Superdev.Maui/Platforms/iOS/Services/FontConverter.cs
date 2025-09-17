using System.Diagnostics;
using Foundation;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Services;
using UIKit;

namespace Superdev.Maui.Platforms.Services
{
    public class FontConverter : FontConverterBase, IDisposable
    {
        private static readonly Lazy<IFontConverter> Implementation = new Lazy<IFontConverter>(CreateInstance, LazyThreadSafetyMode.PublicationOnly);

        public static IFontConverter Current => Implementation.Value;

        private static IFontConverter CreateInstance()
        {
            return new FontConverter();
        }

        private readonly ILogger logger;
        private static readonly NSString UiContentSizeCategoryDidChangeNotificationKey = (NSString)"UIContentSizeCategoryDidChangeNotification";

        private FontConverter() : base()
        {
            this.logger = IPlatformApplication.Current.Services.GetService<ILogger<FontConverter>>();
            NSNotificationCenter.DefaultCenter.AddObserver(UiContentSizeCategoryDidChangeNotificationKey, (n) =>
            {
                this.RaiseFontScalingChangedEvent();
            });
        }

        public override double GetScaledFontSize(double fontSize)
        {
            var scaledFontSize = UIFontMetrics.DefaultMetrics.GetScaledValue((nfloat)fontSize);
#if DEBUG
            this.logger.LogDebug($"GetScaledFontSize: {fontSize} -> {scaledFontSize} (scale factor: {(scaledFontSize / fontSize):F2}x)");
#endif
            return scaledFontSize;
        }

        public override void Dispose()
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(UiContentSizeCategoryDidChangeNotificationKey);
            base.Dispose();
        }
    }
}