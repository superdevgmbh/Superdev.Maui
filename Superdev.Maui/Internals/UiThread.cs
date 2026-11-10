using Microsoft.Extensions.DependencyInjection;

namespace Superdev.Maui.Internals
{
    /// <summary>
    /// Runs a view update on the UI thread. A binding does this by itself; code which reacts to a view model's event
    /// directly has to. An update raised on the UI thread runs at once, and so does one for an element without a
    /// handler — it has no native view to touch, as in a unit test.
    /// </summary>
    internal static class UiThread
    {
        public static void Run(Element element, Action update)
        {
            ArgumentNullException.ThrowIfNull(element);
            ArgumentNullException.ThrowIfNull(update);

            if (Dispatcher.GetForCurrentThread() is null
                && element.Handler?.MauiContext?.Services.GetService<IDispatcher>() is { } dispatcher)
            {
                dispatcher.Dispatch(update);
                return;
            }

            update();
        }
    }
}
