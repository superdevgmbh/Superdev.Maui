using UIKit;

namespace Superdev.Maui.Services
{
    public partial class DeviceInfo
    {
        public string DeviceId => UIDevice.CurrentDevice?.IdentifierForVendor?.ToString();
    }
}