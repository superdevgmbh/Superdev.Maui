using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Superdev.Maui.Controls
{
    /// <summary>
    /// Logging for the ScrollTo attached properties, which live in static classes and therefore
    /// resolve their logger from the application rather than from a constructor.
    /// </summary>
    internal static class ScrollToLogger
    {
        internal static void LogScrollToFailed(Type extensionType, BindableObject target, Exception exception)
        {
            var logger = IPlatformApplication.Current?.Services.GetService<ILoggerFactory>()?.CreateLogger(extensionType.FullName!);
            if (logger != null)
            {
                logger.LogWarning(exception, "ScrollTo failed on {TargetType}", target.GetType().Name);
            }
            else
            {
                Debug.WriteLine($"{extensionType.Name}: ScrollTo failed on {target.GetType().Name}: {exception}");
            }
        }
    }
}
