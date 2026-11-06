using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Validation;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class EditorDemoViewModel : BaseViewModel
    {
        private readonly IViewModelErrorHandler viewModelErrorHandler;

        private string? notes;
        private string? logContent;
        private bool hasKeyboardOffset = true;
        private IRelayCommand? toggleHasKeyboardOffsetCommand;

        public EditorDemoViewModel(
            IViewModelErrorHandler viewModelErrorHandler)
        {
            this.viewModelErrorHandler = viewModelErrorHandler;
        }

        protected override ViewModelValidation SetupValidation()
        {
            // No validate button on this page: fields validate as they change.
            var viewModelValidation = new ViewModelValidation { Mode = ValidationMode.OnChange };

            // The editor's MaxLength already stops input at NotesMaxLength, so a length rule could never fail here.
            viewModelValidation.Property(() => this.Notes)
                .NotEmpty();

            return viewModelValidation;
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
                await Task.Delay(1000);

                this.LogContent = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.";
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        public IRelayCommand ToggleHasKeyboardOffsetCommand
        {
            get => this.toggleHasKeyboardOffsetCommand ??= new RelayCommand(this.ToggleHasKeyboardOffset);
        }

        private void ToggleHasKeyboardOffset()
        {
            this.HasKeyboardOffset = !this.HasKeyboardOffset;
        }

        public bool HasKeyboardOffset
        {
            get => this.hasKeyboardOffset;
            set => this.SetProperty(ref this.hasKeyboardOffset, value);
        }

        public string? Notes
        {
            get => this.notes;
            set => this.SetProperty(ref this.notes, value);
        }

        public string? LogContent
        {
            get => this.logContent;
            set => this.SetProperty(ref this.logContent, value);
        }

        public int NotesMaxLength { get; set; } = 100;
    }
}