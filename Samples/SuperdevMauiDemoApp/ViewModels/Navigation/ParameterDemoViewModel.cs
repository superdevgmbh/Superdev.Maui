using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    /// <summary>
    /// Demonstrates typed navigation parameters and awaiting a result from another page.
    /// </summary>
    public class ParameterDemoViewModel : BaseViewModel
    {
        private readonly INavigationService navigationService;

        private IAsyncRelayCommand? pushWithParameterCommand;
        private IAsyncRelayCommand? pickColorCommand;
        private string? selectedColor;

        public ParameterDemoViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.Title = "Parameters and results";
        }

        public string? SelectedColor
        {
            get => this.selectedColor;
            private set => this.SetProperty(ref this.selectedColor, value);
        }

        public IAsyncRelayCommand PushWithParameterCommand
        {
            get => this.pushWithParameterCommand ??= new AsyncRelayCommand(
                () => this.navigationService.PushAsync<ParameterDetailViewModel>(
                    new ParameterDetailViewModel.NavigationParameter
                    {
                        Title = "Passed by navigation parameter",
                        Value = 42
                    }));
        }

        public IAsyncRelayCommand PickColorCommand
        {
            get => this.pickColorCommand ??= new AsyncRelayCommand(this.PickColorAsync);
        }

        private async Task PickColorAsync()
        {
            // The pushed page returns its result; no TaskCompletionSource has to be passed around.
            this.SelectedColor = await this.navigationService.PushForResultAsync<string>("ColorPickerPage");
        }
    }
}
