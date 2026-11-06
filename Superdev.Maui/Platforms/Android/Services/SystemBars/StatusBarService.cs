using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Android.OS;
using Android.Util;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Platform;
using Superdev.Maui.Services;
using Activity = Android.App.Activity;
using Color = Microsoft.Maui.Graphics.Color;

namespace Superdev.Maui.Platforms.Services
{
    public class StatusBarService : IStatusBarService
    {
        private static readonly Lazy<IStatusBarService> Implementation = new Lazy<IStatusBarService>(CreateStatusBar, LazyThreadSafetyMode.PublicationOnly);

        public static IStatusBarService Current => Implementation.Value;

        private static IStatusBarService CreateStatusBar()
        {
            return new StatusBarService();
        }

        private WindowInsetsHandler insetsListener;
        private AColor? statusBarColor;
        private AColor? navigationBarColor;
        private StatusBarStyle? statusBarStyle;

        private StatusBarService()
        {
        }

        public void OnResume()
        {
            if (this.statusBarColor is AColor statusBarColor)
            {
                this.SetStatusBarColorInternal(statusBarColor);
            }

            if (this.navigationBarColor is AColor navigationBarColor)
            {
                this.SetNavigationBarColorInternal(navigationBarColor);
            }

            if (this.statusBarStyle is StatusBarStyle statusBarStyle)
            {
                this.SetStyle(statusBarStyle);
            }
        }

        public void SetStatusBarColor(Color? color)
        {
            this.statusBarColor = color?.ToPlatform();
            this.SetStatusBarColorInternal(this.statusBarColor);
        }

        public void SetStatusBarColorInternal(AColor? color)
        {
            if (!TryGetActivityAndDecorView(out var activity, out _, out var rootView))
            {
                return;
            }

            if (IsEdgeToEdgeEnforced(activity))
            {
                if (color == null && this.navigationBarColor == null)
                {
                    this.ResetInsetsListener(rootView, activity);
                }
                else
                {
                    this.CreateInsetsListenerIfNotExists(rootView);
                    this.insetsListener.Update(color, this.navigationBarColor);

                    WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);

                    var insetsController = WindowCompat.GetInsetsController(activity.Window, rootView);
                    insetsController.Show(WindowInsetsCompat.Type.StatusBars());

                    ViewCompat.RequestApplyInsets(rootView);
                }
            }
            else
            {
                activity.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);

                var statusBarColor = this.statusBarColor ?? Colors.Transparent.ToPlatform();
                activity.Window.SetStatusBarColor(statusBarColor);
            }
        }

        public void SetNavigationBarColor(Color? color)
        {
            this.navigationBarColor = color?.ToPlatform();
            this.SetNavigationBarColorInternal(this.navigationBarColor);
        }

        public void SetNavigationBarColorInternal(AColor? color)
        {
            if (!TryGetActivityAndDecorView(out var activity, out _, out var rootView))
            {
                return;
            }

            if (IsEdgeToEdgeEnforced(activity))
            {
                if (color == null && this.statusBarColor == null)
                {
                    this.ResetInsetsListener(rootView, activity);
                }
                else
                {
                    this.CreateInsetsListenerIfNotExists(rootView);
                    this.insetsListener.Update(this.statusBarColor, color);

                    WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);

                    ViewCompat.RequestApplyInsets(rootView);
                }
            }
            else
            {
                if (this.navigationBarColor is not AColor navigationBarColor)
                {
                    navigationBarColor = AColor.Transparent;
                }

                // TODO: Set translucent navigation bar if color is transparent
                // https://github.com/CommunityToolkit/Maui/blob/51e122822c602b97793dbbdc2578629729b2c968/src/CommunityToolkit.Maui.Core/Services/DialogFragmentService.android.cs#L128
                // https://stackoverflow.com/questions/29069070/completely-transparent-status-bar-and-navigation-bar-on-lollipop

                activity.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                activity.Window.ClearFlags(WindowManagerFlags.TranslucentNavigation);

                activity.Window.SetNavigationBarColor(navigationBarColor);
            }
        }

        private static bool TryGetActivityAndDecorView([NotNullWhen(true)] out Activity activity, [NotNullWhen(true)] out AView decorView, [NotNullWhen(true)] out AView rootView)
        {
            activity = Platform.CurrentActivity;
            decorView = activity?.Window?.DecorView;
            rootView = activity?.FindViewById(global::Android.Resource.Id.Content);

            return activity != null && decorView != null && rootView != null;
        }

        /// <summary>
        /// Android 15 (API 35) forces edge-to-edge on apps which target API 35 or higher. Apps can opt out
        /// via <c>android:windowOptOutEdgeToEdgeEnforcement</c> - which is exactly what MAUI's default theme does.
        /// While opted out, the system still reserves the status bar area and the legacy window APIs keep working,
        /// so the inset-based drawing below must not be used: MAUI would offset the toolbar a second time.
        /// </summary>
        private static bool IsEdgeToEdgeEnforced(Activity activity)
        {
            if ((int)Build.VERSION.SdkInt < 35 || (int)activity.ApplicationInfo.TargetSdkVersion < 35)
            {
                return false;
            }

            var typedValue = new TypedValue();
            if (activity.Theme is global::Android.Content.Res.Resources.Theme theme &&
                theme.ResolveAttribute(global::Android.Resource.Attribute.WindowOptOutEdgeToEdgeEnforcement, typedValue, true))
            {
                return typedValue.Data == 0;
            }

            return true;
        }

        private void CreateInsetsListenerIfNotExists(AView rootView)
        {
            if (this.insetsListener == null)
            {
                this.insetsListener = new WindowInsetsHandler(rootView);
                ViewCompat.SetOnApplyWindowInsetsListener(rootView, this.insetsListener);
            }
            else
            {
                if (!Equals(this.insetsListener.RootView, rootView))
                {
                    this.insetsListener.Detach();
                    this.insetsListener.RootView = rootView;
                    ViewCompat.SetOnApplyWindowInsetsListener(rootView, null);
                    ViewCompat.SetOnApplyWindowInsetsListener(rootView, this.insetsListener);
                }
            }
        }

        private void ResetInsetsListener(AView rootView, Activity activity)
        {
            this.insetsListener?.Detach();

            ViewCompat.SetOnApplyWindowInsetsListener(rootView, null);
            WindowCompat.SetDecorFitsSystemWindows(activity.Window, true);
            ViewCompat.RequestApplyInsets(rootView);

            this.insetsListener = null;
        }

        public void SetStyle(StatusBarStyle statusBarStyle)
        {
            this.statusBarStyle = statusBarStyle;

            if (!TryGetActivityAndDecorView(out var activity, out var decorView, out _))
            {
                return;
            }

            // The compat controller uses the window insets controller from API 30 on - the only path which
            // works on edge-to-edge windows - and the (deprecated) system UI visibility flags below it.
            var insetsController = WindowCompat.GetInsetsController(activity.Window, decorView);
            insetsController.AppearanceLightStatusBars = statusBarStyle == StatusBarStyle.Light;
        }
    }
}