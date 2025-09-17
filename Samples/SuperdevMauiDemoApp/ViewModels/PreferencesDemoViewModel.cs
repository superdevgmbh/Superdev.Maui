using System.Windows.Input;
using Superdev.Maui.Extensions;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using Superdev.Maui.Services.Settings;
using IPreferences = Superdev.Maui.Services.IPreferences;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class PreferencesDemoViewModel : BaseViewModel
    {
        private readonly SettingsProperty<EnvironmentSetting> currentEnvironment;
        private readonly IPreferences preferences;
        private readonly IToastService toastService;

        private ICommand getCommand;
        private ICommand setCommand;
        private ICommand clearCommand;

        public PreferencesDemoViewModel(
            IPreferences preferences,
            IToastService toastService)
        {
            this.preferences = preferences;
            this.toastService = toastService;

            var defaultEnvironmentSetting = new EnvironmentSetting
            {
                EnvironmentName = "Test",
                BaseUrl = "https://localhost/api"
            };

            this.currentEnvironment = new SettingsProperty<EnvironmentSetting>(preferences, nameof(this.CurrentEnvironment), defaultEnvironmentSetting);

            _ = this.LoadData();
        }

        private async Task LoadData()
        {
            await Task.Delay(3000);

            this.IsInitialized = true;
        }

        public EnvironmentSetting CurrentEnvironment
        {
            get => this.currentEnvironment.Value;
            set
            {
                if (value != null)
                {
                    this.currentEnvironment.Value = value;
                    this.RaisePropertyChanged(nameof(this.CurrentEnvironment));
                }
            }
        }

        public ICommand GetCommand
        {
            get => this.getCommand ??= new Command(this.GetPreferencesValue);
        }

        private void GetPreferencesValue()
        {
            var timespan = this.preferences.Get<TimeSpan>("TimeSpan", default);

            this.toastService.ShortAlert($"GetPreferencesValue returned TimeSpan={timespan}");

            this.RaisePropertyChanged();
        }

        public ICommand SetCommand
        {
            get => this.setCommand ??= new Command(this.SetPreferencesValue);
        }

        private void SetPreferencesValue()
        {
            this.preferences.Set("TimeSpan", new TimeSpan(1, 2, 3));

            this.CurrentEnvironment = new EnvironmentSetting
            {
                EnvironmentName = "Updated",
                BaseUrl = "https://localhost/api/updated",
            };
        }

        public ICommand ClearCommand
        {
            get => this.clearCommand ??= new Command(this.ClearPreferencesValues);
        }

        private void ClearPreferencesValues()
        {
            this.preferences.Clear();

            this.RaisePropertyChanged();
        }
    }
}
