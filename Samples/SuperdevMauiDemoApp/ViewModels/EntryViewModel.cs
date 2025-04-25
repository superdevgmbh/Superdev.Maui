using System.Windows.Input;
using Superdev.Maui.Mvvm;
using SuperdevMauiDemoApp.Services;
using Superdev.Maui.Validation;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class EntryViewModel : BaseViewModel
    {
        private readonly IDisplayService displayService;

        private bool isReadonly;
        private string userName;
        private int userNameMaxLength;

        public EntryViewModel(IDisplayService displayService)
        {
            this.displayService = displayService;

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

        protected override ViewModelValidation SetupValidation()
        {
            var viewModelValidation = new ViewModelValidation();

            viewModelValidation.AddValidationFor(nameof(this.UserName))
                .When(() => string.IsNullOrWhiteSpace(this.UserName))
                .Show(() => $"Username must not be empty");

            return viewModelValidation;
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
                // Demo dynamic adjustment of MaxLength binding
                this.UserNameMaxLength = Math.Max(3, ++this.UserNameMaxLength);
            }
            catch (Exception ex)
            {
                this.ViewModelError = new ViewModelError(ex.Message, async () => await this.LoadData());
            }

            this.IsBusy = false;
        }

        public string UserName
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
            await this.displayService.DisplayAlert("CalloutCommand", $"parameter: {parameter}");
        }

        public bool IsReadonly
        {
            get => this.isReadonly;
            set => this.SetProperty(ref this.isReadonly, value);
        }
    }
}