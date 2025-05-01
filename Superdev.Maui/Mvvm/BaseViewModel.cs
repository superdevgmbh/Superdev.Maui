using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Superdev.Maui.Utils;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Mvvm
{
    public abstract class BaseViewModel : BindableBase
    {
        private readonly RefCountBool busyRefCount = new RefCountBool();

        private string title;
        private string subTitle;
        private string icon;
        private bool isRefreshing;
        private ViewModelError viewModelError;
        private ViewModelValidation validation;
        private bool isInitialized;
        private bool enableBusyRefCount;
        private bool isBusy;

        protected BaseViewModel()
        {
            this.ViewModelError = ViewModelError.None;
            this.SetupValidationInternal();
        }

        public virtual string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        public string Subtitle
        {
            get => this.subTitle;
            set => this.SetProperty(ref this.subTitle, value);
        }

        public string Icon
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
                var isBusyBefore = this.IsBusy;

                if (this.SetProperty(ref this.isInitialized, value))
                {
                    if (value == false)
                    {
                        throw new InvalidOperationException($"{nameof(this.IsInitialized)} must not be set to false.");
                    }

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
        public virtual bool IsBusy
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

        public ICommand RefreshCommand => new Command(async () => await this.InternalRefreshList());

        private async Task InternalRefreshList()
        {
            this.IsRefreshing = true;

            try
            {
                await this.OnRefreshList();
            }
            finally
            {
                this.IsRefreshing = false;
            }
        }

        protected virtual async Task OnRefreshList()
        {
            await Task.FromResult<object>(null);
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

        public ViewModelValidation Validation
        {
            get => this.validation /*?? throw new InvalidOperationException($"Override {nameof(this.SetupValidation)} before accessing {nameof(this.Validation)}")*/;
            private set => this.SetProperty(ref this.validation, value);
        }

        private void SetupValidationInternal()
        {
            var viewModelValidation = this.SetupValidation();
            if (viewModelValidation != null)
            {
                this.Validation = viewModelValidation;
                this.Validation.TrySetContext(this);
            }
        }

        /// <summary>
        /// Overriding <see cref="SetupValidation"/> activates user input validation for this view model.
        /// </summary>
        protected virtual ViewModelValidation SetupValidation()
        {
            return null;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
            this.Validation?.HandlePropertyChange(args.PropertyName);
        }

        protected virtual bool SetProperty(RefCountBool refCountBool, bool value, [CallerMemberName] string propertyName = null)
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