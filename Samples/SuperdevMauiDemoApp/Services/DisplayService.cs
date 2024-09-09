namespace SuperdevMauiDemoApp.Services
{
    public class DisplayService : IDisplayService
    {
        public Task DisplayAlert(string title, string message)
        {
            return Application.Current.MainPage.DisplayAlert(title, message, "OK");
        }
    }
}