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
        public static readonly BindableProperty ColorProperty = BindableProperty.CreateAttached(
            "Color",
            typeof(Color),
            typeof(StatusBar),
            null,
            propertyChanged: OnColorPropertyChanged);

        private static void OnColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not Page page)
            {
                return;
            }

            var existingBehavior = page.Behaviors.FirstOrDefault<StatusBarBehavior>();

            if (newValue is Color color)
            {
                if (existingBehavior == null)
                {
                    var statusBarBehavior = new StatusBarBehavior
                    {
                        StatusBarColor = color,
                        StatusBarStyle = GetStyle(bindable)
                    };
                    page.Behaviors.Add(statusBarBehavior);
                }
                else
                {
                    existingBehavior.StatusBarColor = color;
                }
            }
            else
            {
                if (existingBehavior != null && GetStyle(bindable) == StatusBarStyle.Default)
                {
                    page.Behaviors.Remove(existingBehavior);
                }
            }
        }

        public static Color GetColor(BindableObject bindable)
        {
            return (Color)bindable.GetValue(ColorProperty);
        }

        public static void SetColor(BindableObject view, Color value)
        {
            view.SetValue(ColorProperty, value);
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
                        StatusBarColor = GetColor(bindable),
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
                if (existingBehavior != null && GetColor(bindable) == null)
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