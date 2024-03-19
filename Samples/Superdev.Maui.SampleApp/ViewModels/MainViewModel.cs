using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;

namespace Superdev.Maui.SampleApp.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly ILogger<MainViewModel> logger;
        private ICommand showActivityIndicatorCommand;

        public MainViewModel(
            ILogger<MainViewModel> logger)
        {
            this.logger = logger;
        }

        public ICommand ShowActivityIndicatorCommand =>
            this.showActivityIndicatorCommand ??= new Command(async () => await this.OnShowActivityIndicator());

        private async Task OnShowActivityIndicator()
        {
            //this.activityIndicatorService.ShowLoadingPage("Loading...");

            try
            {
                await Task.Delay(3000);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "OnShowActivityIndicator failed with exception");
            }
            finally
            {
                //this.activityIndicatorService.HideLoadingPage();
            }
        }
    }
}
