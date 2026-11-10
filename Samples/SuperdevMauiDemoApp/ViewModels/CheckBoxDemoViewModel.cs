using Superdev.Maui.Mvvm;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class CheckBoxDemoViewModel : BaseViewModel
    {
        private readonly IViewModelErrorHandler viewModelErrorHandler;

        public CheckBoxDemoViewModel(
            IViewModelErrorHandler viewModelErrorHandler)
        {
            this.viewModelErrorHandler = viewModelErrorHandler;
        }

        protected override Task InitializeAsync()
        {
            return this.LoadData();
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }
    }
}