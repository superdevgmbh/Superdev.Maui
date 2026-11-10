using System.Collections.ObjectModel;
using System.Windows.Input;
using Superdev.Maui.Controls;
using Superdev.Maui.Extensions;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;
using Superdev.Maui.Services;
using SuperdevMauiDemoApp.Model;
using SuperdevMauiDemoApp.Services;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class CollectionViewDemoViewModel : BaseViewModel, INavigatedTo
    {
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private readonly IDialogService dialogService;
        private readonly ICountryService countryService;

        private ObservableCollection<CountryViewModel> countries;
        private ScrollToItem? scrollToCountry;

        public CollectionViewDemoViewModel(
            IViewModelErrorHandler viewModelErrorHandler,
            IDialogService dialogService,
            ICountryService countryService)
        {
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.dialogService = dialogService;
            this.countryService = countryService;

            this.countries = new ObservableCollection<CountryViewModel>();
        }

        /// <summary>
        /// Loads the countries once the page is shown. A collection view drops a scroll request which arrives
        /// before its handler exists, so the initial scroll has to wait for the page as well.
        /// </summary>
        public async Task NavigatedToAsync(NavigationDirection navigationDirection)
        {
            if (navigationDirection == NavigationDirection.Forward)
            {
                await this.LoadData();
            }
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

                // Not animated: an animated scroll binds every row on its way to the target.
                this.ScrollToCountry = null;
                this.ScrollToCountry = new ScrollToItem
                {
                    Item = this.Countries.SingleOrDefault(c => c.Id == 50),
                    Position = Microsoft.Maui.Controls.ScrollToPosition.Center,
                    Animated = false
                };
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

        public ICommand SelectCountryCommand => new Command<CountryViewModel>(this.OnSelectCountry);

        private async void OnSelectCountry(CountryViewModel parameter)
        {
            await this.dialogService.DisplayAlertAsync("SelectCountryCommand", $"country: {parameter.Name ?? "null"}", "OK");
        }

        public ScrollToItem? ScrollToCountry
        {
            get => this.scrollToCountry;
            set => this.SetProperty(ref this.scrollToCountry, value);
        }

        public ICommand ScrollToPositionCommand => new Command(this.ScrollToPosition);

        private void ScrollToPosition()
        {
            var nextPosition = this.ScrollToCountry?.Position == Microsoft.Maui.Controls.ScrollToPosition.Start
                ? Microsoft.Maui.Controls.ScrollToPosition.End
                : Microsoft.Maui.Controls.ScrollToPosition.Start;

            this.ScrollToCountry = new ScrollToItem
            {
                Item = null,
                Position = nextPosition,
                Animated = true
            };
        }

        protected override async Task OnRefreshing()
        {
            this.IsBusy = true;
            await Task.Delay(2000);
            this.IsBusy = false;
        }
    }
}
