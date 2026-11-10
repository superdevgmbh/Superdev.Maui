using Microsoft.Extensions.Time.Testing;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    public class PropertyRuleBuilderExtensionsTests
    {
        #region A missing value passes everything but NotNull and NotEmpty

        public static TheoryData<string, Func<PropertyRuleBuilder<string?>, PropertyRuleBuilder<string?>>> TextRules => new()
        {
            { "MinimumLength", b => b.MinimumLength(3) },
            { "MaximumLength", b => b.MaximumLength(3) },
            { "Length", b => b.Length(2, 4) },
            { "Matches", b => b.Matches("^[a-z]+$") },
            { "EmailAddress", b => b.EmailAddress() },
            { "IsInteger", b => b.IsInteger() },
            { "IsDecimal", b => b.IsDecimal() },
            { "IsDate", b => b.IsDate() },
            { "IsTime", b => b.IsTime() },
            { "GreaterThan", b => b.GreaterThan("m") },
            { "OneOf", b => b.OneOf("a", "b") },
        };

        [Theory]
        [MemberData(nameof(TextRules))]
        public async Task ShouldPassAMissingValue(string rule, Func<PropertyRuleBuilder<string?>, PropertyRuleBuilder<string?>> configure)
        {
            // Arrange
            var viewModel = new TestFormViewModel { UserName = null };
            var validation = new ViewModelValidation();
            configure(validation.Property(() => viewModel.UserName));

            // Act
            var isValid = await validation.ValidateAsync();

            // Assert
            isValid.Should().BeTrue($"{rule} does not require a value; NotEmpty does");
        }

        [Theory]
        [MemberData(nameof(TextRules))]
        public async Task ShouldPassAnEmptyValue(string rule, Func<PropertyRuleBuilder<string?>, PropertyRuleBuilder<string?>> configure)
        {
            // Arrange - a field typed into and cleared again holds empty text, not null
            var viewModel = new TestFormViewModel { UserName = string.Empty };
            var validation = new ViewModelValidation();
            configure(validation.Property(() => viewModel.UserName));

            // Act
            var isValid = await validation.ValidateAsync();

            // Assert
            isValid.Should().BeTrue($"{rule} does not require a value; NotEmpty does");
        }

        [Fact]
        public async Task ShouldRequireAValue_WithNotEmptyAndNotNull()
        {
            // Arrange
            var viewModel = new TestFormViewModel { UserName = " ", Birthdate = null };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName).NotEmpty();
            validation.Property(() => viewModel.Birthdate).NotNull();

            // Act
            await validation.ValidateAsync();

            // Assert
            validation.Errors["UserName"].Should().Equal("User Name must not be empty.");
            validation.Errors["Birthdate"].Should().Equal("Birthdate must not be empty.");
        }

        [Fact]
        public async Task ShouldRequireAnEntry_WithNotEmptyOnACollection()
        {
            // Arrange
            var items = new List<int>();
            var validation = new ViewModelValidation();
            validation.Property("Items", () => items).NotEmpty();

            // Act & Assert
            (await validation.ValidateAsync()).Should().BeFalse();
            items.Add(1);
            (await validation.ValidateAsync()).Should().BeTrue();
        }

        #endregion

        #region Text

        [Theory]
        [InlineData("ab", false)]
        [InlineData("abc", true)]
        [InlineData("abcdef", false)]
        public async Task ShouldCheckTheLength(string value, bool expected)
        {
            // Arrange
            var viewModel = new TestFormViewModel { UserName = value };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName).MinimumLength(3).MaximumLength(5);

            // Act & Assert
            (await validation.ValidateAsync()).Should().Be(expected);
        }

        [Fact]
        public async Task ShouldTellTheLengthInTheMessage()
        {
            // Arrange
            var viewModel = new TestFormViewModel { UserName = "ab" };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName).MinimumLength(3);

            // Act
            await validation.ValidateAsync();

            // Assert
            validation.Errors["UserName"].Should().Equal("User Name must be at least 3 characters long.");
            validation.Fields["UserName"].Errors[0].Arguments["MinimumLength"].Should().Be(3);
        }

        [Theory]
        [InlineData("1234", true)]
        [InlineData("123", false)]
        public async Task ShouldCheckAnExactLength(string value, bool expected)
        {
            // Arrange
            var viewModel = new TestFormViewModel { UserName = value };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName).Length(4);

            // Act & Assert
            (await validation.ValidateAsync()).Should().Be(expected);
        }

        [Theory]
        [InlineData("abc", true)]
        [InlineData("ABC", false)]
        [InlineData("a b", false)]
        public async Task ShouldMatchAPattern(string value, bool expected)
        {
            // Arrange
            var viewModel = new TestFormViewModel { UserName = value };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName).Matches("^[a-z]+$");

            // Act & Assert
            (await validation.ValidateAsync()).Should().Be(expected);
        }

        [Theory]
        [InlineData("thomas@superdev.ch", true)]
        [InlineData("a@b", true)]
        [InlineData("thomas", false)]
        [InlineData("@superdev.ch", false)]
        [InlineData("thomas@", false)]
        [InlineData("thomas @superdev.ch", false)]
        [InlineData("thomas@@superdev.ch", false)]
        public async Task ShouldCheckTheShapeOfAnEmailAddress(string value, bool expected)
        {
            // Arrange
            var viewModel = new TestFormViewModel { Email = value };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.Email).EmailAddress();

            // Act & Assert
            (await validation.ValidateAsync()).Should().Be(expected);
        }

        #endregion

        #region Equality

        [Fact]
        public async Task ShouldCompareWithAConstant_AndNameItInTheMessage()
        {
            // Arrange
            var viewModel = new TestFormViewModel { AcceptsTerms = false };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.AcceptsTerms).EqualTo(true);

            // Act
            await validation.ValidateAsync();

            // Assert
            validation.Errors["AcceptsTerms"].Should().Equal("Accepts Terms must be equal to True.");
        }

        [Fact]
        public async Task ShouldCompareWithAnotherProperty_AndNameItRatherThanItsValue()
        {
            // Arrange
            var viewModel = new TestFormViewModel { Password = "secret", PasswordConfirm = "other" };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.PasswordConfirm).EqualTo(() => viewModel.Password);

            // Act
            await validation.ValidateAsync();

            // Assert - the secret is not in the message
            validation.Errors["PasswordConfirm"].Should().Equal("Password Confirm must match Password.");
        }

        [Theory]
        [InlineData(null, "secret")]
        [InlineData("", "secret")]
        [InlineData("secret", null)]
        [InlineData(null, "")]
        public async Task ShouldPassWhileEitherSideIsMissing_WhenComparingWithAnotherProperty(string? confirm, string? password)
        {
            // Arrange
            var viewModel = new TestFormViewModel { Password = password, PasswordConfirm = confirm };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.PasswordConfirm).EqualTo(() => viewModel.Password);

            // Act
            var isValid = await validation.ValidateAsync();

            // Assert
            isValid.Should().BeTrue("presence is NotEmpty's question, not EqualTo's");
        }

        [Fact]
        public async Task ShouldNameTheOtherProperty_ByItsDisplayName()
        {
            // Arrange
            var viewModel = new TestFormViewModel { Password = "secret", PasswordConfirm = "other" };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.Password).NotEmpty().WithDisplayName("Kennwort");
            validation.Property(() => viewModel.PasswordConfirm).EqualTo(() => viewModel.Password);

            // Act
            await validation.ValidateAsync();

            // Assert
            validation.Errors["PasswordConfirm"].Should().Equal("Password Confirm must match Kennwort.");
        }

        [Fact]
        public async Task ShouldRequireTrue_WithIsTrue()
        {
            // Arrange
            var viewModel = new TestFormViewModel { AcceptsTerms = false };
            bool? consent = null;
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.AcceptsTerms).IsTrue();
            validation.Property("Consent", () => consent).IsTrue();

            // Act
            await validation.ValidateAsync();

            // Assert
            validation.Errors["AcceptsTerms"].Should().Equal("Accepts Terms is required.");
            validation.Errors["Consent"].Should().ContainSingle("an unanswered question is not a yes");

            viewModel.AcceptsTerms = true;
            consent = true;
            (await validation.ValidateAsync()).Should().BeTrue();
        }

        [Fact]
        public async Task ShouldRequireADifference_WithNotEqualTo()
        {
            // Arrange
            var viewModel = new TestFormViewModel { Password = "same", PasswordConfirm = "same" };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.PasswordConfirm).NotEqualTo(() => viewModel.Password);
            validation.Property(() => viewModel.UserName).NotEqualTo("admin");

            // Act
            viewModel.UserName = "admin";
            await validation.ValidateAsync();

            // Assert
            validation.Errors["PasswordConfirm"].Should().ContainSingle();
            validation.Errors["UserName"].Should().Equal("User Name must not be equal to admin.");
        }

        #endregion

        #region Comparison

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(100, true)]
        [InlineData(101, false)]
        public async Task ShouldCheckARange(int value, bool expected)
        {
            // Arrange
            var viewModel = new TestFormViewModel { DiscountPercent = value };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.DiscountPercent).Between(1, 100);

            // Act & Assert
            (await validation.ValidateAsync()).Should().Be(expected);
        }

        [Fact]
        public async Task ShouldCompare_OnNullableAndReferenceTypes()
        {
            // Arrange
            var viewModel = new TestFormViewModel { Amount = 5m, UserName = "b" };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.Amount).GreaterThan(0m).LessThanOrEqualTo(10m);
            validation.Property(() => viewModel.UserName).GreaterThanOrEqualTo("a").LessThan("c");

            // Act & Assert
            (await validation.ValidateAsync()).Should().BeTrue();

            viewModel.Amount = 11m;
            await validation.ValidateAsync();
            validation.Errors["Amount"].Should().Equal("Amount must be less than or equal to 10.");
        }

        [Fact]
        public void ShouldRejectAComparisonRule_OnAValueThatCannotBeOrdered()
        {
            // Arrange
            var validation = new ViewModelValidation();
            var builder = validation.Property("Object", () => new object());

            // Act & Assert
            builder.Invoking(b => b.GreaterThan(new object())).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ShouldRejectAnEmptyRange_ForBetween()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            var builder = validation.Property(() => viewModel.DiscountPercent);

            // Act & Assert
            builder.Invoking(b => b.Between(100, 1)).Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task ShouldCheckMembership_WithOneOf()
        {
            // Arrange
            var viewModel = new TestFormViewModel { UserName = "c" };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName).OneOf("a", "b");

            // Act
            await validation.ValidateAsync();

            // Assert
            validation.Errors["UserName"].Should().Equal("User Name must be one of: a, b.");
        }

        #endregion

        #region Dates

        [Fact]
        public async Task ShouldJudgePastAndFuture_ByTheFormsClock()
        {
            // Arrange
            var time = new FakeTimeProvider(new DateTimeOffset(2026, 9, 22, 12, 0, 0, TimeSpan.Zero));
            var viewModel = new TestFormViewModel { Birthdate = new DateTime(2026, 9, 23, 0, 0, 0, DateTimeKind.Utc) };
            var validation = new ViewModelValidation { TimeProvider = time };
            validation.Property(() => viewModel.Birthdate).InThePast();

            // Act & Assert
            (await validation.ValidateAsync()).Should().BeFalse("tomorrow is not in the past");
            validation.Errors["Birthdate"].Should().Equal("Birthdate must be in the past.");

            time.Advance(TimeSpan.FromDays(2));
            (await validation.ValidateAsync()).Should().BeTrue();
        }

        [Fact]
        public async Task ShouldJudgeADateOnly_ByTheFormsClock()
        {
            // Arrange
            var time = new FakeTimeProvider(new DateTimeOffset(2026, 9, 22, 12, 0, 0, TimeSpan.Zero));
            time.SetLocalTimeZone(TimeZoneInfo.Utc);
            var appointment = new DateOnly(2026, 9, 22);
            var validation = new ViewModelValidation { TimeProvider = time };
            validation.Property("Appointment", () => appointment).InTheFuture();

            // Act & Assert
            (await validation.ValidateAsync()).Should().BeFalse("today is not in the future");
            appointment = new DateOnly(2026, 9, 23);
            (await validation.ValidateAsync()).Should().BeTrue();
        }

        #endregion

        #region Text that has to parse

        [Theory]
        [InlineData("de-DE", "1,5", true)]
        [InlineData("de-DE", "1.5", false)]
        [InlineData("en-US", "1.5", true)]
        [InlineData("en-US", "1,5", false)]
        [InlineData("en-US", "abc", false)]
        public async Task ShouldParseADecimal_InTheCurrentCulture(string culture, string value, bool expected)
        {
            using var _ = new CultureScope(culture);

            // Arrange
            var viewModel = new TestFormViewModel { Age = value };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.Age).IsDecimal();

            // Act & Assert
            (await validation.ValidateAsync()).Should().Be(expected);
        }

        [Theory]
        [InlineData("42", true)]
        [InlineData("-7", true)]
        [InlineData("4.2", false)]
        [InlineData("forty", false)]
        public async Task ShouldParseAnInteger(string value, bool expected)
        {
            using var _ = new CultureScope("en-US");

            // Arrange
            var viewModel = new TestFormViewModel { Age = value };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.Age).IsInteger();

            // Act & Assert
            (await validation.ValidateAsync()).Should().Be(expected);
            if (!expected)
            {
                validation.Errors["Age"].Should().Equal("Age must be a whole number.");
            }
        }

        [Theory]
        [InlineData("de-DE", "24.12.2026", true)]
        [InlineData("en-US", "12/24/2026", true)]
        [InlineData("en-US", "24/12/2026", false)]
        public async Task ShouldParseADate_InTheCurrentCulture(string culture, string value, bool expected)
        {
            using var _ = new CultureScope(culture);

            // Arrange
            var viewModel = new TestFormViewModel { Age = value };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.Age).IsDate();

            // Act & Assert
            (await validation.ValidateAsync()).Should().Be(expected);
        }

        [Theory]
        [InlineData("13:45", true)]
        [InlineData("25:00", false)]
        public async Task ShouldParseATime(string value, bool expected)
        {
            using var _ = new CultureScope("de-DE");

            // Arrange
            var viewModel = new TestFormViewModel { Age = value };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.Age).IsTime();

            // Act & Assert
            (await validation.ValidateAsync()).Should().Be(expected);
        }

        #endregion

        #region Custom

        [Fact]
        public async Task ShouldRunAOneOffRule_WithMust()
        {
            // Arrange
            var viewModel = new TestFormViewModel { Email = "someone@hotmail.ru" };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.Email).Must(e => e == null || !e.EndsWith("@hotmail.ru"));

            // Act
            await validation.ValidateAsync();

            // Assert
            validation.Errors["Email"].Should().Equal("Email is not valid.");
            validation.Fields["Email"].Errors[0].ErrorCode.Should().Be(ValidationErrorCodes.Must);
        }

        [Fact]
        public async Task ShouldAwaitAOneOffRule_WithMustAsync_OrTheValueTaskPrimitive()
        {
            // Arrange
            var viewModel = new TestFormViewModel { UserName = "taken" };
            var validation = new ViewModelValidation();
            validation.Property(() => viewModel.UserName)
                .MustAsync((value, _) => Task.FromResult(value != "taken"))
                .AddAsync(ValidationErrorCodes.MustAsync, (value, _) => new ValueTask<bool>(value != "reserved"));

            // Act & Assert
            (await validation.ValidateAsync()).Should().BeFalse();
            validation.Fields["UserName"].Errors[0].ErrorCode.Should().Be(ValidationErrorCodes.MustAsync);

            viewModel.UserName = "free";
            (await validation.ValidateAsync()).Should().BeTrue();
        }

        #endregion
    }
}
