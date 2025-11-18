using System.Windows.Input;
using Superdev.Maui.Utils.Threading;

namespace Superdev.Maui.Mvvm
{
    public class ViewModelError : BindableBase, IEquatable<ViewModelError>
    {
        public static readonly ViewModelError None = new ViewModelError(null, null, null, null);

        private ICommand retryCommand;
        private string retryButtonText;
        private Func<Task> retryTask;
        private bool isBusy;

        public ViewModelError(string icon, string title, string text)
            : this(icon, title, text, null)
        {
        }

        public ViewModelError(string icon, string title, string text, string retryButtonText, Action retryAction)
            : this(icon, title, text, retryButtonText)
        {
            this.WithRetry(retryAction);
        }

        public ViewModelError(string icon, string title, string text, string retryButtonText, Func<Task> retryTask)
            : this(icon, title, text, retryButtonText)
        {
            this.WithRetry(retryTask);
        }

        public ViewModelError(string icon, string title, string text, string retryButtonText)
        {
            this.Icon = icon;
            this.Title = title;
            this.Text = text;
            this.RetryButtonText = retryButtonText;
        }

        public string Icon { get; }

        public string Title { get; }

        public string Text { get; }

        public bool IsBusy
        {
            get => this.isBusy;
            private set => this.SetProperty(ref this.isBusy, value);
        }

        public string RetryButtonText
        {
            get => this.retryButtonText;
            protected internal set
            {
                if (this.SetProperty(ref this.retryButtonText, value))
                {
                    this.RaisePropertyChanged(nameof(this.CanRetry));
                }
            }
        }

        public ViewModelError WithRetry(Action retryAction, string retryButtonText)
        {
            this.RetryButtonText = retryButtonText;
            return this.WithRetry(retryAction);
        }

        public ViewModelError WithRetry(Action retryAction)
        {
            return this.WithRetry(retryAction == null ? null : () => AsyncHelper.RunAsync(retryAction));
        }

        public ViewModelError WithRetry(Func<Task> retryTask, string retryButtonText)
        {
            this.RetryButtonText = retryButtonText;
            return this.WithRetry(retryTask);
        }

        public ViewModelError WithRetry(Func<Task> retryTask)
        {
            this.RetryTask = retryTask;
            return this;
        }

        /// <summary>
        /// Indicates if this ViewModelError instance can be retried.
        /// </summary>
        public bool CanRetry => this.RetryTask != null && !string.IsNullOrWhiteSpace(this.RetryButtonText);

        /// <summary>
        /// Retry method to run the configured retry task (asynchronous) or retry action (synchronous).
        /// </summary>
        public async Task RetryAsync()
        {
            this.IsBusy = true;

            try
            {
                if (this.RetryTask is Func<Task> task)
                {
                    await task();
                }
            }
            catch
            {
                // Ignore
            }

            this.IsBusy = false;
        }

        private Func<Task> RetryTask
        {
            get => this.retryTask;
            set
            {
                if (this.retryTask != value)
                {
                    this.retryTask = value;
                    this.RetryCommand = value != null ? new Command(() => _ = this.RetryAsync()) : null;
                }
            }
        }

        /// <summary>
        /// RetryCommand used for data binding.
        /// </summary>
        public ICommand RetryCommand
        {
            get => this.retryCommand;
            private set
            {
                if (this.SetProperty(ref this.retryCommand, value))
                {
                    this.RaisePropertyChanged(nameof(this.CanRetry));
                }
            }
        }

        public bool Equals(ViewModelError? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return
                this.Icon == other.Icon && this.Title == other.Title &&
                this.Text == other.Text &&
                this.RetryButtonText == other.RetryButtonText;
        }


        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((ViewModelError)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Icon, this.Title, this.Text, this.RetryButtonText);
        }

        public static bool operator ==(ViewModelError left, ViewModelError right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ViewModelError left, ViewModelError right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return string.Join(", ",
                new[]
                {
                    (nameof(this.Title), this.Title), (nameof(this.Text), this.Text),
                    (nameof(this.RetryButtonText), this.RetryButtonText)
                }.Select(x => $"{x.Item1}=\"{x.Item2 ?? "null"}\""));
        }
    }
}