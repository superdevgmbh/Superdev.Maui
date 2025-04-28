using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Handlers;
using Superdev.Maui.Controls;
using static Android.Views.ViewGroup;

namespace Superdev.Maui.Platforms.Handlers
{
    public partial class CustomTabbedPageHandler : TabbedViewHandler
    {
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
                //this.VirtualView.AddCleanUpEvent(); // Not needed because, pages call DisconnectHandler automagically
                customTabbedPage.Loaded += this.TabbedPage_Loaded;
            }

            base.ConnectHandler(platformView);
        }

        private void UpdateBottomNavigationVisibility(CustomTabbedPage customTabbedPage)
        {
            try
            {
                // Find the tabs navigation bar:
                // https://github.com/dotnet/maui/blob/main/src/Core/src/Platform/Android/Resources/Layout/navigationlayout.axml

                var parentView = FindParent(this.PlatformView, (view) => view is CoordinatorLayout);
                if (parentView is not CoordinatorLayout coordinatorLayout)
                {
                    return;
                }

                var toolbarPlacement = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.GetToolbarPlacement(customTabbedPage);

                if (toolbarPlacement is ToolbarPlacement.Default or ToolbarPlacement.Top)
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
                            toptabs.Visibility = ViewStates.Visible;
                        }

                        toptabs.LayoutParameters = layoutParams;
                    }
                }
                //else if (this.toolbarPlacement == ToolbarPlacement.Bottom)
                //{
                //    var bottomtabs = coordinatorLayout.FindViewById(_Microsoft.Android.Resource.Designer.Resource.Id.navigationlayout_bottomtabs) as FragmentContainerView;
                //    var fragment = bottomtabs.Fragment as Fragment;
                //    var bottomNavigationView = fragment.View as BottomNavigationView;


                //    if (bottomNavigationView.LayoutParameters is LayoutParams layoutParams)
                //    {
                //        if (customTabbedPage.HideTabs)
                //        {
                //            layoutParams.Height = 0;
                //            bottomNavigationView.Visibility = ViewStates.Gone;
                //        }
                //        else
                //        {
                //            layoutParams.Height = LayoutParams.WrapContent;
                //            bottomNavigationView.Visibility = ViewStates.Visible;
                //        }

                //        bottomNavigationView.LayoutParameters = layoutParams;
                //    }

                //}
                else
                {
                    this.logger.LogWarning($"ToolbarPlacement '{toolbarPlacement}' is currently not supported");
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
            if (sender is CustomTabbedPage customTabbedPage)
            {
                this.UpdateBottomNavigationVisibility(customTabbedPage);
            }
        }

        protected override void DisconnectHandler(AView platformView)
        {
            if (this.VirtualView is CustomTabbedPage customTabbedPage)
            {
                customTabbedPage.Loaded -= this.TabbedPage_Loaded;
            }

            this.logger = null;

            base.DisconnectHandler(platformView);
        }
    }
}