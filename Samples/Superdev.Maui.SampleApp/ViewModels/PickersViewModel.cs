using System.Collections.ObjectModel;
using System.Windows.Input;
using Superdev.Maui.Controls;
using Superdev.Maui.Extensions;
using Superdev.Maui.Mvvm;
using Superdev.Maui.SampleApp.Model;
using Superdev.Maui.SampleApp.Services;
using Superdev.Maui.Validation;

namespace Superdev.Maui.SampleApp.ViewModels
{
    public class PickersViewModel : BaseViewModel
    {
        private readonly IDisplayService displayService;
        private readonly ICountryService countryService;
        private string selectedString;
        private ObservableCollection<CountryViewModel> countries;
        private CountryViewModel country;
        private bool isReadonly;
        private DateTime? birthdate;
        private ICommand toggleBirthdateCommand;

        public PickersViewModel(
            IDisplayService displayService,
            ICountryService countryService)
        {
            this.displayService = displayService;
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
                this.Countries = countryDtos.Select(c => new CountryViewModel(c)).Prepend(defaultCountryViewModel).ToObservableCollection();

            }
            catch (Exception ex)
            {
                this.ViewModelError = new ViewModelError(ex.Message, async () => await this.LoadData());
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
                    this.displayService.DisplayAlert("SelectedString", $"value={value}");
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
            get
            {
                return this.toggleBirthdateCommand ??
                       (this.toggleBirthdateCommand = new Command(() => this.ToggleBirthdate()));
            }
        }

        private void ToggleBirthdate()
        {
            this.Birthdate = this.Birthdate == null ? DateTime.Now : null;
        }
    }
}