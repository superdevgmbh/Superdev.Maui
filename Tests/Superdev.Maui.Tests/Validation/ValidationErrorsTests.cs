using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    public class ValidationErrorsTests
    {
        [Fact]
        public async Task ShouldExposeMessagesByProperty_ForTheForm_AndFlattened()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var child = new AddressViewModel();
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName).NotEmpty();
            validation.Include("Address", () => child.Validation);
            await validation.ValidateAsync();
            validation.SetErrors(new Dictionary<string, string[]> { ["InvalidToken"] = new[] { "expired" } });

            // Act
            var errors = validation.Errors;

            // Assert
            errors["UserName"].Should().Equal("User Name must not be empty.");
            errors["Unknown"].Should().BeEmpty();
            errors[null!].Should().BeEmpty();
            errors.Form.Should().Equal("expired");
            errors.All.Should().HaveCount(4, "one own, one form-level, two from the child");
            errors.Count.Should().Be(4);
            errors.Select(e => e.Key).Should().Equal("UserName", "", "Address.Street", "Address.PostalCode");
        }

        [Fact]
        public async Task ShouldNotifyTheIndexerAndTheViews_WhenErrorsChange()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName).NotEmpty();
            var raised = new List<string?>();
            validation.Errors.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            await validation.ValidateAsync();

            // Assert
            raised.Should().Contain(new[] { "Item[UserName]", "Form", "All", "Count" });
        }

        [Fact]
        public async Task ShouldHandOutSnapshots()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName).NotEmpty();
            await validation.ValidateAsync();
            var before = validation.Errors["UserName"];
            var allBefore = validation.Errors.All;

            // Act
            viewModel.UserName = "superdev";
            await validation.ValidateAsync();

            // Assert
            before.Should().ContainSingle();
            allBefore.Should().ContainSingle();
            validation.Errors["UserName"].Should().BeEmpty();
        }

        [Fact]
        public void ShouldNotifyOneIndexer_WhenOneFieldChanges()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation { Mode = ValidationMode.OnChange };
                v.Property(() => vm.UserName).NotEmpty();
                v.Property(() => vm.Email).NotEmpty();
                return v;
            });
            var validation = viewModel.Validation;
            var raised = new List<string?>();
            validation.Errors.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
            var formRaised = new List<string?>();
            validation.PropertyChanged += (_, e) => formRaised.Add(e.PropertyName);

            // Act
            viewModel.UserName = " ";

            // Assert - a keystroke re-reads one binding, not every Errors[X] on the page
            raised.Should().Contain("Item[UserName]").And.Contain(nameof(ValidationErrors.All));
            raised.Should().NotContain("Item").And.NotContain("Item[Email]");
            formRaised.Should().NotContain(nameof(ViewModelValidation.Errors));
        }

        [Fact]
        public async Task ShouldNotifyEveryIndexer_WhenManyFieldsChange()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                v.Property(() => vm.Email).NotEmpty();
                return v;
            });
            var validation = viewModel.Validation;
            var raised = new List<string?>();
            validation.Errors.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
            var formRaised = new List<string?>();
            validation.PropertyChanged += (_, e) => formRaised.Add(e.PropertyName);

            // Act
            await validation.ValidateAsync();

            // Assert - "Item" refreshes every indexer binding, and Errors every compiled path through it
            raised.Should().Contain("Item");
            formRaised.Should().Contain(nameof(ViewModelValidation.Errors));
        }
    }
}
