using System.Collections.ObjectModel;
using System.Windows.Input;
using Superdev.Maui.Controls;
using Superdev.Maui.Extensions;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using SuperdevMauiDemoApp.Model;
using SuperdevMauiDemoApp.Services;
using Superdev.Maui.Validation;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class PickersViewModel : BaseViewModel
    {
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private readonly IDialogService dialogService;
        private readonly ICountryService countryService;
        private string selectedString;
        private ObservableCollection<CountryViewModel> countries;
        private CountryViewModel country;
        private bool isReadonly;
        private DateTime? birthdate;
        private ICommand toggleBirthdateCommand;

        public PickersViewModel(
            IViewModelErrorHandler viewModelErrorHandler,
            IDialogService dialogService,
            ICountryService countryService)
        {
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.dialogService = dialogService;
            this.countryService = countryService;

            this.StringValues = new ObservableCollection<string>
            {
                null,
                "String 1",
                "String 10"
            };

            this.Countries = new ObservableCollection<CountryViewModel>();

            var referenceDate = DateTime.Now;
            this.BirthdateValidityRange = new DateRange(start: referenceDate.AddDays(-2), end: referenceDate.AddDays(2));
            this.Birthdate = referenceDate;

            _ = this.LoadData();
        }

        protected override ViewModelValidation SetupValidation()
        {
            var viewModelValidation = new ViewModelValidation();

            viewModelValidation.AddValidationFor(nameof(this.Birthdate))
                .When(() => this.Birthdate == null)
                .Show(() => $"Birthdate must be set");

            return viewModelValidation;
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
                var defaultCountryViewModel = new CountryViewModel(new CountryDto { Name = null });
                var countryDtos = await this.countryService.GetAllAsync();
                this.Countries = countryDtos
                    .Select(c => new CountryViewModel(c))
                    .Prepend(defaultCountryViewModel)
                    .ToObservableCollection();
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        public ObservableCollection<string> StringValues { get; set; }

        public string SelectedString
        {
            get => this.selectedString;
            set
            {
                if (this.SetProperty(ref this.selectedString, value))
                {
                    this.dialogService.DisplayAlertAsync("SelectedString", $"value={value}", "OK");
                }
            }
        }

        public ObservableCollection<CountryViewModel> Countries
        {
            get => this.countries;
            private set => this.SetProperty(ref this.countries, value, nameof(this.Countries));
        }

        public CountryViewModel Country
        {
            get => this.country;
            set => this.SetProperty(ref this.country, value);
        }

        public bool IsReadonly
        {
            get => this.isReadonly;
            set => this.SetProperty(ref this.isReadonly, value, nameof(this.IsReadonly));
        }

        public DateTime? Birthdate
        {
            get => this.birthdate;
            set
            {
                if (this.SetProperty(ref this.birthdate, value, nameof(this.Birthdate)))
                {
                    _ = this.Validation.IsValidAsync();
                }
            }
        }

        public DateRange BirthdateValidityRange { get; }

        public ICommand ToggleBirthdateCommand
        {
            get => this.toggleBirthdateCommand ??= new Command(this.ToggleBirthdate);
        }

        private void ToggleBirthdate()
        {
            this.Birthdate = this.Birthdate == null ? DateTime.Now : null;
        }
    }
}