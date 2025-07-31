using Superdev.Maui.Behaviors;
using Superdev.Maui.Services;
using Superdev.Maui.Extensions;

namespace Superdev.Maui.Controls
{
    /// <summary>
    /// StatusBar can be used to set the color and style of the status bar at the top of the page.
    /// </summary>
    public static class StatusBar
    {
        public static readonly BindableProperty StatusBarColorProperty = BindableProperty.CreateAttached(
            "StatusBarColor",
            typeof(Color),
            typeof(StatusBar),
            null,
            propertyChanged: OnStatusBarColorPropertyChanged);

        private static void OnStatusBarColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not Page page)
            {
                return;
            }

            var existingBehavior = page.Behaviors.FirstOrDefault<StatusBarBehavior>();

            if (newValue is Color statusBarColor)
            {
                if (existingBehavior == null)
                {
                    var statusBarBehavior = new StatusBarBehavior
                    {
                        StatusBarColor = statusBarColor,
                        NavigationBarColor = GetNavigationBarColor(bindable),
                        StatusBarStyle = GetStyle(bindable)
                    };
                    page.Behaviors.Add(statusBarBehavior);
                }
                else
                {
                    existingBehavior.StatusBarColor = statusBarColor;
                }
            }
            else
            {
                if (existingBehavior != null &&
                    GetNavigationBarColor(bindable) == null &&
                    GetStyle(bindable) == StatusBarStyle.Default)
                {
                    page.Behaviors.Remove(existingBehavior);
                }
            }
        }

        public static Color GetStatusBarColor(BindableObject bindable)
        {
            return (Color)bindable.GetValue(StatusBarColorProperty);
        }

        public static void SetStatusBarColor(BindableObject view, Color value)
        {
            view.SetValue(StatusBarColorProperty, value);
        }

        public static readonly BindableProperty NavigationBarColorProperty = BindableProperty.CreateAttached(
            "NavigationBarColor",
            typeof(Color),
            typeof(StatusBar),
            null,
            propertyChanged: OnNavigationBarColorPropertyChanged);

        private static void OnNavigationBarColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not Page page)
            {
                return;
            }

            var existingBehavior = page.Behaviors.FirstOrDefault<StatusBarBehavior>();

            if (newValue is Color navigationBarColor)
            {
                if (existingBehavior == null)
                {
                    var statusBarBehavior = new StatusBarBehavior
                    {
                        StatusBarColor = GetStatusBarColor(bindable),
                        NavigationBarColor = navigationBarColor,
                        StatusBarStyle = GetStyle(bindable)
                    };
                    page.Behaviors.Add(statusBarBehavior);
                }
                else
                {
                    existingBehavior.StatusBarColor = navigationBarColor;
                }
            }
            else
            {
                if (existingBehavior != null &&
                    GetStatusBarColor(bindable) == null &&
                    GetStyle(bindable) == StatusBarStyle.Default)
                {
                    page.Behaviors.Remove(existingBehavior);
                }
            }
        }

        public static Color GetNavigationBarColor(BindableObject bindable)
        {
            return (Color)bindable.GetValue(NavigationBarColorProperty);
        }

        public static void SetNavigationBarColor(BindableObject view, Color value)
        {
            view.SetValue(NavigationBarColorProperty, value);
        }

        public static readonly BindableProperty StyleProperty = BindableProperty.CreateAttached(
            "Style",
            typeof(StatusBarStyle),
            typeof(StatusBar),
            StatusBarStyle.Default,
            propertyChanged: OnStylePropertyChanged);

        private static void OnStylePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not Page page)
            {
                return;
            }

            var existingBehavior = page.Behaviors.FirstOrDefault<StatusBarBehavior>();

            if (newValue is StatusBarStyle statusBarStyle)
            {
                if (existingBehavior == null)
                {
                    var statusBarBehavior = new StatusBarBehavior
                    {
                        StatusBarColor = GetStatusBarColor(bindable),
                        NavigationBarColor = GetNavigationBarColor(bindable),
                        StatusBarStyle = statusBarStyle
                    };
                    page.Behaviors.Add(statusBarBehavior);
                }
                else
                {
                    existingBehavior.StatusBarStyle = statusBarStyle;
                }
            }
            else
            {
                if (existingBehavior != null &&
                    GetStatusBarColor(bindable) == null &&
                    GetNavigationBarColor(bindable) == null)
                {
                    page.Behaviors.Remove(existingBehavior);
                }
            }
        }

        public static StatusBarStyle GetStyle(BindableObject bindable)
        {
            return (StatusBarStyle)bindable.GetValue(StyleProperty);
        }

        public static void SetStyle(BindableObject view, StatusBarStyle value)
        {
            view.SetValue(StyleProperty, value);
        }
    }
}