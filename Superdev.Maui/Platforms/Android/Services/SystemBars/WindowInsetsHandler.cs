using AndroidX.Core.View;
using Color = Android.Graphics.Color;
using Debug = System.Diagnostics.Debug;
using View = Android.Views.View;

namespace Superdev.Maui.Platforms.Services
{
    internal class WindowInsetsHandler : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        private readonly DecorViewBackgroundDrawable background;

        private Color? statusBarColor;
        private Color? navigationBarColor;

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
            var statusBarInsets = windowInsets?.GetInsets(WindowInsetsCompat.Type.SystemBars());

            if (statusBarInsets == null)
            {
                return windowInsets;
            }

            var statusBarHeight = this.statusBarColor != null ? statusBarInsets.Top : 0;
            var navigationBarHeight = this.navigationBarColor != null ? statusBarInsets.Bottom : 0;

            Debug.WriteLine($"OnApplyWindowInsets: statusBarHeight={statusBarHeight}, navigationBarHeight={navigationBarHeight}");

            if (this.RootView.Background is not DecorViewBackgroundDrawable)
            {
                this.RootView.Background = this.background;
            }

            this.background.Draw(
                this.statusBarColor, statusBarHeight,
                this.navigationBarColor, navigationBarHeight);

            this.RootView.SetPadding(this.RootView.PaddingLeft, statusBarHeight, this.RootView.PaddingRight, navigationBarHeight);

            // Return Consumed if you don't want the window insets to keep passing
            // down to descendant views.
            // return WindowInsetsCompat.Consumed;
            return windowInsets;
        }
    }
}