using CoreGraphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using UIKit;

namespace Superdev.Maui.Platforms.iOS.Handlers
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
            [nameof(CustomEntry.Padding)] = MapControl
        };

        protected override MauiTextField CreatePlatformView()
        {
            var nativeEntry = new MauiTextField();
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

        protected override void ConnectHandler(MauiTextField platformView)
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

        protected override void DisconnectHandler(MauiTextField platformView)
        {
            // Perform any native view cleanup here
            platformView.Dispose();
            base.DisconnectHandler(platformView);
        }

        private static void MapControl(CustomEntryHandler handler, CustomEntry entry)
        {
            MapBorder(handler, entry);
            MapText(handler, entry);
            MapTextColor(handler, entry);

            handler.PlatformView.Enabled = entry.IsEnabled;
        }
        private static void MapBorder(CustomEntryHandler handler, CustomEntry view)
        {
            handler.PlatformView.LeftView = new UIView(new CGRect(0f, 0f, (float)view.Padding.Left, handler.PlatformView.Frame.Height));
            handler.PlatformView.LeftViewMode = UITextFieldViewMode.Always;
            handler.PlatformView.RightView = new UIView(new CGRect(0f, 0f, (float)view.Padding.Right, handler.PlatformView.Frame.Height));
            handler.PlatformView.RightViewMode = UITextFieldViewMode.Always;

            handler.PlatformView.KeyboardAppearance = UIKeyboardAppearance.Dark;
            handler.PlatformView.ReturnKeyType = UIReturnKeyType.Done;
            // Radius for the curves  
            handler.PlatformView.Layer.CornerRadius = Convert.ToSingle(view.CornerRadius);
            // Thickness of the Border Color  
            handler.PlatformView.Layer.BorderColor = view.BorderColor.ToCGColor();
            // Thickness of the Border Width  
            handler.PlatformView.Layer.BorderWidth = view.BorderWidth;
            handler.PlatformView.ClipsToBounds = true;
            handler.PlatformView.BackgroundColor = view.BackgroundColor.ToPlatform();
        }
    }
}