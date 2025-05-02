using System.Collections.ObjectModel;
using System.Windows.Input;
using Superdev.Maui.Controls;
using Superdev.Maui.Mvvm;
using SuperdevMauiDemoApp.Model;
using SuperdevMauiDemoApp.Services;
using Superdev.Maui.Extensions;
using Superdev.Maui.Services;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class ListViewDemoViewModel : BaseViewModel
    {
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private readonly IDialogService dialogService;
        private readonly ICountryService countryService;

        private ObservableCollection<CountryViewModel> countries;
        private ScrollToItem scrollToCountry;

        public ListViewDemoViewModel(
            IViewModelErrorHandler viewModelErrorHandler,
            IDialogService dialogService,
            ICountryService countryService)
        {
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.dialogService = dialogService;
            this.countryService = countryService;
            this.Countries = new ObservableCollection<CountryViewModel>();

            _ = this.LoadData();
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

                this.ScrollToCountry = new ScrollToItem
                {
                    Item = this.Countries.SingleOrDefault(c => c.Id == 50),
                    Position = ScrollToPosition.Center,
                    Animated = true
                };
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        public ObservableCollection<CountryViewModel> Countries
        {
            get => this.countries;
            private set => this.SetProperty(ref this.countries, value);
        }

        public ICommand SelectCountryCommand => new Command<CountryViewModel>(this.OnSelectCountry);

        private async void OnSelectCountry(CountryViewModel parameter)
        {
            await this.dialogService.DisplayAlertAsync("SelectCountryCommand", $"country: {parameter.Name ?? "null"}", "OK");
        }

        public ScrollToItem ScrollToCountry
        {
            get => this.scrollToCountry;
            set => this.SetProperty(ref this.scrollToCountry, value);
        }
    }
}
