using Microsoft.Maui.Controls.Handlers.Compatibility;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers.MauiFix
{
    public class NavigationPageHandler : NavigationRenderer
    {
        static NavigationPageHandler()
        {
            Mapper.AppendToMapping(NavigationPage.BackButtonTitleProperty.PropertyName, UpdateBackButtonTitle);
        }

        private static void UpdateBackButtonTitle(NavigationRenderer navigationRenderer, NavigationPage navigationPage)
        {
            if (navigationRenderer is NavigationPageHandler navigationPageHandler)
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
    }
}