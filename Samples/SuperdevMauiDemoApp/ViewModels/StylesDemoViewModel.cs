using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Extensions;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Resources.Styles;
using Superdev.Maui.Services;
using Superdev.Maui.Utils;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class StylesDemoViewModel : BaseViewModel
    {
        private readonly ILogger logger;
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private readonly IDialogService dialogService;
        private readonly IEmail email;

        private ResourceViewModel[] themeResources;

        public StylesDemoViewModel(
            ILogger<StylesDemoViewModel> logger,
            IViewModelErrorHandler viewModelErrorHandler,
            IDialogService dialogService,
            IEmail email)
        {
            this.logger = logger;
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.dialogService = dialogService;
            this.email = email;

            _ = this.LoadData();
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
                var mergedResources = ReflectionHelper.GetPropertyValue<IEnumerable<KeyValuePair<string, object>>>(Application.Current.Resources, "MergedResources");
                var resources = mergedResources
                    .Select(GetResourceViewModel)
                    .OrderBy(vm => vm.ResourceType)
                    .ThenBy(vm => vm.Key)
                    .ToArray();

                this.ThemeResources = resources;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "LoadData failed with exception");
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        private static ResourceViewModel GetResourceViewModel(KeyValuePair<string, object> kvp)
        {
            if (kvp.Value is FontElement fontElement)
            {
                return new FontElementResourceViewModel(kvp.Key, fontElement);
            }

            if (kvp.Value is Color color)
            {
                return new ColorResourceViewModel(kvp.Key, color);
            }

            return new ObjectResourceViewModel(kvp);
        }


        public ResourceViewModel[] ThemeResources
        {
            get => this.themeResources;
            private set => this.SetProperty(ref this.themeResources, value);
        }

        public ICommand DumpResourcesCommand => new Command(this.OnDumpResources);

        private async void OnDumpResources()
        {
            try
            {
                var stringBuilder = new StringBuilder();
                foreach (var resourceViewModel in this.ThemeResources)
                {
                    stringBuilder.AppendLine($"{resourceViewModel.ResourceType};{resourceViewModel.Key};{resourceViewModel.Value}");
                }

                var resourcesDump = stringBuilder.ToString();

                await this.email.ComposeAsync("Dump Resources", resourcesDump);
            }
            catch (Exception ex)
            {
                await this.dialogService.DisplayAlertAsync("Email Error", $"Failed to send mail: {ex}", "OK");
            }
        }
    }
}