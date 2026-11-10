using Android.Views;

namespace Superdev.Maui.Platforms.Handlers
{
    internal static class GravityExtensions
    {
        /// <summary>
        /// The horizontal gravity of a text alignment, the way MAUI maps it for its own text controls
        /// (its extension is internal).
        /// </summary>
        internal static GravityFlags ToHorizontalGravityFlags(this Microsoft.Maui.TextAlignment alignment)
        {
            return alignment switch
            {
                Microsoft.Maui.TextAlignment.Center => GravityFlags.CenterHorizontal,
                Microsoft.Maui.TextAlignment.End => GravityFlags.End,
                _ => GravityFlags.Start,
            };
        }
    }
}
