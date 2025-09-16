using Microsoft.Maui.Controls.PlatformConfiguration;

namespace Superdev.Maui.Controls.PlatformConfiguration.iOSSpecific
{
    using MauiNavigationPage = Microsoft.Maui.Controls.NavigationPage;

    /// <summary>
    /// The navigation page instance that Microsoft.Maui.Controls created on the iOS platform.
    /// </summary>
    public static class NavigationPage
    {
        #region SwipeBackEnabled

        public static readonly BindableProperty SwipeBackEnabledProperty =
            BindableProperty.Create(
                nameof(SwipeBackEnabled),
                typeof(bool),
                typeof(Page),
                true);

        /// <summary>Returns <see langword="true"/> if the separator is hidden. Otherwise, returns <see langword="false"/>.</summary>
        /// <param name="element">The element for which to return whether the navigation bar separator is hidden.</param>
        /// <returns>see langword="true" /> if the separator is hidden. Otherwise, <see langword="false"/></returns>
        public static bool GetSwipeBackEnabled(BindableObject element)
        {
            return (bool)element.GetValue(SwipeBackEnabledProperty);
        }

        public static void SetSwipeBackEnabled(BindableObject element, bool value)
        {
            element.SetValue(SwipeBackEnabledProperty, value);
        }

        public static IPlatformElementConfiguration<iOS, MauiNavigationPage> SetSwipeBackEnabled(this IPlatformElementConfiguration<iOS, MauiNavigationPage> config, bool value)
        {
            SetSwipeBackEnabled(config.Element, value);
            return config;
        }

        /// <summary>Returns <see langword="true"/> if the separator is hidden. Otherwise, returns <see langword="false"/>.</summary>
        /// <param name="config">The platform configuration for the element for which to return whether the navigation bar separator is hidden.</param>
        /// <returns><see langword="true"/> if the separator is hidden. Otherwise, <see langword="false"/></returns>
        public static bool SwipeBackEnabled(this IPlatformElementConfiguration<iOS, MauiNavigationPage> config)
        {
            return GetSwipeBackEnabled(config.Element);
        }

        #endregion
    }
}