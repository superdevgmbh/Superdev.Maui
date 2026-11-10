using Superdev.Maui.Controls;
using Superdev.Maui.Tests.Validation;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Controls
{
    public class ValidationErrorLabelTests
    {
        [Fact]
        public async Task ShouldShowTheFirstMessage_AndHideItselfWithoutOne()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty().MinimumLength(3);
                return v;
            });
            var label = new ValidationErrorLabel { BindingContext = viewModel, PropertyName = "UserName" };
            label.IsVisible.Should().BeFalse();

            // Act
            await viewModel.Validation.ValidateAsync();

            // Assert
            label.Text.Should().Be("User Name must not be empty.");
            label.IsVisible.Should().BeTrue();

            viewModel.UserName = "superdev";
            label.Text.Should().BeNull();
            label.IsVisible.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldFollowAnExplicitSource()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var source = new ViewModelValidation();
            source.Property(nameof(viewModel.Email), () => viewModel.Email).EmailAddress();
            viewModel.Email = "not-an-address";
            var label = new ValidationErrorLabel { PropertyName = "Email", Source = source };

            // Act
            await source.ValidateAsync();

            // Assert
            label.Text.Should().Be("Email is not a valid e-mail address.");
        }
    }
}
