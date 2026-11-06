using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using SuperdevMauiDemoApp.Model;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class CountryViewModel : BaseViewModel
    {
        private bool isSelected;
        private IAsyncRelayCommand? deleteCommand;

        public CountryViewModel(CountryDto countryDto)
        {
            this.Id = countryDto.Id;
            this.Name = countryDto.Name;
            this.IsInitialized = true;
        }

        public CountryViewModel(CountryDto countryDto, ICommand selectCommand)
            : this(countryDto)
        {
            this.SelectCommand = selectCommand;
        }

        public int Id { get; }

        public string? Name { get; }

        /// <summary>
        /// Runs when the row is tapped, with this item as parameter. The owning page provides it.
        /// </summary>
        public ICommand? SelectCommand { get; }

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

        public override string ToString()
        {
            return $"{{Id={this.Id}, Name={this.Name}}}";
        }
    }
}