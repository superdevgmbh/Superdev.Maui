using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Controls;
using Superdev.Maui.Extensions;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using SuperdevMauiDemoApp.Model;
using SuperdevMauiDemoApp.Services;
using Superdev.Maui.Validation;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class PickerDemoViewModel : BaseViewModel
    {
        private readonly ILogger logger;
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private readonly IDialogService dialogService;
        private readonly ICountryService countryService;
        private readonly IDateTime dateTime;

        private string selectedString;
        private ObservableCollection<CountryViewModel> countries;
        private CountryViewModel country;
        private bool isReadonly;
        private DateTime? birthdate;
        private ICommand toggleBirthdateCommand;
        private DateTime patentStartDate;
        private TimeSpan patentStartTime;
        private TimeSpan? patentEndTime;
        private DateRange patentValidityRange;
        private IRelayCommand toggleIsReadonlyCommand;
        private int selectedInt;

        public PickerDemoViewModel(
            ILogger<PickerDemoViewModel> logger,
            IViewModelErrorHandler viewModelErrorHandler,
            IDialogService dialogService,
            ICountryService countryService,
            IDateTime dateTime)
        {
            this.logger = logger;
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.dialogService = dialogService;
            this.countryService = countryService;
            this.dateTime = dateTime;

            this.IntValues = Enumerable.Range(0, 100).ToArray();
            this.StringValues = new []
            {
                null,
                "String 1",
                "String 2",
                "String 3",
            };
            this.SelectedString = null;

            this.Countries = new ObservableCollection<CountryViewModel>();

            _ = this.LoadData();
        }

        protected override ViewModelValidation SetupValidation()
        {
            var viewModelValidation = new ViewModelValidation();

            viewModelValidation.AddValidationFor(nameof(this.Birthdate))
                .When(() => this.Birthdate == null)
                .Show(() => "Birthdate must be set");

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

                var today = this.dateTime.Now.Date;
                var todayInOneMonth = today.AddMonths(1);
                this.PatentValidityRange = new DateRange(today, todayInOneMonth);

                this.PatentStartDate = today.AddDays(1);
                this.PatentStartTime = new TimeSpan(14, 15, 16);
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        public int[] IntValues { get; private set; }

        public int SelectedInt
        {
            get => this.selectedInt;
            set
            {
                if (this.SetProperty(ref this.selectedInt, value))
                {
                    this.logger.LogDebug($"SelectedInt={value}");
                }
            }
        }

        public string[] StringValues { get; private set; }

        public string SelectedString
        {
            get => this.selectedString;
            set
            {
                if (this.SetProperty(ref this.selectedString, value))
                {
                    this.logger.LogDebug($"SelectedString={value ?? "null"}");
                }
            }
        }

        public ObservableCollection<CountryViewModel> Countries
        {
            get => this.countries;
            private set => this.SetProperty(ref this.countries, value);
        }

        public CountryViewModel Country
        {
            get => this.country;
            set => this.SetProperty(ref this.country, value);
        }

        public IRelayCommand ToggleIsReadonlyCommand
        {
            get => this.toggleIsReadonlyCommand ??= new RelayCommand(this.ToggleIsReadonly);
        }

        private void ToggleIsReadonly()
        {
            this.IsReadonly = !this.IsReadonly;
        }

        public bool IsReadonly
        {
            get => this.isReadonly;
            set => this.SetProperty(ref this.isReadonly, value);
        }

        public DateTime? Birthdate
        {
            get => this.birthdate;
            set
            {
                if (this.SetProperty(ref this.birthdate, value))
                {
                    _ = this.Validation.IsValidAsync();
                }
            }
        }

        public DateTime PatentStartDate
        {
            get => this.patentStartDate;
            set => this.SetProperty(ref this.patentStartDate, value);
        }

        public TimeSpan PatentStartTime
        {
            get => this.patentStartTime;
            set => this.SetProperty(ref this.patentStartTime, value);
        }

        public TimeSpan? PatentEndTime
        {
            get => this.patentEndTime;
            set
            {
                if (this.SetProperty(ref this.patentEndTime, value))
                {
                    // this.RaisePropertyChanged(nameof(this.PatentValidityRange));
                }
            }
        }

        public DateRange PatentValidityRange
        {
            get => this.patentValidityRange;
            private set => this.SetProperty(ref this.patentValidityRange, value);
        }
    }
}