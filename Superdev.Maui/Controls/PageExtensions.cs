namespace Superdev.Maui.Controls
{
    public static class PageExtensions
    {
        public const string HasKeyboardOffset = "HasKeyboardOffset";

        public static readonly BindableProperty HasKeyboardOffsetProperty = BindableProperty.CreateAttached(
            HasKeyboardOffset,
            typeof(bool?),
            typeof(PageExtensions),
            null);

        public static bool? GetHasKeyboardOffset(BindableObject bindable)
        {
            return (bool?)bindable.GetValue(HasKeyboardOffsetProperty);
        }

        /// <summary>
        /// Configures the page to reserve space at the bottom for the on-screen keyboard.
        /// On iOS, this adds a bottom margin to ensure page content remains visible when the keyboard appears.
        /// On Android, this sets WindowSoftInputModeAdjust to <c>Resize</c> when set to <c>true</c>,
        /// so the layout adjusts and content is not obscured by the keyboard.
        /// When set to <c>false</c>, the WindowSoftInputModeAdjust is set to <c>Pan</c>,
        /// so the page pans to keep the focused element in view.
        /// </summary>
        public static void SetHasKeyboardOffset(BindableObject view, bool? value)
        {
            view.SetValue(HasKeyboardOffsetProperty, value);
        }
    }
}