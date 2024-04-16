using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;
using AView = global::Android.Views.View;
using static Android.Views.ViewGroup;

namespace Superdev.Maui.Platforms.Android.Handlers
{
    public partial class CustomTabbedPageHandler : TabbedViewHandler
    {
        private CustomTabbedPage customTabbedPage;
        private ILogger logger;

        public static IPropertyMapper<CustomTabbedPage, CustomTabbedPageHandler> CustomMapper = new PropertyMapper<CustomTabbedPage, CustomTabbedPageHandler>(Mapper)
        {
            [nameof(CustomTabbedPage.HideTabs)] = MapIsHidden,
        };

        private static void MapIsHidden(CustomTabbedPageHandler customTabbedPageHandler, CustomTabbedPage customTabbedPage)
        {
            customTabbedPageHandler.UpdateBottomNavigationVisibility(customTabbedPage);
        }

        public CustomTabbedPageHandler() : base(CustomMapper)
        {
            this.logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<CustomTabbedPageHandler>>();
        }

        protected override void ConnectHandler(AView platformView)
        {
            if (this.VirtualView is CustomTabbedPage customTabbedPage)
            {
                this.customTabbedPage = customTabbedPage;
                this.customTabbedPage.Loaded += this.TabbedPage_Loaded;
            }

            base.ConnectHandler(platformView);
        }

        private void UpdateBottomNavigationVisibility(CustomTabbedPage customTabbedPage)
        {
            try
            {
                // Find the tabs navigation bar:
                // https://github.com/dotnet/maui/blob/main/src/Core/src/Platform/Android/Resources/Layout/navigationlayout.axml

                var coordinatorLayout = FindParent(this.PlatformView, (view) => view is CoordinatorLayout) as CoordinatorLayout;

                {
                    var toptabs = coordinatorLayout.FindViewById(_Microsoft.Android.Resource.Designer.Resource.Id.navigationlayout_toptabs);
                    if (toptabs.LayoutParameters is LayoutParams layoutParams)
                    {
                        if (customTabbedPage.HideTabs)
                        {
                            layoutParams.Height = 0;
                            toptabs.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            layoutParams.Height = LayoutParams.WrapContent;
                        }

                        toptabs.LayoutParameters = layoutParams;
                    }
                }
                {
                    var bottomtabs = coordinatorLayout.FindViewById(_Microsoft.Android.Resource.Designer.Resource.Id.navigationlayout_bottomtabs);
                    if (bottomtabs.LayoutParameters is LayoutParams layoutParams)
                    {
                        if (customTabbedPage.HideTabs)
                        {                            
                            layoutParams.Height = 0;
                            bottomtabs.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            layoutParams.Height = LayoutParams.WrapContent;
                        }

                        bottomtabs.LayoutParameters = layoutParams;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "UpdateBottomNavigationVisibility failed with exception");
            }
        }

        private static AView FindParent(AView view, Func<AView, bool> searchExpression)
        {
            if (view?.Parent is AView pv)
            {
                if (searchExpression(pv))
                {
                    return pv;
                }

                return FindParent(pv, searchExpression);
            }

            return default;
        }

        private void TabbedPage_Loaded(object sender, EventArgs e)
        {
            this.UpdateBottomNavigationVisibility(this.customTabbedPage);
        }

        //Currently the Disconnect Handler needs to be manually called from the App: https://github.com/dotnet/maui/issues/3604
        protected override void DisconnectHandler(AView platformView)
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