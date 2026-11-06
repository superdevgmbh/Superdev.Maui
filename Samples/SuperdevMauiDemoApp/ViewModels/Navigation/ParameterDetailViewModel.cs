using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    /// <summary>
    /// Receives a typed navigation parameter.
    /// </summary>
    public class ParameterDetailViewModel : BaseViewModel, IInitialize<ParameterDetailViewModel.NavigationParameter>
    {
        private string? parameterText;

        public string? ParameterText
        {
            get => this.parameterText;
            private set => this.SetProperty(ref this.parameterText, value);
        }

        public Task InitializeAsync(NavigationParameter parameter)
        {
            this.Title = parameter.Title;
            this.ParameterText = $"Title: {parameter.Title}{Environment.NewLine}Value: {parameter.Value}";
            return Task.CompletedTask;
        }

        /// <summary>
        /// The navigation parameter of this viewmodel.
        /// </summary>
        public class NavigationParameter
        {
            public required string Title { get; init; }

            public required int Value { get; init; }
        }
    }
}
