using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    public class FieldStatesTests
    {
        [Fact]
        public void ShouldYieldAnInertField_ForAnUnknownName()
        {
            // Arrange
            var validation = new ViewModelValidation();

            // Act
            var field = validation.Fields["Gender"];

            // Assert - a binding to a name without rules must not break a page
            field.PropertyName.Should().Be("Gender");
            field.HasErrors.Should().BeFalse();
            validation.Fields.Should().BeEmpty("only fields with rules are enumerated");
        }

        [Fact]
        public void ShouldEnumerateTheFieldsWithRules_InDeclarationOrder()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.Email).EmailAddress();
            validation.Property(() => viewModel.UserName).NotEmpty();

            // Act & Assert
            validation.Fields.Select(f => f.PropertyName).Should().Equal("Email", "UserName");
            validation.Fields.Count.Should().Be(2);
        }

        [Fact]
        public void ShouldNotifyCount_WhenAFieldIsDeclared()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            var raised = new List<string?>();
            validation.Fields.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            validation.Property(() => viewModel.UserName).NotEmpty();
            validation.Property(() => viewModel.UserName).MaximumLength(20);

            // Assert - a second chain for the same property is not a new field
            raised.Should().Equal(nameof(FieldStates.Count));
        }

        [Fact]
        public async Task ShouldHandOutAStableState_WhichNotifiesItself()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName).NotEmpty();
            var state = validation.Fields["UserName"];
            var raised = new List<string?>();
            state.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            await validation.ValidateAsync();

            // Assert - bindings hold on to the state, so the state itself announces the change
            validation.Fields["UserName"].Should().BeSameAs(state);
            raised.Should().Contain(nameof(FieldState.Messages)).And.Contain(nameof(FieldState.HasErrors));
        }
    }
}
