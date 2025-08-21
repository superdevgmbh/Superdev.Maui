namespace Superdev.Maui.Resources.Styles
{
    /// <summary>
    /// Workaround for a MAUI bug
    /// https://github.com/dotnet/maui/issues/19748
    /// </summary>
    public class ClearButtonVisibilityWorkaroundFor19748Extension : IMarkupExtension<ClearButtonVisibility>
    {
        public ClearButtonVisibility ProvideValue(IServiceProvider serviceProvider)
        {
#if IOS
            if (!UIKit.UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                return Microsoft.Maui.ClearButtonVisibility.Never;
            }
#endif

            return ClearButtonVisibility.WhileEditing;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return this.ProvideValue(serviceProvider);
        }
    }
}