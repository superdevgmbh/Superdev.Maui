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
                    layout.IgnoreSafeArea = true;
                }
            });
        }
    }
}