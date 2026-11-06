using System.Globalization;
using Superdev.Maui.Localization;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    public class PropertyRuleBuilderTests
    {
        [Fact]
        public async Task ShouldApplyWithMessageAndWithErrorCode_ToTheRuleJustWritten_Only()
        {
            // Arrange
            var viewModel = new TestFormViewModel { UserName = "ab" };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName)
                .NotEmpty()
                .MinimumLength(3).WithMessage("Three characters, please.").WithErrorCode("TooShort")
                .WithValidationBehavior(ValidationBehavior.All);

            // Act
            await validation.ValidateAsync();
            var errors = validation.Fields["UserName"].Errors;

            // Assert - only the length rule carries the override
            errors.Should().ContainSingle();
            errors[0].ErrorCode.Should().Be("TooShort");
            errors[0].Message.Should().Be("Three characters, please.");

            // Blank but not empty: NotEmpty fails, and MinimumLength has a value to judge.
            viewModel.UserName = " ";
            await validation.ValidateAsync();
            var emptyErrors = validation.Fields["UserName"].Errors;
            emptyErrors.Should().HaveCount(2);
            emptyErrors[0].ErrorCode.Should().Be(ValidationErrorCodes.NotEmpty);
            emptyErrors[0].Message.Should().Be("User Name must not be empty.");
        }

        [Fact]
        public void ShouldThrow_WhenARuleLevelVerbComesBeforeAnyRule()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            var builder = validation.Property(() => viewModel.UserName);

            // Act & Assert
            builder.Invoking(b => b.WithMessage("x")).Should().Throw<InvalidOperationException>();
            builder.Invoking(b => b.WithErrorCode("x")).Should().Throw<InvalidOperationException>();
            builder.Invoking(b => b.InGroup("x")).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task ShouldStopAtTheFirstFailingRule_ByDefault()
        {
            // Arrange
            var viewModel = new TestFormViewModel { UserName = "" };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName).NotEmpty().MinimumLength(3).EmailAddress();

            // Act
            await validation.ValidateAsync();

            // Assert
            validation.Fields["UserName"].Errors.Should().ContainSingle().Which.ErrorCode.Should().Be(ValidationErrorCodes.NotEmpty);
        }

        [Fact]
        public async Task ShouldReportEveryFailingRule_WhenBehaviourIsAll()
        {
            // Arrange
            var viewModel = new TestFormViewModel { UserName = "a b" };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName)
                .MinimumLength(5)
                .Matches("^[a-z]+$")
                .WithValidationBehavior(ValidationBehavior.All);

            // Act
            await validation.ValidateAsync();

            // Assert
            validation.Fields["UserName"].Errors.Select(e => e.ErrorCode).Should().Equal(ValidationErrorCodes.MinimumLength, ValidationErrorCodes.Matches);
        }

        [Fact]
        public async Task ShouldGateTheWholeChain_WithWhenAndUnless()
        {
            // Arrange
            var viewModel = new TestFormViewModel { DiscountPercent = 0 };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.DiscountPercent).NotNull().Between(1, 100).When(() => viewModel.HasDiscount);
            validation.Property(() => viewModel.UserName).NotEmpty().Unless(() => viewModel.HasDiscount);

            // Act & Assert - no discount: the discount rules are off, the user name rule is on
            await validation.ValidateAsync();
            validation.Errors["DiscountPercent"].Should().BeEmpty();
            validation.Errors["UserName"].Should().ContainSingle();

            viewModel.HasDiscount = true;
            await validation.ValidateAsync();
            validation.Errors["DiscountPercent"].Should().ContainSingle();
            validation.Errors["UserName"].Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldResolveALazyMessage_WhenTheRuleRuns_AndAnEagerOneAtSetup()
        {
            // Arrange
            var wording = "first";
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName).NotEmpty().WithMessage(() => wording);
            validation.Property(() => viewModel.Email).NotEmpty().WithMessage(wording);
            wording = "second";

            // Act
            await validation.ValidateAsync();

            // Assert
            validation.Errors["UserName"].Should().Equal("second");
            validation.Errors["Email"].Should().Equal("first");
        }

        [Fact]
        public async Task ShouldNameTheValueInAMessage_WhenAsked()
        {
            // Arrange
            var viewModel = new TestFormViewModel { UserName = "x" };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName).MinimumLength(3).WithMessage(value => $"'{value}' is too short.");

            // Act
            await validation.ValidateAsync();

            // Assert
            validation.Errors["UserName"].Should().Equal("'x' is too short.");
        }

        [Fact]
        public async Task ShouldNameTheProperty_ThroughTheMessageProvider()
        {
            // Arrange
            var translations = new Mock<ITranslationProvider>();
            translations.Setup(t => t.Translate(It.IsAny<string>(), It.IsAny<CultureInfo?>())).Returns<string, CultureInfo?>((key, _) => $"#{key}#");
            translations.Setup(t => t.Translate("ValidationField_UserName", It.IsAny<CultureInfo?>())).Returns("Benutzername");
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation { MessageProvider = new DefaultValidationMessageProvider(translations.Object) };
            validation.Property(() => viewModel.UserName).NotEmpty();

            // Act
            await validation.ValidateAsync();

            // Assert
            validation.Errors["UserName"].Should().Equal("Benutzername must not be empty.");
        }

        [Fact]
        public async Task ShouldUseTheDisplayName_InMessages()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName).NotEmpty().WithDisplayName("Login");

            // Act
            await validation.ValidateAsync();

            // Assert
            validation.Errors["UserName"].Should().Equal("Login must not be empty.");
        }

        [Fact]
        public void ShouldDeriveThePropertyName_FromTheAccessor_OrTakeItExplicitly()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();

            // Act
            var derived = validation.Property(() => viewModel.UserName);
            var explicitName = validation.Property("Login", () => viewModel.UserName);

            // Assert
            derived.PropertyName.Should().Be("UserName");
            explicitName.PropertyName.Should().Be("Login");
        }

        [Fact]
        public void ShouldRejectAnAccessor_ThatIsNotAPlainPropertyAccess()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();

            // Act
            var act = () => validation.Property(() => viewModel.UserName?.Trim());

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Property(nameof(this.UserName), () => this.UserName)*");
        }

        [Fact]
        public void ShouldRegisterAliases_ForRoutingSetErrors()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.Email).NotEmpty().Alias("DuplicateEmail");

            // Act
            validation.SetErrors(new Dictionary<string, string[]> { ["duplicateemail"] = new[] { "taken" } });

            // Assert - aliases are matched case-insensitively
            validation.Errors["Email"].Should().Equal("taken");
        }

        [Fact]
        public async Task ShouldPutRulesIntoSeveralGroups()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.AcceptsTerms).EqualTo(true).InGroups("Submit", "Create");

            // Act & Assert
            (await validation.ValidateAsync()).Should().BeTrue("the rule is grouped and no group is selected");
            (await validation.ValidateAsync(groups: new[] { "Create" })).Should().BeFalse();
            (await validation.ValidateAsync(groups: new[] { "Submit" })).Should().BeFalse();
        }

        [Fact]
        public void ShouldRejectBadArguments()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            var builder = validation.Property(() => viewModel.UserName);

            // Act & Assert
            builder.Invoking(b => b.Add("", _ => true)).Should().Throw<ArgumentException>();
            builder.Invoking(b => b.AddAsync("x", (_, _) => new ValueTask<bool>(true), TimeSpan.FromSeconds(-1))).Should().Throw<ArgumentOutOfRangeException>();
            builder.Invoking(b => b.When(null!)).Should().Throw<ArgumentNullException>();
            builder.Invoking(b => b.Alias("")).Should().Throw<ArgumentException>();
        }
    }
}
