using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SampleApp.Services;
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
        private readonly IEmailService emailService;
        private readonly ILocalizer localizer;
        private readonly IActivityIndicatorService activityIndicatorService;

        private CountryViewModel country;
        private string notes;
        private string adminEmailAddress;

        private int numberOfLoads = 0;
        private ICommand saveProfileButtonCommand;
        private IAsyncRelayCommand loadDataButtonCommand;
        private UserDto user;
        private string logContent;
        private ICommand toggleSwitchCommand;
        private bool isReadonly;
        private ICommand longPressCommand;
        private ICommand normalPressCommand;
        private ObservableCollection<CountryViewModel> countries;
        private DateTime? birthdate;
        private bool isSaving;
        private ObservableCollection<ResourceViewModel> themeResources;
        private ICommand navigateToPageCommand;
        private LanguageViewModel language;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            INavigationService navigationService,
            IViewModelErrorHandler viewModelErrorHandler,
            IDialogService dialogService,
            ICountryService countryService,
            IValidationService validationService,
            IEmailService emailService,
            ILocalizer localizer,
            IActivityIndicatorService activityIndicatorService)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.dialogService = dialogService;
            this.countryService = countryService;
            this.validationService = validationService;
            this.emailService = emailService;
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

        public ICommand NavigateToPageCommand =>
            this.navigateToPageCommand ??= new Command<string>(async (s) => await this.OnNavigateToPage(s));

        private async Task OnNavigateToPage(string pageName)
        {
            try
            {
                this.activityIndicatorService.ShowLoadingPage("Loading...");
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

        public string Notes
        {
            get => this.notes;
            set => this.SetProperty(ref this.notes, value);
        }

        public string LogContent
        {
            get => this.logContent;
            set => this.SetProperty(ref this.logContent, value);
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

        public ICommand DumpResourcesCommand => new Command(this.OnDumpResources);

        private async void OnDumpResources()
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var resourceViewModel in this.ThemeResources)
                {
                    sb.AppendLine($"{resourceViewModel.ResourceType};{resourceViewModel.Key};{resourceViewModel.Value}");
                }

                var resourcesDump = sb.ToString();

                await this.emailService.SendEmail("Send Mail", resourcesDump, new List<string> { this.AdminEmailAddress });
            }
            catch (Exception ex)
            {
                await this.dialogService.DisplayAlertAsync("Email Error", $"Failed to send mail: {ex}", "OK");
            }
        }

        public ICommand MailNavigateCommand => new Command(this.OnMailNavigate);

        private async void OnMailNavigate()
        {
            try
            {
                await this.emailService.SendEmail("Send Mail", "Some text....", new List<string> { this.AdminEmailAddress });
            }
            catch (Exception ex)
            {
                await this.dialogService.DisplayAlertAsync("Email Error", $"Failed to send mail: {ex}", "OK");
            }
        }

        public ICommand PostalCodeUnfocusedCommand => new Command(this.OnPostalCodeUnfocused);

        private void OnPostalCodeUnfocused()
        {
            Console.WriteLine("unfocused");
        }

        public ObservableCollection<ResourceViewModel> ThemeResources
        {
            get => this.themeResources;
            private set => this.SetProperty(ref this.themeResources, value);
        }


        protected override void OnBusyChanged(bool busy)
        {
            this.RaisePropertyChanged(nameof(this.CanExecuteSaveProfileButtonCommand));
            this.RaisePropertyChanged(nameof(this.CanExecuteLoadDataButtonCommand));
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

        public ICommand SaveProfileButtonCommand =>
            this.saveProfileButtonCommand ??= new AsyncRelayCommand(
                this.OnSaveProfile,
                () => this.CanExecuteSaveProfileButtonCommand);

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

        protected override async Task OnRefreshList()
        {
            await Task.Delay(1000);
        }

        public bool CanExecuteLoadDataButtonCommand => this.IsNotBusy && !this.IsSaving;

        public IAsyncRelayCommand LoadDataButtonCommand
        {
            get => this.loadDataButtonCommand ??= new AsyncRelayCommand(
                    execute: this.LoadData,
                    canExecute: () => this.CanExecuteLoadDataButtonCommand);
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

                this.LogContent = this.logContent + Environment.NewLine + $"{DateTime.Now:G} LoadData called";
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

                //this.ThemeResources = Application.Current.Resources.MergedDictionaries.SelectMany(md => md)
                //    .Select(kvp => new ResourceViewModel(kvp))
                //    .OrderBy(vm => vm.Key)
                //    //.ThenBy(vm => vm.Key)
                //    .ToObservableCollection();

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