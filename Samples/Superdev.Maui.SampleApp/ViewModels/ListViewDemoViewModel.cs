using System.Collections.ObjectModel;
using System.Windows.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.SampleApp.Model;
using Superdev.Maui.SampleApp.Services;
using Superdev.Maui.Extensions;

namespace Superdev.Maui.SampleApp.ViewModels
{
    public class ListViewDemoViewModel : BaseViewModel
    {
        private readonly IDisplayService displayService;
        private readonly ICountryService countryService;
        private ObservableCollection<CountryViewModel> countries;

        public ListViewDemoViewModel(
            IDisplayService displayService,
            ICountryService countryService)
        {
            this.displayService = displayService;
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
                this.Countries = countryDtos.Select(c => new CountryViewModel(c)).Prepend(defaultCountryViewModel).ToObservableCollection();
            }
            catch (Exception ex)
            {
                this.ViewModelError = new ViewModelError(ex.Message, async () => await this.LoadData());
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
            await this.displayService.DisplayAlert("SelectCountryCommand", $"country: {parameter.Name ?? "null"}");
        }
    }
}
