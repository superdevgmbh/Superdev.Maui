using Microsoft.Extensions.Logging;
using Superdev.Maui.Effects;

namespace Superdev.Maui.Platform.Effects
{
    /// <summary>
    ///     Adds safe-area padding to the top of the UI element.
    /// </summary>
    public class SafeAreaTopPaddingPlatformEffect : SafeAreaPaddingPlatformEffect
    {
        private readonly SafeAreaPaddingLayout safeAreaPaddingLayout;

        public SafeAreaTopPaddingPlatformEffect()
        {
            this.safeAreaPaddingLayout = new SafeAreaPaddingLayout(SafeAreaPaddingLayout.PaddingPosition.Top);
        }

        protected override Thickness GetSafeAreaPadding(Thickness originalPadding, SafeAreaPaddingLayout _, Thickness safeAreaInsets, bool includeStatusBar)
        {
            var safeAreaPadding = base.GetSafeAreaPadding(originalPadding, this.safeAreaPaddingLayout, safeAreaInsets, includeStatusBar);

            Logger.LogDebug($"SafeAreaTopPaddingEffect.GetSafeAreaPadding returns safeAreaPadding={{{safeAreaPadding.Left}, {safeAreaPadding.Top}, {safeAreaPadding.Right}, {safeAreaPadding.Bottom}}}");
            return safeAreaPadding;
        }
    }
}