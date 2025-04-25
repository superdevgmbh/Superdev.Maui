namespace Superdev.Maui.Services
{
    public interface IStatusBarService
    {
        void SetHexColor(string hexColor);

        void SetColor(Color color);

        void SetStatusBarMode(StatusBarStyle statusBarMode);
    }

    public enum StatusBarStyle
    {
        /// <summary>
        /// Status bar style 'Light' for use with bright status bar colors.
        /// </summary>
        Light,

        /// <summary>
        /// Status bar style 'Dark' for use with dark status bar colors.
        /// </summary>
        Dark
    }
}
