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

        public static void SetHasKeyboardOffset(BindableObject view, bool? value)
        {
            view.SetValue(HasKeyboardOffsetProperty, value);
        }
    }
}