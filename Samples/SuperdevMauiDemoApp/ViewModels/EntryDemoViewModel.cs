using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using Superdev.Maui.Validation;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class EntryDemoViewModel : BaseViewModel
    {
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private readonly IDialogService dialogService;

        private bool isReadonly;
        private string? userName;
        private int userNameMaxLength;

        public EntryDemoViewModel(
            IViewModelErrorHandler viewModelErrorHandler,
            IDialogService dialogService)
        {
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.dialogService = dialogService;
        }

        protected override Task InitializeAsync()
        {
            return this.LoadData();
        }

        protected override ViewModelValidation SetupValidation()
        {
            // No validate button on this page: fields validate as they change.
            var viewModelValidation = new ViewModelValidation { Mode = ValidationMode.OnChange };

            viewModelValidation.Property(() => this.UserName).NotEmpty();

            return viewModelValidation;
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
                // Demo dynamic adjustment of MaxLength binding
                this.UserNameMaxLength = Math.Max(15, ++this.UserNameMaxLength);
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        public string? UserName
        {
            get => this.userName;
            set => this.SetProperty(ref this.userName, value);
        }

        public int UserNameMaxLength
        {
            get => this.userNameMaxLength;
            set => this.SetProperty(ref this.userNameMaxLength, value);
        }

        public ICommand CalloutCommand => new Command<string>(this.OnCalloutCommand);

        private async void OnCalloutCommand(string parameter)
        {
            await this.dialogService.DisplayAlertAsync("CalloutCommand", $"parameter: {parameter}", "OK");
        }

        public bool IsReadonly
        {
            get => this.isReadonly;
            set => this.SetProperty(ref this.isReadonly, value);
        }
    }
}