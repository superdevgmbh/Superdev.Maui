using Android.Content;
using Android.Util;
using Android.Views;
using Microsoft.Maui.Platform;

namespace Superdev.Maui.Platforms.Android.Controls
{
    /// <summary>
    /// A <see cref="MauiScrollView"/> whose scrolling by touch can be switched off with <see cref="ScrollEnabled"/>.
    /// </summary>
    public class LockableMauiScrollView : MauiScrollView
    {
        private static readonly HorizontalScrollLock HorizontalScrollLockListener = new HorizontalScrollLock();

        public LockableMauiScrollView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        /// <summary>
        /// Whether touch gestures scroll the view, in either orientation. Default: <c>true</c>.
        /// </summary>
        public bool ScrollEnabled { get; set; } = true;

        public override void OnViewAdded(AView? child)
        {
            base.OnViewAdded(child);

            // A horizontal scroll view scrolls in the HorizontalScrollView MAUI adds as its child. That view handles its
            // touches itself, so the overrides below do not reach it.
            if (child is global::Android.Widget.HorizontalScrollView horizontalScrollView)
            {
                horizontalScrollView.SetOnTouchListener(HorizontalScrollLockListener);
            }
        }

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

        /// <summary>
        /// Consumes the touches a horizontal scroll view would scroll with while its parent's scrolling is switched off.
        /// Touches a child of it claims still reach that child.
        /// </summary>
        private sealed class HorizontalScrollLock : Java.Lang.Object, AView.IOnTouchListener
        {
            public bool OnTouch(AView? view, MotionEvent? motionEvent)
            {
                return view?.Parent is LockableMauiScrollView { ScrollEnabled: false };
            }
        }
    }
}
