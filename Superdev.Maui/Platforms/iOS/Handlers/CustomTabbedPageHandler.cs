using System.ComponentModel;
using System.Diagnostics;
using CoreGraphics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    public class CustomTabbedPageHandler : TabbedRenderer
    {
        private ILogger logger;

        static CustomTabbedPageHandler()
        {
            Mapper.AppendToMapping(nameof(CustomTabbedPage.HideTabs), MapIsHidden);
        }

        private CGRect originalTabBarFrame;

        private static void MapIsHidden(TabbedRenderer tabbedRenderer, TabbedPage tabbedPage)
        {
            if (tabbedRenderer is CustomTabbedPageHandler customTabbedPageHandler &&
                tabbedPage is CustomTabbedPage customTabbedPage)
            {
                customTabbedPageHandler.UpdateBottomNavigationVisibility(customTabbedPage);
            }
        }

        public CustomTabbedPageHandler()
        {
            this.logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<CustomTabbedPageHandler>>();
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                this.UpdateBottomNavigationVisibility((CustomTabbedPage)this.Element);
                this.Element.PropertyChanged += this.OnElementPropertyChanged;
            }

            if (e.OldElement != null)
            {
                this.Element.PropertyChanged -= this.OnElementPropertyChanged;
            }
        }

        private void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"OnElementPropertyChanged: {e.PropertyName}");

            if (e.PropertyName == nameof(CustomTabbedPage.HideTabs))
            {
                this.UpdateBottomNavigationVisibility((CustomTabbedPage)this.Element);
            }
            else if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName)
            {
                this.View?.SetNeedsLayout();
            }
            else if (e.PropertyName == VisualElement.WidthProperty.PropertyName ||
                     e.PropertyName == VisualElement.HeightProperty.PropertyName)
            {
                this.View?.SetNeedsLayout();
            }
        }

        private void UpdateBottomNavigationVisibility(CustomTabbedPage customTabbedPage)
        {
            if (this.View is not UIView view)
            {
                return;
            }

            var frame = view.Frame;

            var tabBarFrame = this.TabBar.Frame;
            if (tabBarFrame.Height > 0)
            {
                this.originalTabBarFrame = tabBarFrame;
            }

            if (customTabbedPage.HideTabs)
            {
                this.TabBar.Hidden = true;
                this.TabBar.Frame = new CGRect(0, 0, 0, 0);
                customTabbedPage.ContainerArea = new Rect(0, 0, frame.Width, frame.Height);
            }
            else
            {
                this.TabBar.Hidden = false;
                this.TabBar.Frame = this.originalTabBarFrame;
                customTabbedPage.ContainerArea = new Rect(0, 0, frame.Width, frame.Height - this.originalTabBarFrame.Height);
            }
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            this.UpdateBottomNavigationVisibility((CustomTabbedPage)this.Element);
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            this.UpdateBottomNavigationVisibility((CustomTabbedPage)this.Element);
        }
    }
}