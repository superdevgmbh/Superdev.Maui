using System.Diagnostics;
using Android.Graphics;
using Android.Graphics.Drawables;
using Color = Android.Graphics.Color;
using Paint = Android.Graphics.Paint;

namespace Superdev.Maui.Platforms.Services
{
    internal class DecorViewBackgroundDrawable : Drawable
    {
        private readonly Paint paint = new Paint();
        private Color? statusBarColor;
        private Color? navigationBarColor;

        private int statusBarHeight;
        private int navigationBarHeight;

        public DecorViewBackgroundDrawable()
        {
        }

        internal void Draw(Color? statusBarColor, int statusBarHeight, Color? navigationBarColor, int navigationBarHeight)
        {
            this.statusBarColor = statusBarColor;
            this.statusBarHeight = statusBarHeight;
            this.navigationBarColor = navigationBarColor;
            this.navigationBarHeight = navigationBarHeight;

            this.InvalidateSelf();
        }

        public override void Draw(Canvas canvas)
        {
#if DEBUG
            Debug.WriteLine($"Draw: statusBarColor={(this.statusBarColor == null ? "null" : $"{this.statusBarColor}")}, statusBarHeight={this.statusBarHeight}, {Environment.NewLine}" +
                            $"navigationBarColor={(this.navigationBarColor == null ? "null" : $"{this.navigationBarColor}")}, navigationBarHeight={this.navigationBarHeight}");
#endif
            var bounds = this.Bounds;

            // Draw status bar
            if (this.statusBarColor is Color statusBarColor)
            {
                this.paint.Color = statusBarColor;
                canvas.DrawRect(bounds.Left, bounds.Top, bounds.Right, bounds.Top + this.statusBarHeight, this.paint);
            }

            // Draw middle section
            this.paint.Color = Color.Transparent;
            canvas.DrawRect(bounds.Left, bounds.Top + this.statusBarHeight, bounds.Right, bounds.Bottom - this.navigationBarHeight,
                this.paint);

            // Draw navigation bar
            if (this.navigationBarColor is Color navigationBarColor)
            {
                this.paint.Color = navigationBarColor;
                canvas.DrawRect(bounds.Left, bounds.Bottom - this.navigationBarHeight, bounds.Right, bounds.Bottom, this.paint);
            }
        }

        public override void SetAlpha(int alpha)
        {
            this.paint.Alpha = alpha;
        }

        public override void SetColorFilter(ColorFilter? colorFilter)
        {
            this.paint.SetColorFilter(colorFilter);
        }

        public override int Opacity => (int)Format.Opaque;
    }
}