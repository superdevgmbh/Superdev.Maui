namespace Superdev.Maui.Services
{
    public interface IGeolocationSettings
    {
        /// <summary>
        /// Launches the system settings UI for location services, allowing the user to enable or modify location settings.
        /// </summary>
        /// <remarks>
        /// This method only exists on Android. On iOS, it launches the general app settings.
        /// </remarks>
        void ShowSettingsUI();
    }
}