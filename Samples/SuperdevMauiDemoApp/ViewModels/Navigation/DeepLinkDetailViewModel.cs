using Superdev.Maui.Mvvm;
using Superdev.Maui.Navigation;

namespace SuperdevMauiDemoApp.ViewModels.Navigation
{
    /// <summary>
    /// Receives the query parameters of a deep link.
    /// </summary>
    public class DeepLinkDetailViewModel : BaseViewModel, IInitialize<NavigationQueryParameters>
    {
        private string parameterText = "(opened without query parameters)";

        public DeepLinkDetailViewModel()
        {
            this.Title = "Deep link target";
        }

        public string ParameterText
        {
            get => this.parameterText;
            private set => this.SetProperty(ref this.parameterText, value);
        }

        public Task InitializeAsync(NavigationQueryParameters parameter)
        {
            parameter.TryGetValue<int>("id", out var id);
            this.ParameterText =
                $"Query string: {parameter}{Environment.NewLine}Parsed id: {id}";
            return Task.CompletedTask;
        }
    }
}
