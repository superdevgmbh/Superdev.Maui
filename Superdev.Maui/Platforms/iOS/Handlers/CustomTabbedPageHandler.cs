using CoreGraphics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;
using UIKit;

namespace Superdev.Maui.Platforms.iOS.Handlers
{
    public class CustomTabbedPageHandler : TabbedViewHandler
    {
        private CustomTabbedPage customTabbedPage;
        private ILogger logger;

        public static IPropertyMapper<CustomTabbedPage, CustomTabbedPageHandler> CustomMapper = new PropertyMapper<CustomTabbedPage, CustomTabbedPageHandler>(Mapper)
        {
            [nameof(CustomTabbedPage.HideTabs)] = MapIsHidden,
        };
        private CGRect originalTabBarFrame;

        private static void MapIsHidden(CustomTabbedPageHandler customTabbedPageHandler, CustomTabbedPage customTabbedPage)
        {
            customTabbedPageHandler.UpdateBottomNavigationVisibility(customTabbedPage);
        }

        public CustomTabbedPageHandler() : base(CustomMapper)
        {
            this.logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<CustomTabbedPageHandler>>();
        }

        protected override void ConnectHandler(UIView platformView)
        {
            if (this.VirtualView is CustomTabbedPage customTabbedPage)
            {
                this.customTabbedPage = customTabbedPage;
                this.customTabbedPage.Loaded += this.TabbedPage_Loaded;
            }

            base.ConnectHandler(platformView);
        }

        private void TabbedPage_Loaded(object sender, EventArgs e)
        {
            this.UpdateBottomNavigationVisibility(this.customTabbedPage);
        }

        private void UpdateBottomNavigationVisibility(CustomTabbedPage customTabbedPage)
        {
            try
            {
                var frame = this.PlatformView.Frame;
                var tabBarFrame = this.ViewController.TabBarController.TabBar.Frame;
                if (tabBarFrame.Height > 0)
                {
                    this.originalTabBarFrame = tabBarFrame;
                }

                if (customTabbedPage.HideTabs)
                {
                    this.ViewController.TabBarController.TabBar.Hidden = true;
                    this.ViewController.TabBarController.TabBar.Frame = new CGRect(0, 0, 0, 0);
                    customTabbedPage.ContainerArea = new Rect(0, 0, frame.Width, frame.Height);
                }
                else
                {
                    this.ViewController.TabBarController.TabBar.Hidden = false;
                    this.ViewController.TabBarController.TabBar.Frame = this.originalTabBarFrame;
                    customTabbedPage.ContainerArea = new Rect(0, 0, frame.Width, frame.Height - this.originalTabBarFrame.Height);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "UpdateBottomNavigationVisibility failed with exception");
            }
        }

        protected override void DisconnectHandler(UIView platformView)
        {
            if (this.customTabbedPage != null)
            {
                this.customTabbedPage.Loaded -= this.TabbedPage_Loaded;
                this.customTabbedPage = null;
            }

            this.logger = null;

            base.DisconnectHandler(platformView);
        }
    }
}
