using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using SuperdevMauiDemoApp.Model;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class CountryViewModel : BaseViewModel
    {
        private bool isSelected;
        private IAsyncRelayCommand deleteCommand;

        public CountryViewModel(CountryDto countryDto)
        {
            this.Id = countryDto.Id;
            this.Name = countryDto.Name;
            this.IsInitialized = true;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsSelected
        {
            get => this.isSelected;
            set => this.SetProperty(ref this.isSelected, value);
        }

        public IAsyncRelayCommand DeleteCommand
        {
            get => this.deleteCommand ??= new AsyncRelayCommand<CountryViewModel>(this.DeleteAsync);
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

        public IAsyncRelayCommand ItemSelectedCommand { get; set; }

        public override string ToString()
        {
            return $"{{Id={this.Id}, Name={this.Name}}}";
        }
    }
}