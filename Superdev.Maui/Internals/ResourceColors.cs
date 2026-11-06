using Superdev.Maui.Resources.Styles;

namespace Superdev.Maui.Internals
{
    /// <summary>
    /// Theme colours looked up in the application's resources without failing: a missing key, or no application at
    /// all as in a unit test or the previewer, yields the fallback.
    /// </summary>
    internal static class ResourceColors
    {
        /// <summary>The theme's error colour, used for the underline of an invalid field.</summary>
        public static Color? Error(Color? fallback = null)
        {
            return Get(ThemeConstants.Color.Error, fallback);
        }

        public static Color? Get(string key, Color? fallback = null)
        {
            var resources = Application.Current?.Resources;
            return resources != null && resources.TryGetValue(key, out var value) && value is Color color ? color : fallback;
        }
    }
}
