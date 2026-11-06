using System.Windows.Input;

namespace Superdev.Maui.Mvvm
{
    /// <summary>
    /// DeveloperMode allows to unlock a hidden developer mode: <see cref="Unlock"/> has to be called
    /// <see cref="RequiredUnlockCount"/> times in a row, with no more than <see cref="UnlockTimeout"/>
    /// between two calls - e.g. by tapping a version label four times.
    /// </summary>
    public class DeveloperMode : BindableObject, IDeveloperMode
    {
        /// <summary>
        /// The number of consecutive <see cref="Unlock"/> calls which unlock the developer mode.
        /// </summary>
        public const int RequiredUnlockCount = 4;

        /// <summary>
        /// The maximum duration between two <see cref="Unlock"/> calls which still count as consecutive.
        /// </summary>
        public static readonly TimeSpan UnlockTimeout = TimeSpan.FromSeconds(2);

        private readonly TimeProvider timeProvider;
        private readonly object lockObj = new object();

        private int unlockCounter;
        private long lastUnlockTimestamp;
        private bool unlocked;
        private ICommand? unlockCommand;

        public DeveloperMode()
            : this(TimeProvider.System)
        {
        }

        internal DeveloperMode(TimeProvider timeProvider)
        {
            this.timeProvider = timeProvider;
        }

        public ICommand UnlockCommand => this.unlockCommand ??= new Command(this.Unlock);

        public void Unlock()
        {
            bool unlockNow;

            lock (this.lockObj)
            {
                if (this.unlocked)
                {
                    // Already unlocked: further calls neither count nor raise the event again.
                    return;
                }

                var timestamp = this.timeProvider.GetTimestamp();

                // The window slides with every call: a sequence only expires when the previous call is too old.
                if (this.unlockCounter > 0 && this.timeProvider.GetElapsedTime(this.lastUnlockTimestamp, timestamp) > UnlockTimeout)
                {
                    this.unlockCounter = 0;
                }

                this.lastUnlockTimestamp = timestamp;
                this.unlockCounter++;

                unlockNow = this.unlockCounter >= RequiredUnlockCount;
                if (unlockNow)
                {
                    this.unlockCounter = 0;
                }
            }

            if (unlockNow)
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
                lock (this.lockObj)
                {
                    if (this.unlocked == value)
                    {
                        return;
                    }

                    this.unlocked = value;
                    this.unlockCounter = 0;
                }

                this.OnPropertyChanged();
            }
        }

        public event EventHandler<EventArgs>? UnlockedEvent;
    }
}