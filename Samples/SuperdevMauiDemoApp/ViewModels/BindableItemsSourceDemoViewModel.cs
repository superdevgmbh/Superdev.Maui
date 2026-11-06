using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Extensions;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using SuperdevMauiDemoApp.Services;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class BindableItemsSourceDemoViewModel : BaseViewModel
    {
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private readonly IDialogService dialogService;
        private readonly ICountryService countryService;

        private ObservableCollection<CountryViewModel> countries;
        private ICommand? selectCountryCommand;

        public BindableItemsSourceDemoViewModel(
            IViewModelErrorHandler viewModelErrorHandler,
            IDialogService dialogService,
            ICountryService countryService)
        {
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.dialogService = dialogService;
            this.countryService = countryService;
            this.countries = new ObservableCollection<CountryViewModel>();
        }

        protected override Task InitializeAsync()
        {
            return this.LoadData();
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
                var countryDtos = await this.countryService.GetAllAsync();
                this.Countries = countryDtos
                    .Take(5)
                    .Select(c => new CountryViewModel(c, this.SelectCountryCommand))
                    .ToObservableCollection();
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsInitialized = true;
            this.IsBusy = false;
        }

        public ObservableCollection<CountryViewModel> Countries
        {
            get => this.countries;
            private set => this.SetProperty(ref this.countries, value);
        }

        public ICommand SelectCountryCommand => this.selectCountryCommand ??= new AsyncRelayCommand<CountryViewModel>(this.OnSelectCountryAsync);

        private async Task OnSelectCountryAsync(CountryViewModel? countryViewModel)
        {
            await this.dialogService.DisplayAlertAsync("SelectCountryCommand", $"country: {countryViewModel?.Name ?? "null"}", "OK");
        }
    }
}