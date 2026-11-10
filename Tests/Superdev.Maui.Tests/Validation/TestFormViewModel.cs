using System.Collections.ObjectModel;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    /// <summary>
    /// A view model whose rules are handed in, so that a test declares exactly the form it needs and drives it
    /// through property setters like a page would.
    /// </summary>
    public class TestFormViewModel : BaseViewModel
    {
        private readonly Func<TestFormViewModel, ViewModelValidation>? setup;

        private string? userName;
        private string? email;
        private string? password;
        private string? passwordConfirm;
        private string? age;
        private decimal? amount;
        private bool acceptsTerms;
        private bool hasDiscount;
        private int? discountPercent;
        private DateTime? birthdate;
        private AddressViewModel? address;
        private ObservableCollection<string>? tags;

        public TestFormViewModel(Func<TestFormViewModel, ViewModelValidation>? setup = null)
        {
            this.setup = setup;
        }

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

        public string? Age
        {
            get => this.age;
            set => this.SetProperty(ref this.age, value);
        }

        public decimal? Amount
        {
            get => this.amount;
            set => this.SetProperty(ref this.amount, value);
        }

        public bool AcceptsTerms
        {
            get => this.acceptsTerms;
            set => this.SetProperty(ref this.acceptsTerms, value);
        }

        public bool HasDiscount
        {
            get => this.hasDiscount;
            set => this.SetProperty(ref this.hasDiscount, value);
        }

        public int? DiscountPercent
        {
            get => this.discountPercent;
            set => this.SetProperty(ref this.discountPercent, value);
        }

        public DateTime? Birthdate
        {
            get => this.birthdate;
            set => this.SetProperty(ref this.birthdate, value);
        }

        public AddressViewModel? Address
        {
            get => this.address;
            set => this.SetProperty(ref this.address, value);
        }

        public ObservableCollection<string>? Tags
        {
            get => this.tags;
            set => this.SetProperty(ref this.tags, value);
        }

        protected override ViewModelValidation? SetupValidation()
        {
            return this.setup?.Invoke(this);
        }
    }

    public class AddressViewModel : BaseViewModel
    {
        private string? street;
        private string? postalCode;

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

        protected override ViewModelValidation SetupValidation()
        {
            var v = new ViewModelValidation();
            v.Property(() => this.Street).NotEmpty();
            v.Property(() => this.PostalCode).NotEmpty().Length(4);
            return v;
        }
    }
}
