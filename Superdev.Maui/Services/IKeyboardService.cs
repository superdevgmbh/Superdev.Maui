using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Superdev.Maui.Services
{
    public interface IKeyboardService
    {
        void UseWindowSoftInputModeAdjust(object target, WindowSoftInputModeAdjust windowSoftInputModeAdjust);
        
        void ResetWindowSoftInputModeAdjust(object target);
    }
}