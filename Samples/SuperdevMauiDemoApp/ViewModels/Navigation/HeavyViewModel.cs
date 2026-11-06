using Superdev.Maui.Mvvm;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    /// <summary>
    /// Allocates a large buffer, so that a leaked instance is noticeable.
    /// </summary>
    public class HeavyViewModel : BaseViewModel, IDisposable
    {
        private byte[]? buffer = new byte[32 * 1024 * 1024];

        public HeavyViewModel()
        {
            this.Title = "Heavy page";
        }

        public string BufferInfo => $"Allocated {this.buffer?.Length / (1024 * 1024)} MB.";

        public void Dispose()
        {
            this.buffer = null;
        }
    }
}
