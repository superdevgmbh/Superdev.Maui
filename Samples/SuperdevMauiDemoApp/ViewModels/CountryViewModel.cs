using System.Windows.Input;
using Superdev.Maui.Mvvm;
using SuperdevMauiDemoApp.Model;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class CountryViewModel : BindableBase
    {
        private bool isSelected;

        public CountryViewModel(CountryDto countryDto)
        {
            this.Id = countryDto.Id;
            this.Name = countryDto.Name;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsSelected
        {
            get => this.isSelected;
            set => this.SetProperty(ref this.isSelected, value, nameof(this.IsSelected));
        }

        public ICommand DeleteCommand { get; set; }

        public ICommand ItemSelectedCommand { get; set; }

        public override string ToString()
        {
            return $"{this.Id}) {this.Name}";
        }
    }
}