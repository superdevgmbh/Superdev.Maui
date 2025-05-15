using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class ButtonDemoViewModel : BaseViewModel
    {
        private readonly IViewModelErrorHandler viewModelErrorHandler;

        private IAsyncRelayCommand saveProfileButtonCommand;
        private IAsyncRelayCommand loadDataButtonCommand;
        private bool isSaving;

        public ButtonDemoViewModel(
            IViewModelErrorHandler viewModelErrorHandler)
        {
            this.viewModelErrorHandler = viewModelErrorHandler;

            _ = this.InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                await this.LoadData();
            }
            finally
            {
                this.IsInitialized = true;
            }
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        protected override void OnBusyChanged(bool busy)
        {
            this.RaisePropertyChanged(nameof(this.CanExecuteSaveProfileButtonCommand));
            this.RaisePropertyChanged(nameof(this.CanExecuteLoadDataButtonCommand));
        }

        public bool CanExecuteLoadDataButtonCommand => this.IsNotBusy && !this.isSaving;

        public IAsyncRelayCommand LoadDataButtonCommand
        {
            get => this.loadDataButtonCommand ??= new AsyncRelayCommand(
                execute: this.LoadData,
                canExecute: () => this.CanExecuteLoadDataButtonCommand);
        }

        public bool CanExecuteSaveProfileButtonCommand => this.IsNotBusy && !this.IsSaving;

        public bool IsSaving
        {
            get => this.isSaving;
            set
            {
                if (this.SetProperty(ref this.isSaving, value))
                {
                    this.RaisePropertyChanged(nameof(this.CanExecuteSaveProfileButtonCommand));
                    this.RaisePropertyChanged(nameof(this.CanExecuteLoadDataButtonCommand));
                }
            }
        }

        public IAsyncRelayCommand SaveProfileButtonCommand
        {
            get
            {
                return this.saveProfileButtonCommand ??= new AsyncRelayCommand(
                    execute: this.OnSaveProfile,
                    canExecute: () => this.CanExecuteSaveProfileButtonCommand);
            }
        }

        private async Task OnSaveProfile()
        {
            this.IsSaving = true;
            await Task.Delay(1000);

            var isValid = await this.Validation.IsValidAsync();
            if (isValid)
            {
                // TODO Save...
            }

            this.IsSaving = false;
        }

    }
}