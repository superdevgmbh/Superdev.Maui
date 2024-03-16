using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;

namespace Superdev.Maui.Platforms.Android.Handlers
{
    public class CustomEntryHandler : EntryHandler
    {
        public CustomEntryHandler() : base(PropertyMapper)
        {

        }

        public static PropertyMapper<CustomEntry, CustomEntryHandler> PropertyMapper = new(ViewMapper)
        {
            [nameof(CustomEntry.Text)] = MapControl,
            [nameof(CustomEntry.TextColor)] = MapControl,
            [nameof(CustomEntry.BorderColor)] = MapControl,
            [nameof(CustomEntry.BorderWidth)] = MapControl,
            [nameof(CustomEntry.CornerRadius)] = MapControl,
            [nameof(CustomEntry.Padding)] = MapControl,
            [nameof(CustomEntry.IsEnabled)] = MapControl
        };

        protected override AppCompatEditText CreatePlatformView()
        {
            var nativeEntry = new AppCompatEditText(this.Context);
            var myentry = this.VirtualView as CustomEntry;

            //if (myentry.UnderlineThickness == 0)
            //{   // Hide Underline.
            //    nativeEntry.PaintFlags &= ~Android.Graphics.PaintFlags.UnderlineText;
            //    //nativeEntry.Background = null;
            //    //nativeEntry.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
            //}
            //else
            //{   // Show Underline. (Is only under the typed text, not the whole control.)
            //    nativeEntry.PaintFlags |= Android.Graphics.PaintFlags.UnderlineText;
            //    // TODO: Line thickness and color. For color, see https://stackoverflow.com/a/62486103/199364.
            //    // For thickness, probably need to "nest controls", similar to Windows implementation.
            //}

            return nativeEntry;
        }

        protected override void ConnectHandler(AppCompatEditText platformView)
        {
            base.ConnectHandler(platformView);

            // Perform any control setup here
            //if (platformView != null)
            //{
            //    platformView.
            //    var view = (CustomEntry)Element;
            //    if (view.IsCurvedCornersEnabled)
            //    {
            //        // creating gradient drawable for the curved background  
            //        var _gradientBackground = new GradientDrawable();
            //        _gradientBackground.SetShape(ShapeType.Rectangle);
            //        _gradientBackground.SetColor(view.BackgroundColor.ToAndroid());

            //        // Thickness of the stroke line  
            //        _gradientBackground.SetStroke(view.BorderWidth, view.BorderColor.ToAndroid());

            //        // Radius for the curves  
            //        _gradientBackground.SetCornerRadius(
            //            DpToPixels(this.Context, Convert.ToSingle(view.CornerRadius)));

            //        // set the background of the   
            //        Control.SetBackground(_gradientBackground);
            //    }
            //    // Set padding for the internal text from border  
            //    Control.SetPadding(
            //        (int)DpToPixels(this.Context, Convert.ToSingle(5)), Control.PaddingTop,
            //        (int)DpToPixels(this.Context, Convert.ToSingle(5)), Control.PaddingBottom);
            //}
        }

        protected override void DisconnectHandler(AppCompatEditText platformView)
        {
            // Perform any native view cleanup here
            platformView.Dispose();
            base.DisconnectHandler(platformView);
        }

        private static void MapText(CustomEntryHandler handler, CustomEntry view)
        {
            handler.PlatformView.Text = view.Text;
            handler.PlatformView?.SetSelection(handler.PlatformView?.Text?.Length ?? 0);
        }

        private static void MapTextColor(CustomEntryHandler handler, CustomEntry view)
        {
            handler.PlatformView?.SetTextColor(view.TextColor.ToPlatform());
        }

        private static void MapControl(CustomEntryHandler handler, CustomEntry entry)
        {
            MapBorder(handler, entry);
            MapPadding(handler, entry);
            MapText(handler, entry);
            MapTextColor(handler, entry);

            handler.PlatformView.Enabled = entry.IsEnabled;
        }

        private static void MapPadding(CustomEntryHandler handler, CustomEntry entry)
        {
            var padLeft = DpToPixels(handler.Context, (float)entry.Padding.Left);
            var padTop = DpToPixels(handler.Context, (float)entry.Padding.Top);
            var padRight = DpToPixels(handler.Context, (float)entry.Padding.Right);
            var padBottom = DpToPixels(handler.Context, (float)entry.Padding.Bottom);

            handler.PlatformView?.SetPadding((int)padLeft, (int)padTop, (int)padRight, (int)padBottom);

            MapBorder(handler, entry);
        }

        private static void MapBorder(CustomEntryHandler handler, CustomEntry view)
        {
            var _gradientBackground = new GradientDrawable();
            _gradientBackground.SetShape(ShapeType.Rectangle);
            _gradientBackground.SetColor(view.BackgroundColor.ToPlatform());

            // Thickness of the stroke line  
            _gradientBackground.SetStroke(view.BorderWidth, view.BorderColor.ToPlatform());

            // Radius for the curves  
            _gradientBackground.SetCornerRadius(
                DpToPixels(handler.PlatformView.Context!, Convert.ToSingle(view.CornerRadius)));

            handler.PlatformView.Background = _gradientBackground;
        }

        private static float DpToPixels(Context context, float valueInDp)
        {
            if (context is null)
            {
                return valueInDp;
            }

            var metrics = context.Resources?.DisplayMetrics!;
            return TypedValue.ApplyDimension(ComplexUnitType.Dip, valueInDp, metrics);
        }
    }
}