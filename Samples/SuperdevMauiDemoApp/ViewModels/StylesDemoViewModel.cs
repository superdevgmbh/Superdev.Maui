using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Superdev.Maui;
using Superdev.Maui.Extensions;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Resources.Styles;
using Superdev.Maui.Services;
using Superdev.Maui.Utils;
using Font = Microsoft.Maui.Font;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class StylesDemoViewModel : BaseViewModel
    {
        private readonly ILogger logger;
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private readonly IDialogService dialogService;
        private readonly IThemeHelper themeHelper;
        private readonly IClipboard clipboard;
        private readonly IEmail email;

        private IAsyncRelayCommand dumpResourcesCommand;
        private IRelayCommand updateColorsCommand;
        private IAsyncRelayCommand loadDataCommand;
        private IRelayCommand switchThemesCommand;
        private AppTheme appTheme;
        private ColorResourceViewModel[] colors;
        private FontResourceViewModel[] fonts;
        private ObjectResourceViewModel[] resources;

        public StylesDemoViewModel(
            ILogger<StylesDemoViewModel> logger,
            IViewModelErrorHandler viewModelErrorHandler,
            IDialogService dialogService,
            IThemeHelper themeHelper,
            IClipboard clipboard,
            IEmail email)
        {
            this.logger = logger;
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.dialogService = dialogService;
            this.themeHelper = themeHelper;
            this.clipboard = clipboard;
            this.email = email;

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

        public IAsyncRelayCommand LoadDataCommand
        {
            get => this.loadDataCommand ??= new AsyncRelayCommand(this.LoadData);
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
                await Task.Yield();

                this.appTheme = this.themeHelper.AppTheme;
                this.RaisePropertyChanged(nameof(this.AppTheme));

                var mergedResources = ReflectionHelper.GetPropertyValue<IEnumerable<KeyValuePair<string, object>>>(Application.Current.Resources, "MergedResources");
                var resources = mergedResources
                    .Select(GetResourceViewModel)
                    .OrderBy(vm => vm.ResourceType)
                    .ThenBy(vm => vm.Key)
                    .ToArray();

                this.Colors = resources
                    .OfType<ColorResourceViewModel>()
                    .ToArray();

                this.Fonts = resources
                    .OfType<FontResourceViewModel>()
                    .ToArray();

                this.Resources = resources
                    .OfType<ObjectResourceViewModel>()
                    .ToArray();
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
                return new FontResourceViewModel(kvp.Key, fontElement);
            }

            if (kvp.Value is Font font)
            {
                return new FontResourceViewModel(kvp.Key, font);
            }

            if (kvp.Value is Color color)
            {
                return new ColorResourceViewModel(kvp.Key, color);
            }

            return new ObjectResourceViewModel(kvp);
        }

        public ColorResourceViewModel[] Colors
        {
            get => this.colors;
            private set => this.SetProperty(ref this.colors, value);
        }

        public FontResourceViewModel[] Fonts
        {
            get => this.fonts;
            private set => this.SetProperty(ref this.fonts, value);
        }

        public ObjectResourceViewModel[] Resources
        {
            get => this.resources;
            private set => this.SetProperty(ref this.resources, value);
        }

        public IRelayCommand UpdateColorsCommand
        {
            get => this.updateColorsCommand ??= new RelayCommand(this.UpdateColors);
        }

        private void UpdateColors()
        {
            // this.themeHelper.Colors.Primary = MaterialColors.Red500;
            // this.themeHelper.Colors.Secondary = MaterialColors.BlueGray500;
            // this.themeHelper.Colors.Tertiary = MaterialColors.Green500;

            // SuperdevMaui.ColorResources["Theme.SwipeView.Destructive"] = MaterialColors.Yellow500;
        }

        public IAsyncRelayCommand DumpResourcesCommand
        {
            get => this.dumpResourcesCommand ??= new AsyncRelayCommand(this.DumpResourcesAsync);
        }

        private async Task DumpResourcesAsync()
        {
            try
            {
                var stringBuilder = new StringBuilder();
                foreach (var resourceViewModel in this.Resources)
                {
                    stringBuilder.AppendLine($"{resourceViewModel.ResourceType};{resourceViewModel.Key};{resourceViewModel.Value}");
                }

                var resourcesDump = stringBuilder.ToString();

                await this.clipboard.SetTextAsync(resourcesDump);
                await this.email.ComposeAsync("Dump Resources", resourcesDump);
            }
            catch (Exception ex)
            {
                await this.dialogService.DisplayAlertAsync("Email Error", $"Failed to send mail: {ex}", "OK");
            }
        }

        public IRelayCommand SwitchThemesCommand
        {
            get => this.switchThemesCommand ??= new RelayCommand(this.OnSwitchThemes);
        }

        private void OnSwitchThemes()
        {
            this.AppTheme = this.AppTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
            this.themeHelper.AppTheme = this.AppTheme;
        }

        public AppTheme AppTheme
        {
            get => this.appTheme;
            private set
            {
                if (this.SetProperty(ref this.appTheme, value))
                {
                    this.themeHelper.AppTheme =  value;
                }
            }
        }
    }
}