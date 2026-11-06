using AndroidX.Core.View;
using Color = Android.Graphics.Color;
using Debug = System.Diagnostics.Debug;
using View = Android.Views.View;

namespace Superdev.Maui.Platforms.Services
{
    internal class WindowInsetsHandler : Java.Lang.Object, IOnApplyWindowInsetsListener, AView.IOnLayoutChangeListener
    {
        private readonly DecorViewBackgroundDrawable background;

        private Color? statusBarColor;
        private Color? navigationBarColor;
        private bool isAttached;

        public WindowInsetsHandler(AView rootView)
        {
            this.RootView = rootView;
            this.background = new DecorViewBackgroundDrawable();
        }

        public AView RootView { get; set; }

        public void Update(Color? statusBarColor, Color? navigationBarColor)
        {
            this.statusBarColor = statusBarColor;
            this.navigationBarColor = navigationBarColor;
        }

        public WindowInsetsCompat OnApplyWindowInsets(View _, WindowInsetsCompat windowInsets)
        {
            var systemBarInsets = windowInsets?.GetInsets(WindowInsetsCompat.Type.SystemBars());

            if (systemBarInsets == null)
            {
                return windowInsets;
            }

            var statusBarHeight = this.statusBarColor != null ? systemBarInsets.Top : 0;
            var navigationBarHeight = this.navigationBarColor != null ? systemBarInsets.Bottom : 0;

            Debug.WriteLine($"OnApplyWindowInsets: statusBarHeight={statusBarHeight}, navigationBarHeight={navigationBarHeight}");

            this.AttachIfNotAttached();
            this.UpdateBounds();

            this.background.Draw(
                this.statusBarColor, statusBarHeight,
                this.navigationBarColor, navigationBarHeight);

            // The insets are deliberately passed through unmodified. MAUI offsets its own chrome
            // (the AppBarLayout grows by the status bar inset) from the window insets, so consuming
            // them here - or applying them as padding on the root view - would offset the content twice.
            return windowInsets;
        }

        public void Detach()
        {
            this.RootView.RemoveOnLayoutChangeListener(this);
            this.RootView.Overlay?.Remove(this.background);
            this.isAttached = false;
        }

        public void OnLayoutChange(AView? view, int left, int top, int right, int bottom,
            int oldLeft, int oldTop, int oldRight, int oldBottom)
        {
            this.UpdateBounds();
        }

        /// <summary>
        /// The system bar backgrounds are drawn into the root view's overlay, which is rendered on top of
        /// all children. Drawing them as the root view's background would place them behind MAUI's
        /// AppBarLayout, whose own background already fills the status bar area when running edge-to-edge.
        /// </summary>
        private void AttachIfNotAttached()
        {
            if (this.isAttached)
            {
                return;
            }

            this.RootView.Overlay?.Add(this.background);
            this.RootView.AddOnLayoutChangeListener(this);
            this.isAttached = true;
        }

        private void UpdateBounds()
        {
            var width = this.RootView.Width;
            var height = this.RootView.Height;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            var bounds = this.background.Bounds;
            if (bounds.Right == width && bounds.Bottom == height)
            {
                return;
            }

            // A drawable in an overlay is invalidated against its bounds, so they must be kept in sync
            // with the root view - an empty bounds rectangle would never be redrawn.
            this.background.SetBounds(0, 0, width, height);
            this.background.InvalidateSelf();
        }
    }
}
