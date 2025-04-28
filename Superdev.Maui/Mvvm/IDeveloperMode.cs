using System.Windows.Input;

namespace Superdev.Maui.Mvvm
{
    public interface IDeveloperMode
    {
        ICommand UnlockCommand { get; }

        bool Unlocked { get; set; }

        event EventHandler<EventArgs> UnlockedEvent;
    }
}