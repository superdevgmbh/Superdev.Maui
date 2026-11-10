using Superdev.Maui.Controls;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Resources.Styles;
using Superdev.Maui.Tests.Navigation.TestDoubles;
using Application = Microsoft.Maui.Controls.Application;

namespace Superdev.Maui.Tests.Controls
{
    [Collection(ApplicationCollection.Name)]
    public class ValidatablePickerTests : IDisposable
    {
        private readonly Application? previousApplication;

        public ValidatablePickerTests()
        {
            TestDispatcherProvider.EnsureInstalled();
            this.previousApplication = Application.Current;

            var application = new Application();
            application.Resources.MergedDictionaries.Add(new SuperdevMauiStyles());
            Application.Current = application;
        }

        public void Dispose()
        {
            Application.Current = this.previousApplication;
        }

        [Fact]
        public void ShouldShowANewSelectedItem_WhileReadonly()
        {
            // Arrange - as on the services demo page: read-only, the selection bound one way
            var viewModel = new ThemeViewModel { Theme = AppTheme.Light };
            var picker = new ValidatablePicker { DisplayMemberPath = ".", IsReadonly = true, BindingContext = viewModel };
            picker.SetBinding(ValidatablePicker.ItemsSourceProperty, nameof(ThemeViewModel.Themes));
            picker.SetBinding(ValidatablePicker.SelectedItemProperty, new Binding(nameof(ThemeViewModel.Theme), BindingMode.OneWay));
            picker.ReadonlyLabel.Text.Should().Be("Light");

            // Act
            viewModel.Theme = AppTheme.Dark;

            // Assert
            picker.ReadonlyLabel.Text.Should().Be("Dark");
        }

        [Fact]
        public void ShouldPassAPickToATwoWayBinding_AndStillFollowTheViewModel()
        {
            // Arrange
            var viewModel = new ThemeViewModel { Theme = AppTheme.Light };
            var picker = new ValidatablePicker { DisplayMemberPath = ".", BindingContext = viewModel };
            picker.SetBinding(ValidatablePicker.ItemsSourceProperty, nameof(ThemeViewModel.Themes));
            picker.SetBinding(ValidatablePicker.SelectedItemProperty, new Binding(nameof(ThemeViewModel.Theme), BindingMode.TwoWay));

            // Act - the user picks, then the view model changes the selection itself
            picker.Picker.SelectedItem = AppTheme.Dark;
            viewModel.Theme.Should().Be(AppTheme.Dark);
            viewModel.Theme = AppTheme.Light;

            // Assert
            picker.SelectedItem.Should().Be(AppTheme.Light);
            picker.Picker.SelectedItem.Should().Be(AppTheme.Light);
        }

        private sealed class ThemeViewModel : BindableBase
        {
            private AppTheme theme;

            public AppTheme[] Themes { get; } = Enum.GetValues<AppTheme>();

            public AppTheme Theme
            {
                get => this.theme;
                set => this.SetProperty(ref this.theme, value);
            }
        }
    }
}
