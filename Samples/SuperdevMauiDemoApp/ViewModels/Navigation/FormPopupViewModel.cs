using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    public class FormPopupViewModel : BaseViewModel
    {
        private readonly INavigationService navigationService;
        private IAsyncRelayCommand? saveCommand;
        private IAsyncRelayCommand? cancelCommand;
        private string? comment;

        public FormPopupViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public string? Comment
        {
            get => this.comment;
            set => this.SetProperty(ref this.comment, value);
        }

        public IAsyncRelayCommand SaveCommand
        {
            get => this.saveCommand ??= new AsyncRelayCommand(
                () => this.navigationService.PopAsync(this.Comment ?? string.Empty));
        }

        public IAsyncRelayCommand CancelCommand
        {
            get => this.cancelCommand ??= new AsyncRelayCommand(() => this.navigationService.PopAsync());
        }
    }
}
