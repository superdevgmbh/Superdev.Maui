namespace Superdev.Maui.Services
{
    public partial class DeviceInfo : IDeviceInfo
    {
        private static readonly Lazy<IDeviceInfo> Implementation = new Lazy<IDeviceInfo>(() => new DeviceInfo(), LazyThreadSafetyMode.PublicationOnly);

        public static IDeviceInfo Current => Implementation.Value;

        private readonly Microsoft.Maui.Devices.IDeviceInfo deviceInfo = Microsoft.Maui.Devices.DeviceInfo.Current;

        private DeviceInfo()
        {
        }

        public string Model => this.deviceInfo.Model;

        public string Manufacturer => this.deviceInfo.Manufacturer;

        public string Name => this.deviceInfo.Name;

        public string VersionString => this.deviceInfo.VersionString;

        public Version Version => this.deviceInfo.Version;

        public DevicePlatform Platform => this.deviceInfo.Platform;

        public DeviceIdiom Idiom => this.deviceInfo.Idiom;

        public DeviceType DeviceType => this.deviceInfo.DeviceType;

#if !(ANDROID || IOS)
        public string DeviceId => null;
#endif
    }
}