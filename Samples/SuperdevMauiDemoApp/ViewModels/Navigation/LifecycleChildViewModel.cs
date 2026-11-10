using Superdev.Maui.Mvvm;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    /// <summary>
    /// A child page which makes the callbacks of the page below observable.
    /// </summary>
    public class LifecycleChildViewModel : BaseViewModel
    {
        public LifecycleChildViewModel()
        {
            this.Title = "Lifecycle child";
        }
    }
}
