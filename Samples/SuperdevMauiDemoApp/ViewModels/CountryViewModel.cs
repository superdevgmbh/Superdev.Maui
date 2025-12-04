using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using SuperdevMauiDemoApp.Model;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class CountryViewModel : BaseViewModel
    {
        private bool isSelected;
        private IAsyncRelayCommand? deleteCommand;
        private IRelayCommand<CountryViewModel>? itemSelectedCommand;

        public CountryViewModel(CountryDto countryDto)
        {
            this.Id = countryDto.Id;
            this.Name = countryDto.Name;
            this.IsInitialized = true;
        }

        public int Id { get; }

        public string? Name { get; }

        public bool IsSelected
        {
            get => this.isSelected;
            set => this.SetProperty(ref this.isSelected, value);
        }

        public IAsyncRelayCommand DeleteCommand
        {
            get => this.deleteCommand ??= new AsyncRelayCommand<CountryViewModel>(this.DeleteAsync!);
        }

        private async Task DeleteAsync(CountryViewModel countryViewModel)
        {
            this.IsBusy = true;

            try
            {
                await Task.Delay(3000);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        public IRelayCommand ItemSelectedCommand
        {
            get => this.itemSelectedCommand ??= new RelayCommand<CountryViewModel>(this.ItemSelected!);
        }

        private void ItemSelected(CountryViewModel countryViewModel)
        {
            Debug.WriteLine($"ItemSelected: {countryViewModel.Name}");
        }


        public override string ToString()
        {
            return $"{{Id={this.Id}, Name={this.Name}}}";
        }
    }
}