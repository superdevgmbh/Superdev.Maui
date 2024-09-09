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

        public EntryViewModel(IDisplayService displayService)
        {
            this.displayService = displayService;

        }

        protected override ViewModelValidation SetupValidation()
        {
            var viewModelValidation = new ViewModelValidation();

            viewModelValidation.AddValidationFor(nameof(this.UserName))
                .When(() => string.IsNullOrWhiteSpace(this.UserName))
                .Show(() => $"Username must not be empty");


            return viewModelValidation;
        }

        public string UserName
        {
            get => this.userName;
            set => this.SetProperty(ref this.userName, value);
        }

        public ICommand CalloutCommand => new Command<string>(this.OnCalloutCommand);

        private async void OnCalloutCommand(string parameter)
        {
            await this.displayService.DisplayAlert("CalloutCommand", $"parameter: {parameter}");
        }

        public bool IsReadonly
        {
            get => this.isReadonly;
            set => this.SetProperty(ref this.isReadonly, value, nameof(this.IsReadonly));
        }
    }
}