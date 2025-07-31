namespace Superdev.Maui.Services
{
    public interface IDeviceInfo : Microsoft.Maui.Devices.IDeviceInfo
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IDateTime"/>.
        /// </summary>
        public static IDeviceInfo Current => DeviceInfo.Current;

        string DeviceId { get; }
    }
}