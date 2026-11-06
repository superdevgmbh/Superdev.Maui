using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using Superdev.Maui.Validation;
using SuperdevMauiDemoApp.Services.Validation;

namespace SuperdevMauiDemoApp.ViewModels
{
    /// <summary>
    /// A test bench for the validation API: every rule, every trigger mode switchable at runtime, a fake backend
    /// with a selectable outcome, a nested form, groups, a culture switch, and a live view of each field's state.
    /// </summary>
    public class InputValidationDemoViewModel : BaseViewModel
    {
        private readonly IValidationDemoService validationDemoService;
        private readonly IViewModelErrorHandler viewModelErrorHandler;

        private string? userName;
        private string? email;
        private string? nickname;
        private string? notes;
        private string? ageText;
        private string? amountText;
        private double amount;
        private DateTime? birthdate;
        private DateTime? appointment;
        private string? password;
        private string? passwordConfirm;
        private bool hasDiscount;
        private string? discountPercentText;
        private bool acceptsTerms;
        private string? country;
        private bool isNewItem;
        private string? culture;
        private string? lastResult;
        private IAsyncRelayCommand? saveCommand;
        private IAsyncRelayCommand? saveDraftCommand;
        private IAsyncRelayCommand? publishCommand;
        private IRelayCommand? resetCommand;
        private IAsyncRelayCommand? loadSampleCommand;

        public InputValidationDemoViewModel(
            IValidationDemoService validationDemoService,
            IViewModelErrorHandler viewModelErrorHandler)
        {
            this.validationDemoService = validationDemoService;
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.Address = new AddressFormViewModel();
            this.culture = this.Cultures.Contains(CultureInfo.CurrentCulture.Name) ? CultureInfo.CurrentCulture.Name : null;
            this.IsInitialized = true;
        }

        protected override ViewModelValidation SetupValidation()
        {
            var v = new ViewModelValidation();

            // Presence & text
            v.Property(() => this.UserName)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(20)
                .Matches("^[a-z0-9_]+$").WithMessage("Only lower-case letters, digits and underscores.")
                .MustAsync(this.validationDemoService.IsUserNameAvailableAsync, debounce: TimeSpan.FromMilliseconds(400)).WithMessage("This user name is taken.");

            v.Property(() => this.Email)
                .NotEmpty()
                .EmailAddress()
                .Alias("DuplicateEmail", "InvalidEmail");

            v.Property(() => this.Nickname)
                .MaximumLength(10);

            v.Property(() => this.Country)
                .NotNull().WithMessage("Please pick a country.");

            // Numbers as text, and a typed number. Field names come from Translations/ValidationStrings*.resx.
            v.Property(() => this.AgeText).IsInteger();
            v.Property(() => this.AmountText).IsDecimal();
            v.Property(() => this.Amount).Between(0d, 1000d);

            // Dates
            v.Property(() => this.Birthdate).NotNull().InThePast();
            v.Property(() => this.Appointment).InTheFuture();

            // Cross-field & conditional
            v.Property(() => this.Password)
                .NotEmpty()
                .MinimumLength(8)
                .ValidateOn(ValidationMode.OnChange);
            v.Property(() => this.PasswordConfirm)
                .NotEmpty()
                .EqualTo(() => this.Password);
            v.Property(() => this.DiscountPercentText)
                .NotEmpty()
                .IsInteger()
                .Must(IsPercentage).WithMessage("Enter a percentage between 1 and 100.")
                .When(() => this.HasDiscount);

            // Groups: the draft runs the ungrouped rules only; publishing adds "Submit", a new item adds "Create"
            v.Property(() => this.Notes)
                .MaximumLength(200)
                .NotEmpty().InGroup("Submit")
                .MinimumLength(20).InGroup("Submit");
            v.Property(() => this.AcceptsTerms)
                .IsTrue().WithMessage("Please accept the terms.").InGroups("Submit", "Create");

            // Nested form: the address follows this form's mode, groups and clock
            v.Include(nameof(this.Address), () => this.Address.Validation);

            return v;
        }

        private static bool IsPercentage(string? text)
        {
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var percent) && percent is >= 1 and <= 100;
        }

        #region Policy bar

        public ValidationMode[] Modes { get; } = Enum.GetValues<ValidationMode>();

        public ReValidationMode[] ReValidateModes { get; } = Enum.GetValues<ReValidationMode>();

        public BackendOutcome[] Outcomes { get; } = Enum.GetValues<BackendOutcome>();

        public string[] Cultures { get; } = { "en-US", "de-CH", "de-DE" };

        /// <summary>The mode of this form, which the included address follows.</summary>
        public ValidationMode Mode
        {
            get => this.Validation.Mode;
            set
            {
                this.Validation.Mode = value;
                this.RaisePropertyChanged();
            }
        }

        public ReValidationMode ReValidateMode
        {
            get => this.Validation.ReValidateMode;
            set
            {
                this.Validation.ReValidateMode = value;
                this.RaisePropertyChanged();
            }
        }

        public BackendOutcome Outcome
        {
            get => this.validationDemoService.Outcome;
            set
            {
                this.validationDemoService.Outcome = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The culture the numbers and dates typed as text are parsed in: <c>1,5</c> is a number under de-DE,
        /// <c>1.5</c> under en-US and de-CH.
        /// </summary>
        public string? Culture
        {
            get => this.culture;
            set
            {
                // A picker pushes null while it resets its items; the culture stays as it is then.
                if (string.IsNullOrEmpty(value) || !this.SetProperty(ref this.culture, value))
                {
                    return;
                }

                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(value);
                _ = this.RevalidateCultureSensitiveFieldsAsync();
            }
        }

        public string? LastResult
        {
            get => this.lastResult;
            private set => this.SetProperty(ref this.lastResult, value);
        }

        private async Task RevalidateCultureSensitiveFieldsAsync()
        {
            foreach (var propertyName in new[] { nameof(this.AgeText), nameof(this.AmountText) })
            {
                if (this.Validation.Fields[propertyName].IsTouched)
                {
                    await this.Validation.ValidatePropertyAsync(propertyName);
                }
            }
        }

        #endregion

        #region Fields

        public string? UserName
        {
            get => this.userName;
            set => this.SetProperty(ref this.userName, value);
        }

        public string? Email
        {
            get => this.email;
            set => this.SetProperty(ref this.email, value);
        }

        public string? Nickname
        {
            get => this.nickname;
            set => this.SetProperty(ref this.nickname, value);
        }

        public string? Notes
        {
            get => this.notes;
            set => this.SetProperty(ref this.notes, value);
        }

        public string[] Countries { get; } = { "Switzerland", "Germany", "Austria" };

        public string? Country
        {
            get => this.country;
            set => this.SetProperty(ref this.country, value);
        }

        public string? AgeText
        {
            get => this.ageText;
            set => this.SetProperty(ref this.ageText, value);
        }

        public string? AmountText
        {
            get => this.amountText;
            set => this.SetProperty(ref this.amountText, value);
        }

        /// <summary>A typed number from a slider, which reaches past the allowed range on purpose.</summary>
        public double Amount
        {
            get => this.amount;
            set => this.SetProperty(ref this.amount, Math.Round(value));
        }

        public DateTime? Birthdate
        {
            get => this.birthdate;
            set => this.SetProperty(ref this.birthdate, value);
        }

        public DateTime? Appointment
        {
            get => this.appointment;
            set => this.SetProperty(ref this.appointment, value);
        }

        public string? Password
        {
            get => this.password;
            set => this.SetProperty(ref this.password, value);
        }

        public string? PasswordConfirm
        {
            get => this.passwordConfirm;
            set => this.SetProperty(ref this.passwordConfirm, value);
        }

        public bool HasDiscount
        {
            get => this.hasDiscount;
            set => this.SetProperty(ref this.hasDiscount, value);
        }

        /// <summary>The percentage as typed: bound as text, so that input which is not a number is reported rather than dropped.</summary>
        public string? DiscountPercentText
        {
            get => this.discountPercentText;
            set => this.SetProperty(ref this.discountPercentText, value);
        }

        public bool AcceptsTerms
        {
            get => this.acceptsTerms;
            set => this.SetProperty(ref this.acceptsTerms, value);
        }

        public bool IsNewItem
        {
            get => this.isNewItem;
            set => this.SetProperty(ref this.isNewItem, value);
        }

        public AddressFormViewModel Address { get; }

        #endregion

        #region Commands

        /// <summary>Validate, then let the fake backend answer — the validate-and-act loop.</summary>
        public IAsyncRelayCommand SaveCommand => this.saveCommand ??= new AsyncRelayCommand(this.SaveAsync);

        /// <summary>Runs the ungrouped rules only.</summary>
        public IAsyncRelayCommand SaveDraftCommand => this.saveDraftCommand ??= new AsyncRelayCommand(this.SaveDraftAsync);

        /// <summary>Runs the ungrouped rules plus the "Submit" group, and "Create" for a new item.</summary>
        public IAsyncRelayCommand PublishCommand => this.publishCommand ??= new AsyncRelayCommand(this.PublishAsync);

        public IRelayCommand ResetCommand => this.resetCommand ??= new RelayCommand(this.Validation.Reset);

        /// <summary>
        /// Fills the form from a background thread, as data loaded after <c>ConfigureAwait(false)</c> would be: the
        /// controls wired to the fields still update on the UI thread. The loaded values then become the baseline
        /// <see cref="ViewModelValidation.IsDirty"/> compares against.
        /// </summary>
        public IAsyncRelayCommand LoadSampleCommand => this.loadSampleCommand ??= new AsyncRelayCommand(this.LoadSampleAsync);

        private async Task SaveAsync()
        {
            try
            {
                var succeeded = await this.Validation.ValidateAsync(this.validationDemoService.SavePersonAsync);
                this.LastResult = succeeded ? "Saved." : "Not saved: see the errors above.";
            }
            catch (Exception ex)
            {
                // An action which throws propagates: the usual error handling applies.
                this.LastResult = "The save failed.";
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.SaveAsync);
            }
        }

        private Task LoadSampleAsync()
        {
            return Task.Run(() =>
            {
                this.UserName = "validation_demo";
                this.Email = "demo@superdev.ch";
                this.Country = "Switzerland";
                this.Birthdate = new DateTime(1990, 5, 17);
                this.Password = "correct horse";
                this.PasswordConfirm = "correct horse";
                this.Address.Street = "Bahnhofstrasse 1";
                this.Address.PostalCode = "8001";
                this.Address.Place = "Zürich";
                this.Address.Country = "Switzerland";
                this.Validation.Reset();
                this.LastResult = "Sample loaded on a background thread.";
            });
        }

        private async Task SaveDraftAsync()
        {
            // An empty list: the ungrouped rules only. Without it, the groups of the last Publish would stay active.
            var isValid = await this.Validation.ValidateAsync(groups: Array.Empty<string>());
            this.LastResult = isValid ? "Draft is valid." : "Draft has errors.";
        }

        private async Task PublishAsync()
        {
            var groups = this.IsNewItem ? new[] { "Submit", "Create" } : new[] { "Submit" };
            var isValid = await this.Validation.ValidateAsync(groups);
            this.LastResult = isValid ? "Ready to publish." : "Not ready to publish.";
        }

        #endregion
    }

    public class AddressFormViewModel : BaseViewModel
    {
        private string? street;
        private string? postalCode;
        private string? place;
        private string? country;

        public string? Street
        {
            get => this.street;
            set => this.SetProperty(ref this.street, value);
        }

        public string? PostalCode
        {
            get => this.postalCode;
            set => this.SetProperty(ref this.postalCode, value);
        }

        public string? Place
        {
            get => this.place;
            set => this.SetProperty(ref this.place, value);
        }

        public string[] Countries { get; } = { "Switzerland", "Germany", "Austria" };

        public string? Country
        {
            get => this.country;
            set => this.SetProperty(ref this.country, value);
        }

        protected override ViewModelValidation SetupValidation()
        {
            var v = new ViewModelValidation();
            v.Property(() => this.Street).NotEmpty();
            v.Property(() => this.PostalCode).NotEmpty().Length(4).IsInteger();
            v.Property(() => this.Place).NotEmpty();
            v.Property(() => this.Country).NotNull().WithMessage("Please pick a country.");
            return v;
        }
    }
}
