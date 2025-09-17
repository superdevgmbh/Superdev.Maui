using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using SuperdevMauiDemoApp.Services;
using Superdev.Maui.Extensions;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class BindableItemsSourceDemoViewModel : BaseViewModel
    {
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private readonly ICountryService countryService;

        private ObservableCollection<CountryViewModel> countries;
        private IAsyncRelayCommand appearingCommand;

        public BindableItemsSourceDemoViewModel(
            IViewModelErrorHandler viewModelErrorHandler,
            ICountryService countryService)
        {
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.countryService = countryService;
            this.Countries = new ObservableCollection<CountryViewModel>();
        }

        public IAsyncRelayCommand AppearingCommand
        {
            get => this.appearingCommand ??= new AsyncRelayCommand(this.InitializeAsync);
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

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
                var countryDtos = await this.countryService.GetAllAsync();
                this.Countries = countryDtos
                    .Take(5)
                    .Select(c => new CountryViewModel(c))
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
    }
}
