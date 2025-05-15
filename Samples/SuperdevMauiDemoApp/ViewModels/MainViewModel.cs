using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SampleApp.ViewModels;
using Superdev.Maui.Extensions;
using Superdev.Maui.Localization;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using SuperdevMauiDemoApp.Model;
using SuperdevMauiDemoApp.Services;
using SuperdevMauiDemoApp.Services.Validation;
using Superdev.Maui.Validation;
using SuperdevMauiDemoApp.Services.Navigation;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ILogger<MainViewModel> logger;
        private readonly INavigationService navigationService;
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private readonly IDialogService dialogService;
        private readonly ICountryService countryService;
        private readonly IValidationService validationService;
        private readonly ILocalizer localizer;
        private readonly IActivityIndicatorService activityIndicatorService;

        private CountryViewModel country;
        private string adminEmailAddress;

        private int numberOfLoads;
        private UserDto user;
        private ICommand toggleSwitchCommand;
        private bool isReadonly;
        private ICommand longPressCommand;
        private ICommand normalPressCommand;
        private ObservableCollection<CountryViewModel> countries;
        private DateTime? birthdate;
        private IAsyncRelayCommand navigateToPageCommand;
        private LanguageViewModel language;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            INavigationService navigationService,
            IViewModelErrorHandler viewModelErrorHandler,
            IDialogService dialogService,
            ICountryService countryService,
            IValidationService validationService,
            ILocalizer localizer,
            IActivityIndicatorService activityIndicatorService)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.dialogService = dialogService;
            this.countryService = countryService;
            this.validationService = validationService;
            this.localizer = localizer;
            this.activityIndicatorService = activityIndicatorService;

            this.EnableBusyRefCount = false;
            this.ViewModelError = ViewModelError.None;
            this.User = new UserDto();
            this.Countries = new ObservableCollection<CountryViewModel>();

            this.Languages = new ObservableCollection<LanguageViewModel>
            {
                new LanguageViewModel(new CultureInfo("en")), new LanguageViewModel(new CultureInfo("de"))
            };
            this.language = this.Languages.First();

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

        public ObservableCollection<LanguageViewModel> Languages { get; }

        public LanguageViewModel Language
        {
            get => this.language;
            set
            {
                this.language = value;

                if (value != null)
                {
                    var cultureInfo = value.Dto;

                    this.localizer.SetCultureInfo(cultureInfo);

                    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
                }

                // Update all bindings
                this.RaisePropertyChanged("");
            }
        }

        private UserDto User
        {
            get => this.user;
            set
            {
                if (this.SetProperty(ref this.user, value, nameof(this.User)))
                {
                    this.RaisePropertyChanged(nameof(this.UserId));
                    this.RaisePropertyChanged(nameof(this.UserName));
                }
            }
        }

        public int UserId
        {
            get => this.User?.Id ?? 0;
            set => this.SetProperty(this.User, value, nameof(this.UserId),
                nameof(this.User.Id)); // Sync property value based on specified string
        }

        public string UserName
        {
            get => this.User?.UserName;
            set => this.SetProperty(this.User, value); // Sync property value based on property name
        }

        public DateTime? Birthdate
        {
            get => this.birthdate;
            set => this.SetProperty(ref this.birthdate, value, nameof(this.Birthdate));
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

        public IAsyncRelayCommand NavigateToPageCommand
        {
            get => this.navigateToPageCommand ??= new AsyncRelayCommand<string>(this.OnNavigateToPage);
        }

        private async Task OnNavigateToPage(string pageName)
        {
            try
            {
                this.activityIndicatorService.ShowLoadingPage("Navigating...");
                // await Task.Delay(5000);
                await this.navigationService.PushAsync(pageName);
            }
            catch (Exception ex)
            {
                var viewModelError = this.viewModelErrorHandler.FromException(ex);
                await this.dialogService.DisplayAlertAsync(viewModelError, "OK", "Cancel");
            }
            finally
            {
                this.activityIndicatorService.HideLoadingPage();
            }
        }

        public string AdminEmailAddress
        {
            get => this.adminEmailAddress;
            set => this.SetProperty(ref this.adminEmailAddress, value);
        }

        public ICommand NormalPressCommand =>
            this.normalPressCommand ??
            (this.normalPressCommand =
                new Command<string>(async (message) => await this.dialogService.DisplayAlertAsync("NormalPressCommand", message, "OK")));

        public ICommand LongPressCommand => this.longPressCommand ??=
            new Command<string>(async (message) => await this.dialogService.DisplayAlertAsync("LongPressCommand", message, "OK"));

        public ICommand PostalCodeUnfocusedCommand => new Command(this.OnPostalCodeUnfocused);

        private void OnPostalCodeUnfocused()
        {
            Console.WriteLine("unfocused");
        }

        protected override async Task OnRefreshList()
        {
            await Task.Delay(1000);
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
                this.activityIndicatorService.ShowLoadingPage("Test loading message...");
                await Task.Delay(3000);

                this.User = new UserDto { Id = 1, UserName = "thomasgalliker" };
                this.UserId = 2;

                this.numberOfLoads++;

                if (this.numberOfLoads % 2 == 0)
                {
                    // Simulate a data load exception
                    throw new InvalidOperationException("Failed to load data. Try again.");
                }

                var defaultCountryViewModel = new CountryViewModel(new CountryDto { Name = null });
                var countryDtos = (await this.countryService.GetAllAsync()).ToList();

                // Set countries all at once
                this.Countries.Clear();
                this.Countries =
                    new ObservableCollection<CountryViewModel>(countryDtos.Select(c => new CountryViewModel(c))
                        .Prepend(defaultCountryViewModel));

                // Set countries one after the other
                this.Countries.Clear();
                this.Countries.AddRange(countryDtos.Select(c => new CountryViewModel(c)).Prepend(defaultCountryViewModel));

                //this.Notes = $"Test test test{Environment.NewLine}Line 2 text text text";
                this.AdminEmailAddress = "thomas@bluewin.ch";
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }
            finally
            {
                this.activityIndicatorService.HideLoadingPage();
            }

            this.IsBusy = false;
        }

        protected override ViewModelValidation SetupValidation()
        {
            var viewModelValidation = new ViewModelValidation();

            viewModelValidation.AddValidationFor(nameof(this.UserName))
                .When(() => string.IsNullOrWhiteSpace(this.UserName))
                .Show(() => $"Username must not be empty");

            viewModelValidation.AddValidationFor(nameof(this.Birthdate))
                .When(() => this.Birthdate == null)
                .Show(() => $"Birthdate must be set");

            viewModelValidation.AddDelegateValidation(nameof(this.UserName))
                .Validate(async () => (await this.validationService.ValidatePersonAsync(this.CreatePerson())).Errors,
                    TimeSpan.FromMilliseconds(1000));

            return viewModelValidation;
        }

        private PersonDto CreatePerson()
        {
            return new PersonDto { UserName = this.UserName };
        }

        public bool IsReadonly
        {
            get => this.isReadonly;
            set => this.SetProperty(ref this.isReadonly, value, nameof(this.IsReadonly));
        }

        public ICommand ToggleSwitchCommand
        {
            get => this.toggleSwitchCommand ??= new Command(() => { this.IsReadonly = !this.IsReadonly; });
        }
    }
}