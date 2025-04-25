using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class StatusBarServiceViewModel : BaseViewModel
    {
        private readonly IStatusBarService statusBarService;

        public StatusBarServiceViewModel(IStatusBarService statusBarService)
        {
            this.statusBarService = statusBarService;

            _ = this.InitializeAsync();
        }
        private async Task InitializeAsync()
        {
            try
            {
                await this.LoadData();
            }
            finally
            {
                this.IsInitialized = true;
            }
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
                await Task.Delay(1000);
                this.statusBarService.SetStatusBarMode(StatusBarStyle.Dark);
                this.statusBarService.SetColor(Colors.Magenta);

                await Task.Delay(1000);
                this.statusBarService.SetStatusBarMode(StatusBarStyle.Light);
                this.statusBarService.SetColor(Colors.DeepSkyBlue);
            }
            catch (Exception ex)
            {
                this.ViewModelError = new ViewModelError(ex.Message, async () => await this.LoadData());
            }

            this.IsBusy = false;
        }

    }
}