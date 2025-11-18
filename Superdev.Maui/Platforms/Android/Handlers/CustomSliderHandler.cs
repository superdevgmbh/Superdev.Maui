using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Widget;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Utils;
using BlendMode = Android.Graphics.BlendMode;
using Color = Android.Graphics.Color;
using OvalShape = Android.Graphics.Drawables.Shapes.OvalShape;
using ShapeDrawable = Android.Graphics.Drawables.ShapeDrawable;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<CustomSlider, CustomSliderHandler>;

    public class CustomSliderHandler : SliderHandler
    {
        private int? originalIntrinsicHeight;
        private int? originalIntrinsicWidth;

        public new static readonly PM Mapper = new PM(SliderHandler.Mapper)
        {
            [nameof(CustomSlider.ThumbSize)] = MapThumbSize
        };

        public CustomSliderHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public CustomSliderHandler()
            : base(Mapper)
        {
        }

        protected override void ConnectHandler(SeekBar platformView)
        {
#if !NET9_0_OR_GREATER
            this.VirtualView.AddCleanUpEvent();
#endif
            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(SeekBar platformView)
        {
            this.originalIntrinsicHeight = null;
            this.originalIntrinsicWidth = null;

            base.DisconnectHandler(platformView);
        }

        private static void MapThumbSize(ISliderHandler sliderHandler, ISlider slider)
        {
            if (sliderHandler is CustomSliderHandler customSliderHandler && slider is CustomSlider customSlider)
            {
                customSliderHandler.UpdateThumbSize(customSlider);
            }
        }

        private void UpdateThumbSize(CustomSlider customSlider)
        {
            var seekBar = this.PlatformView;

            if (customSlider.ThumbSize is int thumbSize)
            {
                if (seekBar.Thumb is Drawable thumb)
                {
                    this.originalIntrinsicHeight = thumb.IntrinsicHeight;
                    this.originalIntrinsicWidth = thumb.IntrinsicWidth;
                }

                var thumbSizePx = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, thumbSize, this.Context.Resources.DisplayMetrics);
                this.SetThumb(customSlider.ThumbColor.ToPlatform(), thumbSizePx, thumbSizePx);
            }
            else if (this.originalIntrinsicWidth is int intrinsicWidth && this.originalIntrinsicHeight is int intrinsicHeight)
            {
                this.SetThumb(customSlider.ThumbColor.ToPlatform(), intrinsicWidth, intrinsicHeight);
            }
        }

        private void SetThumb(Color color, int intrinsicWidth, int intrinsicHeight)
        {
            var drawable = new ShapeDrawable(new OvalShape());
            drawable.SetIntrinsicWidth(intrinsicWidth);
            drawable.SetIntrinsicHeight(intrinsicHeight);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                drawable.SetColorFilter(new BlendModeColorFilter(color, BlendMode.SrcOver));
            }
            else
            {
                drawable.SetColorFilter(color, PorterDuff.Mode.SrcOver);
            }

            var seekBar = this.PlatformView;
            seekBar.SetThumb(drawable);
        }
    }
}