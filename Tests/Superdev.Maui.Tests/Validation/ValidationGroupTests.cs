using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    public class ValidationGroupTests
    {
        [Fact]
        public async Task ShouldAlwaysRunUngroupedRules_AndGroupedRulesOnlyWhenSelected()
        {
            // Arrange
            var viewModel = new TestFormViewModel { UserName = new string('x', 81) };
            var validation = CreateForm(viewModel);

            // Act & Assert
            await validation.ValidateAsync();
            validation.Errors["UserName"].Should().ContainSingle("the length rule is ungrouped");
            validation.Errors["Email"].Should().BeEmpty("the group was not selected");

            await validation.ValidateAsync(groups: new[] { "Submit" });
            validation.Errors["Email"].Should().ContainSingle();
        }

        [Fact]
        public async Task ShouldTreatTheBlockAndInGroup_Alike()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = CreateForm(viewModel);

            // Act
            await validation.ValidateAsync(groups: new[] { "Submit" });

            // Assert - Email came from the block, UserName's NotEmpty from .InGroup
            validation.Errors["Email"].Should().ContainSingle();
            validation.Errors["UserName"].Should().ContainSingle();
        }

        [Fact]
        public async Task ShouldRunARuleInSeveralGroups_WhenAnyIsSelected()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = CreateForm(viewModel);

            // Act & Assert
            await validation.ValidateAsync(groups: new[] { "Create" });
            validation.Errors["AcceptsTerms"].Should().ContainSingle();
            validation.Errors["Email"].Should().BeEmpty("Email is only in Submit");
        }

        [Fact]
        public async Task ShouldMakeTheSelectedGroupsActive_ForReValidation()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = CreateForm(viewModel);
            await validation.ValidateAsync(groups: new[] { "Submit" });
            validation.Errors["Email"].Should().ContainSingle();

            // Act - the user fixes the field; the re-validation runs under the same scenario
            viewModel.Email = "a@b";
            await validation.HandlePropertyChangeAsync(nameof(viewModel.Email));

            // Assert
            validation.Errors["Email"].Should().BeEmpty();
            validation.ActiveGroups.Should().BeEquivalentTo(new[] { "Submit" });
        }

        [Fact]
        public async Task ShouldDropTheErrorsOfDeselectedGroups()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = CreateForm(viewModel);
            await validation.ValidateAsync(groups: new[] { "Submit", "Create" });
            validation.Errors["AcceptsTerms"].Should().ContainSingle();

            // Act
            validation.ActiveGroups = new HashSet<string> { "Submit" };

            // Assert
            validation.Errors["AcceptsTerms"].Should().ContainSingle("AcceptsTerms is in Submit too");
            validation.ActiveGroups = new HashSet<string>();
            validation.Errors["AcceptsTerms"].Should().BeEmpty();
            validation.Errors["Email"].Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldPassTheGroups_ThroughInclude()
        {
            // Arrange
            var child = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.Email).NotEmpty().InGroup("Submit");
                return v;
            });
            var parent = new ViewModelValidation();
            parent.Include("Child", () => child.Validation);

            // Act & Assert
            (await parent.ValidateAsync()).Should().BeTrue();
            (await parent.ValidateAsync(groups: new[] { "Submit" })).Should().BeFalse();
            child.Validation.ActiveGroups.Should().BeEquivalentTo(new[] { "Submit" });
        }

        [Fact]
        public async Task ShouldRunAChildsGroupedRules_WithTheParentsActiveGroups()
        {
            // Arrange
            var child = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.Email).NotEmpty().InGroup("Submit");
                return v;
            });
            var parent = new ViewModelValidation();
            parent.Include("Child", () => child.Validation);

            // Act - the scenario is known up front, as a view model knows it in InitializeAsync
            parent.ActiveGroups = new HashSet<string> { "Submit" };

            // Assert
            parent.IsValid.Should().BeFalse("the child's Submit rule counts once the parent selects Submit");
            (await parent.ValidateAsync()).Should().BeFalse();
            child.Validation.Errors["Email"].Should().ContainSingle();
        }

        [Fact]
        public async Task ShouldDropAChildsGroupedErrors_WhenTheParentDeselectsTheGroup()
        {
            // Arrange
            var child = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.Email).NotEmpty().InGroup("Submit");
                return v;
            });
            var parent = new ViewModelValidation();
            parent.Include("Child", () => child.Validation);
            await parent.ValidateAsync(groups: new[] { "Submit" });
            child.Validation.Errors["Email"].Should().ContainSingle();

            // Act
            parent.ActiveGroups = new HashSet<string>();

            // Assert
            child.Validation.Errors["Email"].Should().BeEmpty();
            parent.HasErrors.Should().BeFalse();
        }

        [Fact]
        public void ShouldDescribeTheGroups_AndTheirProperties()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = CreateForm(viewModel);

            // Act
            var groups = validation.Groups;

            // Assert
            groups.Select(g => g.Name).Should().BeEquivalentTo(new[] { "Submit", "Create" });
            groups.Single(g => g.Name == "Submit").PropertyNames.Should().BeEquivalentTo(new[] { "UserName", "Email", "AcceptsTerms" });
            groups.Single(g => g.Name == "Create").PropertyNames.Should().Equal("AcceptsTerms");
        }

        private static ViewModelValidation CreateForm(TestFormViewModel viewModel)
        {
            var v = new ViewModelValidation();

            v.Property(() => viewModel.UserName)
                .MaximumLength(80)
                .NotEmpty().InGroup("Submit");

            v.Group("Submit", g =>
            {
                g.Property(() => viewModel.Email).NotEmpty();
            });

            v.Property(() => viewModel.AcceptsTerms).EqualTo(true).InGroups("Submit", "Create");

            return v;
        }
    }
}
