using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Mvvm;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class LabelDemoViewModel : BaseViewModel
    {
        private readonly ILogger logger;
        private IAsyncRelayCommand hyperlinkCommand;

        public LabelDemoViewModel(
            ILogger<LabelDemoViewModel> logger)
        {
            this.logger = logger;
        }

        public IAsyncRelayCommand HyperlinkCommand
        {
            get => this.hyperlinkCommand ??= new AsyncRelayCommand(this.HyperlinkAsync);
        }

        private async Task HyperlinkAsync()
        {
            this.logger.LogDebug("HyperlinkAsync started");
            await Task.Delay(3000);
            this.logger.LogDebug("HyperlinkAsync finished");
        }
    }
}