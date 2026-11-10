using Microsoft.Maui.Handlers;

namespace Superdev.Maui.Handlers
{
    public class CustomLayoutHandler : LayoutHandler
    {
        static CustomLayoutHandler()
        {
            Mapper.AppendToMapping("GlobalIgnoreSafeArea", (_, v) =>
            {
                if (v is Layout layout)
                {
                    // The replacement this member points at, SafeAreaElement.IgnoreSafeArea, is internal
                    // in Microsoft.Maui.Controls 10.0, so there is no public API to migrate to yet.
#pragma warning disable CS0618
                    layout.IgnoreSafeArea = true;
#pragma warning restore CS0618
                }
            });
        }
    }
}