using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Superdev.Maui.Navigation;
using Superdev.Maui.Utils;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Mvvm
{
    public abstract class BaseViewModel : BindableBase, IInitialize
    {
        private readonly RefCountBool busyRefCount = new RefCountBool();

        private string? title;
        private string? subTitle;
        private string? icon;
        private bool isRefreshing;
        private ViewModelError viewModelError = ViewModelError.None;
        private ViewModelValidation? validation;
        private bool isInitialized;
        private bool enableBusyRefCount;
        private bool isBusy;
        private Command? refreshCommand;
        private bool isRefreshRunning;

        protected BaseViewModel()
        {
        }

        Task IInitialize.InitializeAsync()
        {
            return this.InitializeAsync();
        }

        /// <summary>
        /// Initializes this viewmodel. The navigation service calls this method once per page instance,
        /// before the page becomes visible. <see cref="IsInitialized"/> is set as soon as it completed.
        /// </summary>
        /// <remarks>
        /// This method is awaited before the navigation transition starts. Keep it fast and either move
        /// longer operations to <see cref="INavigatedTo"/> or start them here without awaiting them.
        /// </remarks>
        protected virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public virtual string? Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        public string? Subtitle
        {
            get => this.subTitle;
            set => this.SetProperty(ref this.subTitle, value);
        }

        public string? Icon
        {
            get => this.icon;
            set => this.SetProperty(ref this.icon, value);
        }

        /// <summary>
        /// Indicates if the viewmodel is initialized. An already initialized viewmodel cannot be uninitialized.
        /// This property participates in the <see cref="IsBusy"/> property: If IsInitialized is <c>false</c>, IsBusy is <c>true</c>.
        /// </summary>
        /// <exception cref="InvalidOperationException">If an IsInitialized is switch from <c>true</c> to <c>false</c>.</exception>
        public bool IsInitialized
        {
            get => this.isInitialized;
            set
            {
                // The guard runs before anything is written: setting the field first and throwing afterwards
                // left the view model uninitialized with no change notification for the busy state.
                if (value == false && this.isInitialized)
                {
                    throw new InvalidOperationException($"{nameof(this.IsInitialized)} must not be set to false.");
                }

                var isBusyBefore = this.IsBusy;

                if (this.SetProperty(ref this.isInitialized, value))
                {
                    var isBusyAfter = this.IsBusy;
                    if (isBusyBefore != isBusyAfter)
                    {
                        this.RaisePropertyChanged(nameof(this.IsBusy));
                        this.RaisePropertyChanged(nameof(this.IsNotBusy));
                        this.RaisePropertyChanged(nameof(this.IsContentReady));
                        this.OnBusyChanged(isBusyAfter);
                    }
                }
            }
        }

        /// <summary>
        /// Enable or disable "BusyRefCount". This is used to count the IsBusy calls with <c>true</c> or <c>false</c> values.
        /// </summary>
        public bool EnableBusyRefCount
        {
            get => this.enableBusyRefCount;
            set
            {
                if (this.enableBusyRefCount != value)
                {
                    this.busyRefCount.Reset();
                    this.enableBusyRefCount = value;
                }
            }
        }

        /// <summary>
        /// Indicates the busy state of the viewmodel. This flag can be used to block the user interface with a loading message.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return (this.EnableBusyRefCount ? this.busyRefCount.Value : this.isBusy) || !this.IsInitialized;
            }
            set
            {
                bool propertyChanged;

                if (this.EnableBusyRefCount)
                {
                    propertyChanged = this.SetProperty(this.busyRefCount, value);
                }
                else
                {
                    propertyChanged = this.SetProperty(ref this.isBusy, value);
                }

                if (propertyChanged)
                {
                    this.RaisePropertyChanged(nameof(this.IsNotBusy));
                    this.RaisePropertyChanged(nameof(this.IsContentReady));
                    this.OnBusyChanged(value);
                }
            }
        }

        public bool IsNotBusy => !this.IsBusy;

        protected virtual void OnBusyChanged(bool busy)
        {
        }

        /// <summary>
        /// Runs <see cref="OnRefreshing" />. The command is cached, so that bindings and
        /// <see cref="Command.CanExecute" /> subscribers observe one instance, and it cannot be
        /// re-entered while a refresh is running.
        /// </summary>
        public ICommand RefreshCommand
        {
            get => this.refreshCommand ??= new Command(this.Refresh, () => !this.isRefreshRunning);
        }

        private async void Refresh()
        {
            if (this.isRefreshRunning)
            {
                return;
            }

            this.isRefreshRunning = true;
            this.refreshCommand?.ChangeCanExecute();

            try
            {
                await this.OnRefreshing();
            }
            catch (Exception ex)
            {
                // A command has no caller which could observe this exception, so without this catch it
                // would surface as an unhandled exception on the UI thread and terminate the application.
                this.OnRefreshFailed(ex);
            }
            finally
            {
                this.isRefreshRunning = false;
                this.refreshCommand?.ChangeCanExecute();
                this.IsRefreshing = false;
            }
        }

        protected virtual Task OnRefreshing()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when <see cref="OnRefreshing" /> threw. The base implementation traces the exception.
        /// Override in order to report it, e.g. through <see cref="ViewModelError" />.
        /// </summary>
        protected virtual void OnRefreshFailed(Exception exception)
        {
            Trace.WriteLine($"{this.GetType().Name}.OnRefreshing failed with exception: {exception}");
        }

        public bool IsRefreshing
        {
            get => this.isRefreshing;
            set => this.SetProperty(ref this.isRefreshing, value);
        }

        public virtual ViewModelError ViewModelError
        {
            get => this.viewModelError;
            set
            {
                if (this.SetProperty(ref this.viewModelError, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsContentReady));
                    this.OnViewModelErrorChanged();
                }
            }
        }

        protected virtual void OnViewModelErrorChanged()
        {
        }

        public bool IsContentReady => this.IsInitialized && this.IsNotBusy && this.ViewModelError == ViewModelError.None;

        /// <summary>
        /// Gets the validation of this view model. It is created from <see cref="SetupValidation"/> on first
        /// access, which is after the constructor of the derived class ran, so that an override can use
        /// constructor-injected dependencies.
        /// </summary>
        public ViewModelValidation Validation
        {
            get
            {
                var current = this.validation;
                if (current != null)
                {
                    return current;
                }

                var created = this.SetupValidation() ?? new ViewModelValidation();

                // Two threads racing here both created one; the first one wins for both.
                return Interlocked.CompareExchange(ref this.validation, created, null) ?? created;
            }
        }

        /// <summary>
        /// Overriding <see cref="SetupValidation"/> activates user input validation for this view model.
        /// It is called once, on the first access to <see cref="Validation"/>.
        /// </summary>
        protected virtual ViewModelValidation? SetupValidation()
        {
            return null;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            // Only a validation which already exists is asked: creating it here would call SetupValidation
            // from within the constructor of the derived class, where the dependencies it needs are not assigned yet.
            // The task does not fault: a throwing rule is reported through ViewModelValidation.OnValidationFailed.
            _ = this.validation?.HandlePropertyChangeAsync(args.PropertyName);
        }

        protected virtual bool SetProperty(RefCountBool refCountBool, bool value, [CallerMemberName] string? propertyName = null)
        {
            var hasChanged = refCountBool.SetValue(value);
            if (hasChanged)
            {
                this.RaisePropertyChanged(propertyName);
            }

            return hasChanged;
        }
    }
}