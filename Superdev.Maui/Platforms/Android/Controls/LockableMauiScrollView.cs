using Android.Content;
using Android.Util;
using Android.Views;
using Microsoft.Maui.Platform;

namespace Superdev.Maui.Platforms.Android.Controls
{
    public class LockableMauiScrollView : MauiScrollView
    {
        public LockableMauiScrollView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public bool ScrollEnabled { get; set; }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            if (this.ScrollEnabled)
            {
                return base.OnInterceptTouchEvent(ev);
            }
            else
            {
                return false;
            }
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            if (this.ScrollEnabled)
            {
                return base.OnTouchEvent(ev);
            }
            else
            {
                return false;
            }
        }
    }
}