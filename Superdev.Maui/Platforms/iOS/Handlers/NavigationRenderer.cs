using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;
using Superdev.Maui.Navigation.Hosts;
using Superdev.Maui.Utils;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    public class NavigationRenderer : Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer
    {
        private static readonly IntPtr ShouldPopItemSelector = Selector.GetHandle("navigationBar:shouldPopItem:");

        /// <summary>
        /// MAUI's page view controller exposes its page through an internal property. The lookup is cached
        /// per view controller type, because it runs on every layout pass, including push and pop animations.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, PropertyInfo?> ChildPropertyCache = new ConcurrentDictionary<Type, PropertyInfo?>();

        static NavigationRenderer()
        {
            Mapper.AppendToMapping(NavigationPage.BackButtonTitleProperty.PropertyName, MapBackButtonTitle);
        }

        private static void MapBackButtonTitle(Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer navigationRenderer, NavigationPage navigationPage)
        {
            if (navigationRenderer is NavigationRenderer navigationPageHandler)
            {
                navigationPageHandler.UpdateBackButtonTitle(navigationPage);
            }
        }

        private void UpdateBackButtonTitle(NavigationPage navigationPage)
        {
            var backButtonTitle = NavigationPage.GetBackButtonTitle(navigationPage);

            foreach (var item in this.NavigationBar.Items)
            {
                item.BackBarButtonItem = new UIBarButtonItem(backButtonTitle, UIBarButtonItemStyle.Plain, null);
            }
        }

        /// <summary>
        /// Routes the back button of the navigation bar through the navigation service, so that the
        /// navigation guards of the page are evaluated before it is popped.
        /// </summary>
        /// <remarks>
        /// MAUI's renderer asks the current page whether it handles the back button and pops the page
        /// otherwise; the navigation page is never asked, so a <see cref="Controls.CustomNavigationPage"/>
        /// cannot intercept the button on iOS the way it intercepts the hardware back button on Android.
        /// The selector is re-exported here, which is how a subclass takes over a member the base class
        /// declares internal, and the base implementation is reached through the Objective-C super call
        /// when the navigation service does not take over.
        /// </remarks>
        [Export("navigationBar:shouldPopItem:")]
        [Preserve(Conditional = true)]
        public bool ShouldPopItemThroughNavigationService(UINavigationBar navigationBar, UINavigationItem item)
        {
            var navigationPage = (this as IElementHandler).VirtualView as NavigationPage;
            var currentPage = navigationPage?.CurrentPage;
            var navigationHost = IPlatformApplication.Current?.Services.GetService<INavigationHost>();

            if (currentPage != null && navigationHost != null && navigationHost.RequestBack(currentPage))
            {
                // The navigation service pops through its own pipeline once the guards agreed; UIKit must not.
                return false;
            }

            return bool_objc_msgSendSuper_IntPtr_IntPtr(this.SuperHandle, ShouldPopItemSelector, navigationBar.Handle, item.Handle);
        }

        [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSendSuper")]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool bool_objc_msgSendSuper_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            var navigationController = base.TopViewController.NavigationController;
            if (navigationController is { InteractivePopGestureRecognizer: UIGestureRecognizer interactivePopGestureRecognizer })
            {
                if (interactivePopGestureRecognizer.Delegate is not InteractivePopGestureRecognizerDelegate)
                {
                    interactivePopGestureRecognizer.Delegate = new InteractivePopGestureRecognizerDelegate(navigationController);
                }

                var childPage = GetChildPage(this.TopViewController);
                if (childPage != null)
                {
                    var swipeBackEnabled = Superdev.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.GetSwipeBackEnabled(childPage);

                    Debug.WriteLine($"ViewDidLayoutSubviews: InteractivePopGestureRecognizer: swipeBackEnabled={swipeBackEnabled}");
                    interactivePopGestureRecognizer.Enabled = swipeBackEnabled;
                }
            }
        }

        private static Page? GetChildPage(UIViewController viewController)
        {
            var childProperty = ChildPropertyCache.GetOrAdd(viewController.GetType(), t => ReflectionHelper.GetPropertyInfo(t, "Child"));
            return childProperty?.GetValue(viewController) as Page;
        }

        private class InteractivePopGestureRecognizerDelegate : UIGestureRecognizerDelegate
        {
            private readonly UINavigationController navigationController;

            public InteractivePopGestureRecognizerDelegate(UINavigationController navigationController)
            {
                this.navigationController = navigationController;
            }

            public override bool ShouldBegin(UIGestureRecognizer recognizer)
            {
                var canSwipeBack = (this.navigationController?.ViewControllers?.Length ?? 0) > 1;
                return canSwipeBack;
            }
        }
    }
}