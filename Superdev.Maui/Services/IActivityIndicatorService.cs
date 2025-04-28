using Superdev.Maui.Controls;

namespace Superdev.Maui.Services
{
    public interface IActivityIndicatorService
    {
        void Init<T>(T activityIndicatorPage) where T : ContentPage, IActivityIndicatorPage;

        void ShowLoadingPage(string text);

        void HideLoadingPage();
    }
}