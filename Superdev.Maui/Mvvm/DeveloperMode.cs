using System.Windows.Input;

namespace Superdev.Maui.Mvvm
{
    /// <summary>
    /// DeveloperMode allows to unlock a hidden developer mode.
    /// </summary>
    public class DeveloperMode : BindableObject, IDeveloperMode
    {
        private const int UnlockCounterMax = 3;
        private static readonly TimeSpan UnlockCounterTimeSpan = TimeSpan.FromMilliseconds(2000);
        private int unlockCounter;
        private bool unlocked;

        public DeveloperMode()
        {
            this.UnlockCommand = new Command(this.UnlockDeveloperMode, () => true);
        }

        public ICommand UnlockCommand { get; }

        private void UnlockDeveloperMode()
        {
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(UnlockCounterTimeSpan);
                this.unlockCounter = 0;
            });

            if (this.unlockCounter <= UnlockCounterMax)
            {
                this.unlockCounter++;
            }
            else
            {
                this.Unlocked = true;
                this.UnlockedEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool Unlocked
        {
            get => this.unlocked;
            set
            {
                this.unlocked = value;
                this.OnPropertyChanged();
            }
        }

        public event EventHandler<EventArgs> UnlockedEvent;
    }
}