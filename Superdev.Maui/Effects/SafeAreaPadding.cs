using System.Diagnostics;
using Superdev.Maui.Extensions;

namespace Superdev.Maui.Effects
{
    public static class SafeAreaPadding
    {
        public static readonly BindableProperty EnableSafeAreaPaddingProperty =
            BindableProperty.CreateAttached(
                "EnableSafeAreaPadding",
                typeof(bool),
                typeof(SafeAreaPadding),
                false,
                propertyChanged: OnEnableSafeAreaPadding);

        public static bool GetEnableSafeAreaPadding(BindableObject element)
        {
            return (bool)element.GetValue(EnableSafeAreaPaddingProperty);
        }

        public static void SetEnableSafeAreaPadding(BindableObject element, bool value)
        {
            element.SetValue(EnableSafeAreaPaddingProperty, value);
        }

        public static readonly BindableProperty LayoutProperty =
            BindableProperty.CreateAttached(
                "Layout",
                typeof(SafeAreaPaddingLayout),
                typeof(SafeAreaPadding),
                null,
                propertyChanged: OnSafeAreaPaddingLayoutChanged);

        private static void OnSafeAreaPaddingLayoutChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue is SafeAreaPaddingLayout layout && layout.Positions.Any())
            {
                AttachEffect(bindable);
            }
            else
            {
                DetachEffect(bindable);
            }
        }

        public static SafeAreaPaddingLayout GetLayout(BindableObject element)
        {
            return (SafeAreaPaddingLayout)element.GetValue(LayoutProperty);
        }

        public static void SetLayout(BindableObject element, SafeAreaPaddingLayout value)
        {
            element.SetValue(LayoutProperty, value);
        }

        public static readonly BindableProperty SafeAreaInsetsProperty =
            BindableProperty.CreateAttached(
                "SafeAreaInsets",
                typeof(Thickness),
                typeof(SafeAreaPadding),
                new Thickness(0, 0, 0, 0));

        public static Thickness GetSafeAreaInsets(BindableObject element)
        {
            return (Thickness)element.GetValue(SafeAreaInsetsProperty);
        }

        public static void SetSafeAreaInsets(BindableObject element, Thickness value)
        {
            element.SetValue(SafeAreaInsetsProperty, value);
        }

        public static readonly BindableProperty ShouldIncludeStatusBarProperty =
            BindableProperty.CreateAttached(
                "ShouldIncludeStatusBar",
                typeof(bool),
                typeof(SafeAreaPadding),
                false);

        public static bool GetShouldIncludeStatusBar(BindableObject element)
        {
            return (bool)element.GetValue(ShouldIncludeStatusBarProperty);
        }

        public static void SetShouldIncludeStatusBar(BindableObject element, bool value)
        {
            element.SetValue(ShouldIncludeStatusBarProperty, value);
        }

        private static void OnEnableSafeAreaPadding(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue is bool isEnable && isEnable)
            {
                AttachEffect(bindable);
            }
            else
            {
                DetachEffect(bindable);
            }
        }

        static void AttachEffect(BindableObject bindableObject)
        {
            Debug.WriteLine($"AttachEffect for {bindableObject?.GetType().Name}...");
            if (!(bindableObject is IElementController controller))
            {
                return;
            }

            if (bindableObject is Element element)
            {
                element.Effects.Add(new SafeAreaPaddingEffect());
                // element.Effects.Add(Effect.Resolve(EffectName));
                Debug.WriteLine($"AttachEffect added for {bindableObject?.GetType().Name}...");
            }
        }

        static void DetachEffect(BindableObject bindableObject)
        {
            Debug.WriteLine($"DetachEffect for {bindableObject?.GetType().Name}...");
            if (!(bindableObject is IElementController controller))
            {
                return;
            }

            if (bindableObject is Element element)
            {
                var toRemove = element.Effects.FirstOrDefault<SafeAreaPaddingEffect>();
                if (toRemove != null)
                {
                    element.Effects.Remove(toRemove);
                    Debug.WriteLine($"DetachEffect removed for {bindableObject?.GetType().Name}...");
                }
            }
        }

        public static IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.iOS, Element> UseSafeArea(this IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.iOS, Element> config, bool includeStatusBar)
        {
            SetShouldIncludeStatusBar(config.Element, includeStatusBar);
            SetEnableSafeAreaPadding(config.Element, true);
            return config;
        }

        public static IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.iOS, Element> UseSafeAreaWithInsets(this IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.iOS, Element> config, bool includeStatusBar, Thickness padding)
        {
            SetShouldIncludeStatusBar(config.Element, includeStatusBar);
            SetEnableSafeAreaPadding(config.Element, true);
            SetSafeAreaInsets(config.Element, padding);
            return config;
        }

        public static IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.iOS, Element> SetSafeAreaInsets(this IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.iOS, Element> config, Thickness padding)
        {
            if (GetEnableSafeAreaPadding(config.Element))
            {
                SetSafeAreaInsets(config.Element, padding);
            }

            return config;
        }
    }
}