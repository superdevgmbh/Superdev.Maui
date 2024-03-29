using System.ComponentModel;
using System.Windows.Input;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Mvvm
{
    public abstract class BaseViewModel : BindableBase
    {
        private string title = string.Empty;
        private string subTitle = string.Empty;
        private string icon = null;
        private bool isBusy = true;
        private bool isRefreshing;
        private ViewModelError viewModelError;
        private bool isLoadingMore;
        private ViewModelValidation validation;

        protected BaseViewModel()
        {
            this.ViewModelError = ViewModelError.None;
            this.SetupValidationInternal();
        }

        public virtual string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value, nameof(this.Title));
        }

        public string Subtitle
        {
            get => this.subTitle;
            set => this.SetProperty(ref this.subTitle, value, nameof(this.Subtitle));
        }

        public string Icon
        {
            get => this.icon;
            set => this.SetProperty(ref this.icon, value, nameof(this.Icon));
        }

        public bool IsLoadingMore
        {
            get => this.isLoadingMore;
            set => this.SetProperty(ref this.isLoadingMore, value, nameof(this.IsLoadingMore));
        }

        public virtual bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value, nameof(this.IsBusy)))
                {
                    this.RaisePropertyChanged(nameof(this.IsNotBusy));
                    this.RaisePropertyChanged(nameof(this.HasViewModelError));
                    this.RaisePropertyChanged(nameof(this.IsNotBusyAndHasNoViewModelError));
                    this.RaisePropertyChanged(nameof(this.HasNoDataAvailable));
                    this.OnBusyChanged(value);
                }
            }
        }

        public bool IsNotBusy => !this.isBusy;

        protected virtual void OnBusyChanged(bool busy)
        {
        }

        public ICommand RefreshCommand => new Command(async () => await this.InternalRefreshList());

        private async Task InternalRefreshList()
        {
            this.IsRefreshing = true;

            await this.OnRefreshList();

            this.IsRefreshing = false;
        }

        protected virtual async Task OnRefreshList()
        {
            await Task.FromResult<object>(null);
        }

        public bool IsRefreshing
        {
            get => this.isRefreshing;
            set => this.SetProperty(ref this.isRefreshing, value, nameof(this.IsRefreshing));
        }

        public virtual ViewModelError ViewModelError
        {
            get => this.viewModelError;
            set
            {
                if (this.SetProperty(ref this.viewModelError, value, nameof(this.ViewModelError)))
                {
                    this.RaisePropertyChanged(nameof(this.HasViewModelError));
                    this.RaisePropertyChanged(nameof(this.IsNotBusyAndHasNoViewModelError));
                    this.RaisePropertyChanged(nameof(this.HasNoDataAvailable));
                    this.OnViewModelErrorChanged();
                }
            }
        }

        protected virtual void OnViewModelErrorChanged()
        {
        }

        public bool HasViewModelError => this.IsNotBusy && this.viewModelError.HasError;

        public bool IsNotBusyAndHasNoViewModelError => this.IsNotBusy && this.viewModelError.HasError == false;

        /// <summary>
        /// Indicates if the view model has payload data available.
        /// Override this property and return a bool value to indicated if data loading returned any data.
        /// </summary>
        public virtual bool HasDataAvailable => true;

        public bool HasNoDataAvailable => !this.HasDataAvailable && this.IsNotBusy && !this.HasViewModelError;

        public ViewModelValidation Validation
        {
            get => this.validation /*?? throw new InvalidOperationException($"Override {nameof(this.SetupValidation)} before accessing {nameof(this.Validation)}")*/;
            private set => this.SetProperty(ref this.validation, value, nameof(this.Validation));
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
    }
}