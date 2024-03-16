namespace Superdev.Maui.Services
{
    public interface IToastService
    {
        void LongAlert(string message);

        void ShortAlert(string message);
    }
}