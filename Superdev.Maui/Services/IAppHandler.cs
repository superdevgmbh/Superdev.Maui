namespace Superdev.Maui.Services
{
    public interface IAppHandler
    {
        bool LaunchApp(string uri);

        bool OpenAppSettings();

        bool OpenLocationServiceSettings();
    }
}