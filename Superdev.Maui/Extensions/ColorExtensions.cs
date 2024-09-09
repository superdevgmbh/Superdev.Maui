namespace Superdev.Maui.Extensions
{
    public static class ColorExtensions
    {
        public static Color Complement(this Color color)
        {
            var hue = (color.GetHue() * 359.0f);
            var newHue = ((hue + 180f) % 359.0f);
            var complement = color.WithHue(newHue / 359.0f);

            return complement;
        }

        /// <summary>
        /// Inverts the R-G-B parts of <paramref name="color"/> with <paramref name="alpha"/>.
        /// </summary>
        /// <param name="color">The input color.</param>
        /// <param name="alpha">The alpha value of the returned color. If null, the alpha value of <paramref name="color"/> is used.</param>
        /// <returns>An inverted color.</returns>
        public static Color Invert(this Color color, double? alpha)
        {
            var r = 255 - (int)(255 * color.Red);
            var g = 255 - (int)(255 * color.Green);
            var b = 255 - (int)(255 * color.Blue);
            var a = alpha ?? color.Alpha;

            return Color.FromRgba(r, g, b, a);
        }

        /// <summary>
        /// Inverts the R-G-B parts of <paramref name="color"/>.
        /// </summary>
        /// <param name="color">The input color.</param>
        /// <returns>An inverted color.</returns>
        public static Color Invert(this Color color)
        {
            var r = 255 - (int)(255 * color.Red);
            var g = 255 - (int)(255 * color.Green);
            var b = 255 - (int)(255 * color.Blue);

            return Color.FromRgb(r, g, b);
        }
    }
}
