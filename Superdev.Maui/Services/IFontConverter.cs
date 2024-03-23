namespace Superdev.Maui.Services
{
    public interface IFontConverter : IDisposable
    {
        event EventHandler FontScalingChanged;

        double GetScaledFontSize(double fontSize);
    }
}