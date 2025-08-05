using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Controls;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;

namespace SuperdevMauiDemoApp.ViewModels
{
    public abstract class DrilldownBaseViewModel : BaseViewModel, IDrilldownView
    {
        private readonly IDialogService dialogService;
        private IAsyncRelayCommand command;

        protected DrilldownBaseViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            this.IsInitialized = true;
        }

        private async Task ExecuteCommandAsync()
        {
            try
            {
                this.IsBusy = true;

                await Task.Delay(2000);

                await this.dialogService.DisplayAlertAsync(this.Title, "Command executed", "OK");
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        public bool IsEnabled { get; set; } = true;

        public ICommand Command
        {
            get => this.command ??= new AsyncRelayCommand(this.ExecuteCommandAsync);
        }

        public object CommandParameter { get; set; }
    }
}