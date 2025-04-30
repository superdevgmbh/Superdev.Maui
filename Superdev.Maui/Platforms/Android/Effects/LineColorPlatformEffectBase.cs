using System.ComponentModel;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using Superdev.Maui.Effects;
using BlendMode = Android.Graphics.BlendMode;
using Debug = System.Diagnostics.Debug;

namespace Superdev.Maui.Platforms.Effects
{
    public class LineColorPlatformEffectBase : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                this.UpdateLineColor();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        protected override void OnDetached()
        {
            // Nothing to do here
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == LineColorEffect.LineColorProperty.PropertyName)
            {
                this.UpdateLineColor();
            }
        }

        private void UpdateLineColor()
        {
            try
            {
                if (this.Control is EditText editText)
                {
                    var lineColor = LineColorEffect.GetLineColor(this.Element);

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Q) // API 29+
                    {
                        editText.Background.SetColorFilter(new BlendModeColorFilter(lineColor.ToAndroid(), BlendMode.SrcAtop));
                    }
                    else
                    {
                        editText.Background.SetColorFilter(lineColor.ToAndroid(), PorterDuff.Mode.SrcAtop);
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}