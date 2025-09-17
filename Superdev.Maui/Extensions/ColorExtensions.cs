using Superdev.Maui.Services;

namespace Superdev.Maui.Extensions
{
    public static class ColorExtensions
    {
        public static Color Greyscale(this Color color)
        {
            return color.WithSaturation(0f);
        }

        /// <summary>
        /// Brightens the color by increasing its luminosity by a relative factor (percentage of current luminosity).
        /// </summary>
        /// <param name="color">Original color</param>
        /// <param name="factor">Percentage increase (0.1 = +10%)</param>
        public static Color BrightenRelative(this Color color, float factor)
        {
            var luminosity = color.GetLuminosity();
            var newLum = Math.Clamp(luminosity * (1f + factor), 0f, 1f);
            return color.WithLuminosity(newLum);
        }

        /// <summary>
        /// Darkens the color by decreasing its luminosity by a relative factor (percentage of current luminosity).
        /// </summary>
        /// <param name="color">Original color</param>
        /// <param name="factor">Percentage decrease (0.1 = -10%)</param>
        public static Color DarkenRelative(this Color color, float factor)
        {
            var luminosity = color.GetLuminosity();
            var newLum = Math.Clamp(luminosity * (1f - factor), 0f, 1f);
            return color.WithLuminosity(newLum);
        }

        /// <summary>
        ///     Brightens the color by increasing its luminosity.
        /// </summary>
        /// <param name="color">Original color</param>
        /// <param name="amount">Amount to brighten (0..1)</param>
        public static Color Brighten(this Color color, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);

            // Increase luminosity by amount, clamp to 1.0
            var luminosity = color.GetLuminosity();
            var newLum = Math.Clamp(luminosity + amount, 0f, 1f);
            return color.WithLuminosity(newLum);
        }

        /// <summary>
        ///     Darkens the color by decreasing its luminosity.
        /// </summary>
        /// <param name="color">Original color</param>
        /// <param name="amount">Amount to darken (0..1)</param>
        public static Color Darken(this Color color, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);

            // Decrease luminosity by amount, clamp to 0.0
            var luminosity = color.GetLuminosity();
            var newLum = Math.Clamp(luminosity - amount, 0f, 1f);
            return color.WithLuminosity(newLum);
        }

        public static Color Complement(this Color color)
        {
            var hue = color.GetHue() * 359.0f;
            var newHue = (hue + 180f) % 359.0f;
            var complement = color.WithHue(newHue / 359.0f);

            return complement;
        }

        /// <summary>
        ///     Inverts the R-G-B parts of <paramref name="color" /> with <paramref name="alpha" />.
        /// </summary>
        /// <param name="color">The input color.</param>
        /// <param name="alpha">The alpha value of the returned color. If null, the alpha value of <paramref name="color" /> is used.</param>
        /// <returns>An inverted color.</returns>
        public static Color Invert(this Color color, float? alpha)
        {
            var r = 255f - 255f * color.Red;
            var g = 255f - 255f * color.Green;
            var b = 255f - 255f * color.Blue;
            var a = alpha ?? color.Alpha;

            return new Color(r, g, b, a);
        }

        /// <summary>
        ///     Inverts the R-G-B parts of <paramref name="color" />.
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

        public static StatusBarStyle ToStatusBarStyle(this Color color)
        {
            if (color == null)
            {
                return StatusBarStyle.Default;
            }

            // Source: https://en.wikipedia.org/wiki/Luma_(video)
            var y = 0.2126 * color.Red + 0.7152 * color.Green + 0.0722 * color.Blue;

            return y < 128
                ? StatusBarStyle.Dark
                : StatusBarStyle.Light;
        }
    }
}