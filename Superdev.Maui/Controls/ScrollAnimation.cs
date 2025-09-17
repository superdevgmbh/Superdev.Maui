using System.ComponentModel;
using Microsoft.Maui.Converters;

namespace Superdev.Maui.Controls
{
    public class ScrollAnimation
    {
        public static readonly ScrollAnimation Default = new ScrollAnimation();

        /// <summary>
        /// The time, in milliseconds, between frames.
        /// </summary>
        public uint Rate { get; set; } = 16U;

        /// <summary>
        /// The number of milliseconds over which to interpolate the animation.
        /// </summary>
        public uint Length { get; set; } = 250U;

        /// <summary>
        /// The easing function to use to transition in, out, or in and out of the animation.
        /// </summary>
        [TypeConverter(typeof(EasingTypeConverter))]
        public Easing Easing { get; set; } = Easing.Default;
    }
}