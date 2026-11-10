using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    /// <summary>
    /// Returns the selected color to the page which pushed this page.
    /// </summary>
    public class ColorPickerViewModel : BaseViewModel
    {
        private readonly INavigationService navigationService;
        private IAsyncRelayCommand<string>? selectCommand;

        public ColorPickerViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.Title = "Pick a color";
        }

        public IReadOnlyList<string> Colors { get; } = new[] { "Red", "Green", "Blue" };

        public IAsyncRelayCommand<string> SelectCommand
        {
            get => this.selectCommand ??= new AsyncRelayCommand<string>(
                color => this.navigationService.PopAsync(color!));
        }
    }
}
