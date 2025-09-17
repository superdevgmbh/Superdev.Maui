using Secure = Android.Provider.Settings.Secure;

namespace Superdev.Maui.Services
{
    public partial class DeviceInfo
    {
        public string DeviceId => Secure.GetString(AApplication.Context?.ContentResolver, Secure.AndroidId);
    }
}