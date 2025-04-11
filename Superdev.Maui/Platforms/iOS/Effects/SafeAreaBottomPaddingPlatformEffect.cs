using Microsoft.Extensions.Logging;
using Superdev.Maui.Effects;

namespace Superdev.Maui.Platform.Effects
{
    /// <summary>
    ///     Adds safe-area padding to the bottom of the UI element.
    /// </summary>
    public class SafeAreaBottomPaddingPlatformEffect : SafeAreaPaddingPlatformEffect
    {
        private readonly SafeAreaPaddingLayout safeAreaPaddingLayout;

        public SafeAreaBottomPaddingPlatformEffect()
        {
            this.safeAreaPaddingLayout = new SafeAreaPaddingLayout(SafeAreaPaddingLayout.PaddingPosition.Bottom);
        }

        protected override Thickness GetSafeAreaPadding(Thickness originalPadding, SafeAreaPaddingLayout _, Thickness safeAreaInsets, bool includeStatusBar)
        {
            var safeAreaPadding = base.GetSafeAreaPadding(originalPadding, this.safeAreaPaddingLayout, safeAreaInsets, includeStatusBar: false);

            Logger.LogDebug($"SafeAreaBottomPaddingEffect.GetSafeAreaPadding returns safeAreaPadding={{{safeAreaPadding.Left}, {safeAreaPadding.Top}, {safeAreaPadding.Right}, {safeAreaPadding.Bottom}}}");
            return safeAreaPadding;
        }
    }
}