namespace Superdev.Maui.Internals
{
    /// <summary>
    /// Bridges the members which .NET MAUI 10 renamed to an <c>Async</c> suffix and marked obsolete in
    /// their old shape, so that this library can target MAUI 9 and MAUI 10 without calling an obsolete
    /// member on either.
    /// </summary>
    /// <remarks>
    /// Delete this file together with the <c>net9.0</c> target frameworks and call the MAUI 10 members
    /// directly at the call sites.
    /// </remarks>
    internal static class MauiCompat
    {
        public static Task FadeToCompatAsync(this VisualElement element, double opacity, uint length, Easing easing)
        {
#if NET10_0_OR_GREATER
            return element.FadeToAsync(opacity, length, easing);
#else
            return element.FadeTo(opacity, length, easing);
#endif
        }

        public static Task TranslateToCompatAsync(
            this VisualElement element, double x, double y, uint length, Easing easing)
        {
#if NET10_0_OR_GREATER
            return element.TranslateToAsync(x, y, length, easing);
#else
            return element.TranslateTo(x, y, length, easing);
#endif
        }

        public static Task ScaleToCompatAsync(this VisualElement element, double scale, uint length, Easing easing)
        {
#if NET10_0_OR_GREATER
            return element.ScaleToAsync(scale, length, easing);
#else
            return element.ScaleTo(scale, length, easing);
#endif
        }

        public static Task DisplayAlertCompatAsync(this Page page, string title, string message, string cancel)
        {
#if NET10_0_OR_GREATER
            return page.DisplayAlertAsync(title, message, cancel);
#else
            return page.DisplayAlert(title, message, cancel);
#endif
        }

        public static Task<bool> DisplayAlertCompatAsync(
            this Page page, string title, string message, string accept, string cancel)
        {
#if NET10_0_OR_GREATER
            return page.DisplayAlertAsync(title, message, accept, cancel);
#else
            return page.DisplayAlert(title, message, accept, cancel);
#endif
        }

        public static Task<string> DisplayActionSheetCompatAsync(
            this Page page, string title, string? cancel, string? destruction, params string[] buttons)
        {
#if NET10_0_OR_GREATER
            return page.DisplayActionSheetAsync(title, cancel, destruction, buttons);
#else
            return page.DisplayActionSheet(title, cancel, destruction, buttons);
#endif
        }
    }
}
