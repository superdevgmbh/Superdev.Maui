using System.Diagnostics;
using Superdev.Maui.Utils;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    public class NavigationRenderer : Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer
    {
        static NavigationRenderer()
        {
            Mapper.AppendToMapping(NavigationPage.BackButtonTitleProperty.PropertyName, UpdateBackButtonTitle);
        }

        private static void UpdateBackButtonTitle(Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer navigationRenderer, NavigationPage navigationPage)
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

                var current = ReflectionHelper.GetPropertyValue<Page>(this.TopViewController, "Child");
                var swipeBackEnabled = Superdev.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.GetSwipeBackEnabled(current);

                Debug.WriteLine($"ViewDidLayoutSubviews: InteractivePopGestureRecognizer: swipeBackEnabled={swipeBackEnabled}");
                interactivePopGestureRecognizer.Enabled = swipeBackEnabled;
            }
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