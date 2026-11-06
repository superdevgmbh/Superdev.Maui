using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Time.Testing;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    public class ViewModelValidationTests
    {
        #region Validating

        [Fact]
        public async Task ShouldNotReportErrors_AfterThePropertyWasCorrected()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                return v;
            });
            var validation = viewModel.Validation;

            await validation.ValidateAsync();
            validation.HasErrors.Should().BeTrue("the user name is empty");

            // Act
            viewModel.UserName = "superdev";
            var isValid = await validation.ValidateAsync();

            // Assert
            isValid.Should().BeTrue();
            validation.HasErrors.Should().BeFalse();
            validation.Errors["UserName"].Should().BeEmpty();
            validation.GetErrors().Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldValidateAnEmptyForm_AsValid()
        {
            // Arrange
            var validation = new ViewModelValidation();

            // Act
            var isValid = await validation.ValidateAsync();

            // Assert
            isValid.Should().BeTrue();
            validation.HasErrors.Should().BeFalse();
            validation.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldReportAThrowingRule_OnPropertyChange_InsteadOfFaulting()
        {
            // Arrange - the rule reports an error first, so that the property is validated again on change
            var shouldThrow = false;
            var viewModel = new TestFormViewModel();
            var validation = new RecordingValidation();
            validation.Property(nameof(viewModel.UserName), () => viewModel.UserName)
                .Must(_ => !shouldThrow ? false : throw new InvalidOperationException("The rule threw"));
            await validation.ValidateAsync();
            shouldThrow = true;

            // Act - this runs on every property change of a view model, so it must never fault
            var handling = validation.HandlePropertyChangeAsync("UserName");
            await handling;

            // Assert
            handling.IsCompletedSuccessfully.Should().BeTrue();
            validation.Failures.Should().ContainSingle();
            validation.Failures[0].PropertyName.Should().Be("UserName");
            validation.Failures[0].Exception.Should().BeOfType<InvalidOperationException>();
        }

        [Fact]
        public async Task ShouldNotValidateOnPropertyChange_WhileAFullValidationIsRunning()
        {
            // Arrange - a full validation is held inside its rule
            var rule = new ScriptedRule { IsValid = false };
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            validation.Property(nameof(viewModel.UserName), () => viewModel.UserName).MustAsync(rule.EvaluateAsync);
            await validation.ValidateAsync();

            rule.Hold();
            var fullValidation = validation.ValidateAsync();
            rule.CallCount.Should().Be(2);

            // Act
            await validation.HandlePropertyChangeAsync("UserName");

            // Assert - the running validation covers the property; a second, concurrent run would race it
            rule.CallCount.Should().Be(2, "a property change during a full validation is not validated separately");

            rule.Release();
            await fullValidation;
        }

        [Fact]
        public async Task ShouldHandOutSnapshots_NotTheLiveLists()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                return v;
            });
            var validation = viewModel.Validation;
            await validation.ValidateAsync();

            var propertyErrors = validation.Errors["UserName"];
            var allErrors = validation.GetErrors();
            var fieldMessages = validation.Fields["UserName"].Messages;
            var summary = validation.Errors.All;

            // Act - the property becomes valid
            viewModel.UserName = "superdev";
            await validation.ValidateAsync();

            // Assert - what was handed out before is unaffected, a fresh read reflects the change
            propertyErrors.Should().ContainSingle();
            allErrors["UserName"].Should().ContainSingle();
            fieldMessages.Should().ContainSingle();
            summary.Should().ContainSingle();

            validation.Errors["UserName"].Should().BeEmpty();
            validation.Errors.All.Should().BeEmpty();
            validation.GetErrors().Should().BeEmpty();
            validation.HasErrors.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldRaiseChangeNotifications_OnTheContextTheValidationWasStartedOn()
        {
            // Arrange - the rule finishes on a thread pool thread, like an asynchronous rule would
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            validation.Property(nameof(viewModel.UserName), () => viewModel.UserName)
                .MustAsync(async (_, _) =>
                {
                    await Task.Run(() => { }).ConfigureAwait(false);
                    return false;
                });

            var context = new RecordingSynchronizationContext();
            SynchronizationContext? errorsChangedContext = null;
            validation.ErrorsChanged += (_, _) => errorsChangedContext ??= SynchronizationContext.Current;

            var previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                // Act
                await validation.ValidateAsync();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }

            // Assert
            errorsChangedContext.Should().BeSameAs(context,
                "started from the UI thread, the notifications must arrive on the UI thread rather than on the thread pool");
            context.PostCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task ShouldCompleteSynchronously_WhenNoAsyncRuleIsPresent()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty().MinimumLength(3);
                v.Property(() => vm.Email).EmailAddress();
                return v;
            });

            // Act
            var task = viewModel.Validation.ValidateAsync();

            // Assert - no synchronization context, no thread pool: the caller gets the answer at once
            task.IsCompletedSuccessfully.Should().BeTrue();
            (await task).Should().BeFalse();
        }

        [Fact]
        public async Task ShouldValidateOneProperty_OnRequest()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                v.Property(() => vm.Email).NotEmpty();
                return v;
            });

            // Act
            var isValid = await viewModel.Validation.ValidatePropertyAsync(nameof(viewModel.UserName));

            // Assert
            isValid.Should().BeFalse();
            viewModel.Validation.Errors["UserName"].Should().ContainSingle();
            viewModel.Validation.Errors["Email"].Should().BeEmpty("only the requested property was validated");
        }

        #endregion

        #region Trigger policy

        [Fact]
        public async Task ShouldReproduceTodaysBehaviour_WithManualAndOnChange()
        {
            // Arrange - the defaults
            var viewModel = CreateUserNameForm();
            var validation = viewModel.Validation;
            validation.Mode.Should().Be(ValidationMode.Manual);
            validation.ReValidateMode.Should().Be(ReValidationMode.OnChange);

            // Act & Assert - nothing until an explicit validation
            viewModel.UserName = "";
            validation.HasErrors.Should().BeFalse("nothing validates before the form was validated explicitly");

            await validation.ValidateAsync();
            validation.Errors["UserName"].Should().ContainSingle();

            // ... then the failing field re-validates as it is corrected
            viewModel.UserName = "superdev";
            validation.Errors["UserName"].Should().BeEmpty("the error clears the moment the value is right");

            // ... and, once validated, a field that becomes invalid again is flagged live
            viewModel.UserName = "";
            validation.Errors["UserName"].Should().ContainSingle();
        }

        [Theory]
        [InlineData(ValidationMode.OnChange)]
        [InlineData(ValidationMode.All)]
        public void ShouldValidateOnFirstChange_WhenTheModeSaysSo(ValidationMode mode)
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            viewModel.Validation.Mode = mode;

            // Act
            viewModel.UserName = "";

            // Assert
            viewModel.Validation.Errors["UserName"].Should().ContainSingle();
        }

        [Theory]
        [InlineData(ValidationMode.OnBlur)]
        [InlineData(ValidationMode.OnTouched)]
        public void ShouldWaitForTheBlur_BeforeTheFirstValidation(ValidationMode mode)
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            viewModel.Validation.Mode = mode;

            // Act - typing does not nag
            viewModel.UserName = "";
            viewModel.Validation.Errors["UserName"].Should().BeEmpty("the user is still in the field");

            // ... leaving the field does
            viewModel.Validation.MarkTouched(nameof(viewModel.UserName));

            // Assert
            viewModel.Validation.Fields["UserName"].IsTouched.Should().BeTrue();
            viewModel.Validation.Errors["UserName"].Should().ContainSingle();
        }

        [Fact]
        public void ShouldValidateOnEveryChange_OnceTouched_InOnTouchedMode()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            viewModel.Validation.Mode = ValidationMode.OnTouched;
            viewModel.Validation.ReValidateMode = ReValidationMode.Manual;
            viewModel.UserName = "superdev";
            viewModel.Validation.MarkTouched(nameof(viewModel.UserName));
            viewModel.Validation.Errors["UserName"].Should().BeEmpty();

            // Act - a touched field validates on change, whatever the re-validation mode says
            viewModel.UserName = "";

            // Assert
            viewModel.Validation.Errors["UserName"].Should().ContainSingle();
        }

        [Fact]
        public async Task ShouldNotReValidateOnChange_WhenReValidationIsManual()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            viewModel.Validation.ReValidateMode = ReValidationMode.Manual;
            await viewModel.Validation.ValidateAsync();
            viewModel.Validation.Errors["UserName"].Should().ContainSingle();

            // Act
            viewModel.UserName = "superdev";

            // Assert - the error stays until the next explicit validation
            viewModel.Validation.Errors["UserName"].Should().ContainSingle();
            (await viewModel.Validation.ValidateAsync()).Should().BeTrue();
            viewModel.Validation.Errors["UserName"].Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReValidateOnBlur_WhenReValidationIsOnBlur()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            viewModel.Validation.ReValidateMode = ReValidationMode.OnBlur;
            await viewModel.Validation.ValidateAsync();

            // Act
            viewModel.UserName = "superdev";
            viewModel.Validation.Errors["UserName"].Should().ContainSingle("a change alone does not re-validate");
            viewModel.Validation.MarkTouched(nameof(viewModel.UserName));

            // Assert
            viewModel.Validation.Errors["UserName"].Should().BeEmpty();
        }

        [Fact]
        public void ShouldHonourAPerFieldMode_OverTheFormsMode()
        {
            // Arrange - the form is manual, the password wants live feedback
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                v.Property(() => vm.Password).MinimumLength(8).ValidateOn(ValidationMode.OnChange);
                return v;
            });
            _ = viewModel.Validation;   // a page binds the validation before the user types

            // Act
            viewModel.UserName = "";
            viewModel.Password = "short";

            // Assert
            viewModel.Validation.Errors["UserName"].Should().BeEmpty();
            viewModel.Validation.Errors["Password"].Should().ContainSingle();
        }

        [Fact]
        public async Task ShouldReValidateTheDependentField_WhenTheOtherSideChanges()
        {
            // Arrange - confirm password
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.Password).NotEmpty();
                v.Property(() => vm.PasswordConfirm).EqualTo(() => vm.Password);
                return v;
            });
            viewModel.Password = "secret";
            viewModel.PasswordConfirm = "secret";
            (await viewModel.Validation.ValidateAsync()).Should().BeTrue();

            // Act - the other side changes
            viewModel.Password = "changed";

            // Assert
            viewModel.Validation.Errors["PasswordConfirm"].Should().ContainSingle();
            viewModel.Validation.Errors["PasswordConfirm"][0].Should().Be("Password Confirm must match Password.");
        }

        #endregion

        #region Touched, dirty, reset

        [Fact]
        public async Task ShouldTrackDirty_AgainstTheValueAtSetup_AndReBaselineOnReset()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            viewModel.UserName = "initial";
            var validation = viewModel.Validation;   // setup captures "initial"

            // Act & Assert
            viewModel.UserName = "changed";
            validation.Fields["UserName"].IsDirty.Should().BeTrue();
            validation.IsDirty.Should().BeTrue();

            viewModel.UserName = "initial";
            validation.IsDirty.Should().BeFalse();

            viewModel.UserName = "changed";
            await validation.ValidateAsync();
            validation.Reset();

            validation.IsDirty.Should().BeFalse("the current value is the baseline after a reset");
            validation.HasValidated.Should().BeFalse();
            validation.HasErrors.Should().BeFalse();
            validation.Fields["UserName"].IsTouched.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldClearErrorsButKeepTouched_OnClearErrors()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            await viewModel.Validation.ValidateAsync();
            viewModel.Validation.SetError("UserName", "taken");
            viewModel.Validation.HasErrors.Should().BeTrue();

            // Act
            viewModel.Validation.ClearErrors();

            // Assert
            viewModel.Validation.HasErrors.Should().BeFalse();
            viewModel.Validation.Errors["UserName"].Should().BeEmpty();
            viewModel.Validation.Fields["UserName"].IsTouched.Should().BeTrue();
            viewModel.Validation.HasValidated.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldMarkEveryFieldTouched_OnAnExplicitValidation()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                v.Property(() => vm.Email).EmailAddress();
                return v;
            });

            // Act
            await viewModel.Validation.ValidateAsync();

            // Assert
            viewModel.Validation.Fields.Should().OnlyContain(f => f.IsTouched);
            viewModel.Validation.HasValidated.Should().BeTrue();
        }

        #endregion

        #region IsValid, the silent check

        [Fact]
        public void ShouldComputeIsValidSilently_WithoutShowingErrors()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            var validation = viewModel.Validation;

            // Act & Assert
            validation.IsValid.Should().BeFalse("the user name is empty");
            validation.HasErrors.Should().BeFalse("nothing was shown");
            validation.Errors["UserName"].Should().BeEmpty();

            viewModel.UserName = "superdev";
            validation.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldRaiseIsValid_OnlyWhenItFlips()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            var validation = viewModel.Validation;
            var raised = new List<string?>();
            validation.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            viewModel.UserName = "a";
            viewModel.UserName = "b";
            viewModel.UserName = "";

            // Assert
            raised.Count(n => n == nameof(ViewModelValidation.IsValid)).Should().Be(2);
        }

        #endregion

        #region Errors set from outside

        [Fact]
        public async Task ShouldReplaceSetErrors_NotAppend()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            var validation = viewModel.Validation;
            validation.SetErrors(new Dictionary<string, string[]> { ["UserName"] = new[] { "taken" } });

            // Act
            validation.SetErrors(new Dictionary<string, string[]> { ["UserName"] = new[] { "reserved" } });

            // Assert
            validation.Errors["UserName"].Should().Equal("reserved");
            await Task.CompletedTask;
        }

        [Fact]
        public async Task ShouldKeepSetErrors_ThroughAnExplicitValidation()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            viewModel.UserName = "superdev";
            var validation = viewModel.Validation;
            validation.SetError("UserName", "taken");

            // Act
            var isValid = await validation.ValidateAsync();

            // Assert - the rules pass, the verdict from outside still stands
            isValid.Should().BeFalse();
            validation.Errors["UserName"].Should().Equal("taken");
        }

        [Fact]
        public void ShouldDropSetErrors_WhenTheFieldChanges()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            viewModel.UserName = "superdev";
            var validation = viewModel.Validation;
            validation.SetError("UserName", "taken");

            // Act - the verdict was on the old value
            viewModel.UserName = "superdev2";

            // Assert
            validation.Errors["UserName"].Should().BeEmpty();
            validation.HasErrors.Should().BeFalse();
        }

        [Fact]
        public void ShouldRouteUnownedKeys_ToTheForm_NeverDropThem()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            var validation = viewModel.Validation;

            // Act
            validation.SetErrors(new Dictionary<string, List<string>>
            {
                ["UserName"] = new List<string> { "taken" },
                ["InvalidToken"] = new List<string> { "The token has expired." },
                [""] = new List<string> { "Something is off." },
            });

            // Assert
            validation.Errors["UserName"].Should().Equal("taken");
            validation.Errors.Form.Should().Equal("The token has expired.", "Something is off.");
            validation.Errors.All.Should().HaveCount(3);
            validation.GetErrors().Should().ContainKey(string.Empty);
            validation.GetErrors("").Should().BeEquivalentTo(new[] { "The token has expired.", "Something is off." });
            validation.HasErrors.Should().BeTrue();
        }

        [Fact]
        public void ShouldRouteAliasedKeys_ToTheFieldThatClaimedThem()
        {
            // Arrange - the Identity case
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.Email).NotEmpty().EmailAddress().Alias("DuplicateEmail", "InvalidEmail");
                v.Property(() => vm.Password).NotEmpty().Alias(key => key.StartsWith("Password"));
                return v;
            });
            var validation = viewModel.Validation;

            // Act
            validation.SetErrors(new Dictionary<string, string[]>
            {
                ["DuplicateEmail"] = new[] { "E-mail is already taken." },
                ["PasswordTooShort"] = new[] { "Passwords must be at least 8 characters." },
                ["PasswordRequiresDigit"] = new[] { "Passwords must have a digit." },
                ["InvalidToken"] = new[] { "Invalid token." },
            });

            // Assert
            validation.Errors["Email"].Should().Equal("E-mail is already taken.");
            validation.Errors["Password"].Should().Equal("Passwords must be at least 8 characters.", "Passwords must have a digit.");
            validation.Errors.Form.Should().Equal("Invalid token.");
        }

        [Fact]
        public void ShouldMergeSetErrorsWithRuleErrors_InEveryView()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            viewModel.Validation.Mode = ValidationMode.OnChange;
            viewModel.UserName = "";
            viewModel.Validation.SetError("UserName", "taken");

            // Act
            var errors = viewModel.Validation;

            // Assert
            errors.Errors["UserName"].Should().HaveCount(2);
            errors.Errors.All.Should().HaveCount(2);
            errors.GetErrors()["UserName"].Should().HaveCount(2);
            errors.Fields["UserName"].Errors.Should().HaveCount(2);
            errors.HasErrors.Should().BeTrue();
        }

        [Fact]
        public void ShouldAcceptAValidationResult_AsSetErrors()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            var result = ValidationResult.FromErrors(new Dictionary<string, string[]> { ["UserName"] = new[] { "taken" } });

            // Act
            viewModel.Validation.SetErrors(result);

            // Assert
            viewModel.Validation.Errors["UserName"].Should().Equal("taken");
        }

        #endregion

        #region Validate and act

        [Fact]
        public async Task ShouldRunTheAction_OnlyWhenTheRulesPass()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            var actionRuns = 0;

            // Act
            var invalid = await viewModel.Validation.ValidateAsync(_ =>
            {
                actionRuns++;
                return Task.FromResult(ValidationResult.Success);
            });

            viewModel.UserName = "superdev";
            var valid = await viewModel.Validation.ValidateAsync(_ =>
            {
                actionRuns++;
                return Task.FromResult(ValidationResult.Success);
            });

            // Assert
            invalid.Should().BeFalse();
            valid.Should().BeTrue();
            actionRuns.Should().Be(1);
        }

        [Fact]
        public async Task ShouldShowTheActionsVerdict_AndFocusTheFirstInvalidField()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                v.Property(() => vm.Email).NotEmpty();
                return v;
            });
            viewModel.UserName = "superdev";
            viewModel.Email = "a@b";
            var focused = new List<string>();
            viewModel.Validation.FocusRequested += (_, e) => focused.Add(e.PropertyName);

            // Act
            var succeeded = await viewModel.Validation.ValidateAsync(_ =>
                Task.FromResult(ValidationResult.FromErrors(new Dictionary<string, string[]> { ["Email"] = new[] { "already registered" } })));

            // Assert
            succeeded.Should().BeFalse();
            viewModel.Validation.Errors["Email"].Should().Equal("already registered");
            focused.Should().Equal("Email");
        }

        [Fact]
        public async Task ShouldFocusTheFirstInvalidField_InDeclarationOrder()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                v.Property(() => vm.Email).NotEmpty();
                return v;
            });
            var focused = new List<string>();
            viewModel.Validation.FocusRequested += (_, e) => focused.Add(e.PropertyName);

            // Act
            await viewModel.Validation.ValidateAsync();

            // Assert
            focused.Should().Equal("UserName");
        }

        [Fact]
        public async Task ShouldClearSetErrors_WhenTheActionSucceeds()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            viewModel.UserName = "superdev";
            viewModel.Validation.SetError("UserName", "taken");

            // Act
            var succeeded = await viewModel.Validation.ValidateAsync(_ => Task.FromResult(ValidationResult.Success));

            // Assert
            succeeded.Should().BeTrue();
            viewModel.Validation.HasErrors.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldRefuseASecondValidation_WhileOneIsInProgress()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            viewModel.UserName = "superdev";
            var gate = new TaskCompletionSource();
            var first = viewModel.Validation.ValidateAsync(async _ =>
            {
                await gate.Task;
                return ValidationResult.Success;
            });

            // Act
            viewModel.Validation.IsValidating.Should().BeTrue();
            var second = await viewModel.Validation.ValidateAsync();

            // Assert - the double-tap guard
            second.Should().BeFalse();
            gate.SetResult();
            (await first).Should().BeTrue();
            viewModel.Validation.IsValidating.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldPropagateAThrowingAction_AndResetIsValidating()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            viewModel.UserName = "superdev";

            // Act
            var act = () => viewModel.Validation.ValidateAsync(_ => throw new HttpRequestException("offline"));

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();
            viewModel.Validation.IsValidating.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldRunTheSelectedGroups_AndMakeThemActive()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).MaximumLength(80);
                v.Property(() => vm.Email).NotEmpty().InGroup("Submit");
                return v;
            });

            // Act
            var draft = await viewModel.Validation.ValidateAsync();
            var submit = await viewModel.Validation.ValidateAsync(groups: new[] { "Submit" });

            // Assert
            draft.Should().BeTrue("the draft only runs the ungrouped rules");
            submit.Should().BeFalse();
            viewModel.Validation.ActiveGroups.Should().BeEquivalentTo(new[] { "Submit" });
        }

        #endregion

        #region Nested forms

        [Fact]
        public async Task ShouldValidateIncludedChildren_CreatedLater()
        {
            // Arrange - the child does not exist when the rules are declared
            var viewModel = CreateProfileForm();
            viewModel.Address = new AddressViewModel();

            // Act
            var isValid = await viewModel.Validation.ValidateAsync();

            // Assert
            isValid.Should().BeFalse();
            viewModel.Validation.HasErrors.Should().BeTrue();
            viewModel.Address.Validation.GetErrors().Keys.Should().BeEquivalentTo(new[] { "Street", "PostalCode" }, "a child's errors are its own, unprefixed");
            viewModel.Validation.GetErrors().Keys.Should().BeEquivalentTo(new[] { "UserName", "Address.Street", "Address.PostalCode" });
            viewModel.Validation.Errors.All.Should().HaveCount(3);
        }

        [Fact]
        public void ShouldRouteKeys_ToTheChildThatOwnsThem()
        {
            // Arrange
            var viewModel = CreateProfileForm();
            viewModel.Address = new AddressViewModel();

            // Act
            viewModel.Validation.SetErrors(new Dictionary<string, string[]>
            {
                ["Address.Street"] = new[] { "by prefix" },
                ["PostalCode"] = new[] { "by unique owner" },
                ["UserName"] = new[] { "own" },
            });

            // Assert
            viewModel.Address.Validation.Errors["Street"].Should().Equal("by prefix");
            viewModel.Address.Validation.Errors["PostalCode"].Should().Equal("by unique owner");
            viewModel.Validation.Errors["UserName"].Should().Equal("own");
        }

        [Fact]
        public async Task ShouldCascadeReset_AndFocusIntoAChild()
        {
            // Arrange
            var viewModel = CreateProfileForm();
            viewModel.UserName = "superdev";
            viewModel.Address = new AddressViewModel();
            var focused = new List<string>();
            viewModel.Address.Validation.FocusRequested += (_, e) => focused.Add(e.PropertyName);

            // Act
            await viewModel.Validation.ValidateAsync();
            focused.Should().ContainSingle("the parent is valid, so the first invalid field is the child's").Which.Should().Be("Street");

            viewModel.Validation.Reset();

            // Assert
            viewModel.Address.Validation.HasErrors.Should().BeFalse();
            viewModel.Address.Validation.HasValidated.Should().BeFalse();
        }

        #endregion

        #region Asynchronous rules

        [Fact]
        public async Task ShouldDebounceAnAsyncRule_OnChange_AndRunItAtOnceExplicitly()
        {
            // Arrange
            var time = new FakeTimeProvider();
            var rule = new ScriptedRule { IsValid = false };
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation { TimeProvider = time, Mode = ValidationMode.OnChange };
            validation.Property(nameof(viewModel.UserName), () => viewModel.UserName)
                .MustAsync(rule.EvaluateAsync, debounce: TimeSpan.FromMilliseconds(400));

            // Act - a change waits for the debounce
            var onChange = validation.HandlePropertyChangeAsync("UserName");
            rule.CallCount.Should().Be(0);
            validation.Fields["UserName"].IsPending.Should().BeTrue();
            validation.IsPending.Should().BeTrue();

            time.Advance(TimeSpan.FromMilliseconds(400));
            await onChange;

            // Assert
            rule.CallCount.Should().Be(1);
            validation.Fields["UserName"].IsPending.Should().BeFalse();
            validation.Errors["UserName"].Should().ContainSingle();

            // ... an explicit validation does not wait
            var explicitRun = validation.ValidateAsync();
            explicitRun.IsCompleted.Should().BeTrue();
            rule.CallCount.Should().Be(2);
        }

        [Fact]
        public async Task ShouldDiscardASupersededRun_NeverReportItClean()
        {
            // Arrange - the first run would report "clean", but a newer value arrived meanwhile
            var time = new FakeTimeProvider();
            var rule = new ScriptedRule { IsValid = false };
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation { TimeProvider = time, Mode = ValidationMode.OnChange };
            validation.Property(nameof(viewModel.UserName), () => viewModel.UserName)
                .MustAsync(rule.EvaluateAsync, debounce: TimeSpan.FromMilliseconds(400));
            await validation.ValidateAsync();
            validation.Errors["UserName"].Should().ContainSingle();

            // Act
            rule.IsValid = true;
            var first = validation.HandlePropertyChangeAsync("UserName");
            var second = validation.HandlePropertyChangeAsync("UserName");
            rule.IsValid = false;
            time.Advance(TimeSpan.FromMilliseconds(400));
            await Task.WhenAll(first, second);

            // Assert - only the latest run's verdict is shown
            rule.CallCount.Should().Be(2, "the superseded run never evaluated: its delay was abandoned");
            validation.Errors["UserName"].Should().ContainSingle();
        }

        [Fact]
        public async Task ShouldCountTheLastAsyncVerdict_ForIsValid()
        {
            // Arrange
            var rule = new ScriptedRule { IsValid = false };
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            validation.Property(nameof(viewModel.UserName), () => viewModel.UserName).MustAsync(rule.EvaluateAsync);

            // Act
            validation.IsValid.Should().BeTrue("nothing is known yet; the silent check skips asynchronous rules");
            await validation.ValidateAsync();

            // Assert
            validation.IsValid.Should().BeFalse();
        }

        #endregion

        #region Groups

        [Fact]
        public async Task ShouldDropTheErrorsOfAGroup_WhenItIsNoLongerActive()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.Email).NotEmpty().InGroup("Submit");
                return v;
            });
            await viewModel.Validation.ValidateAsync(groups: new[] { "Submit" });
            viewModel.Validation.Errors["Email"].Should().ContainSingle();

            // Act
            viewModel.Validation.ActiveGroups = new HashSet<string>();

            // Assert
            viewModel.Validation.Errors["Email"].Should().BeEmpty();
            viewModel.Validation.IsValid.Should().BeTrue();
        }

        #endregion

        #region Nested forms: set errors, aggregate state, inherited policy

        [Fact]
        public async Task ShouldRunTheAction_DespiteASetErrorOnAChildField()
        {
            // Arrange - the server rejected the street; the user corrected something else and saves again
            var viewModel = CreateProfileForm();
            viewModel.UserName = "superdev";
            viewModel.Address = new AddressViewModel { Street = "Main Street 1", PostalCode = "8000" };
            viewModel.Validation.SetErrors(new Dictionary<string, string[]> { ["Address.Street"] = new[] { "unknown at this postal code" } });
            var calls = 0;

            // Act
            var saved = await viewModel.Validation.ValidateAsync(_ =>
            {
                calls++;
                return Task.FromResult(ValidationResult.Success);
            });

            // Assert
            calls.Should().Be(1, "the action's verdict replaces whatever was set before, in a child too");
            saved.Should().BeTrue();
            viewModel.Address.Validation.HasErrors.Should().BeFalse("a successful action clears the child's set errors");
            viewModel.Validation.HasErrors.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldRunTheAction_DespiteAFormLevelErrorOnAChild()
        {
            // Arrange - a key the child cannot match lands in its Errors.Form, which no edit clears
            var viewModel = CreateProfileForm();
            viewModel.UserName = "superdev";
            viewModel.Address = new AddressViewModel { Street = "Main Street 1", PostalCode = "8000" };
            viewModel.Validation.SetErrors(new Dictionary<string, string[]> { ["Address.Canton"] = new[] { "not served" } });
            viewModel.Address.Validation.Errors.Form.Should().Equal("not served");

            // Act
            var saved = await viewModel.Validation.ValidateAsync(_ => Task.FromResult(ValidationResult.Success));

            // Assert
            saved.Should().BeTrue();
            viewModel.Address.Validation.Errors.Form.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldFailAPlainValidation_WhileAChildHasASetError()
        {
            // Arrange
            var viewModel = CreateProfileForm();
            viewModel.UserName = "superdev";
            viewModel.Address = new AddressViewModel { Street = "Main Street 1", PostalCode = "8000" };
            viewModel.Validation.SetErrors(new Dictionary<string, string[]> { ["Address.Street"] = new[] { "unknown" } });

            // Act
            var isValid = await viewModel.Validation.ValidateAsync();

            // Assert - without an action there is no fresh verdict, so the child's error still stands
            isValid.Should().BeFalse();
            viewModel.Address.Validation.Errors["Street"].Should().Equal("unknown");
        }

        [Fact]
        public void ShouldRaiseIsValid_WhenAChildBecomesValid()
        {
            // Arrange
            var viewModel = CreateProfileForm();
            viewModel.UserName = "superdev";
            viewModel.Address = new AddressViewModel();
            var validation = viewModel.Validation;
            validation.IsValid.Should().BeFalse("the address is empty");
            var raised = new List<string?>();
            validation.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            viewModel.Address.Street = "Main Street 1";
            viewModel.Address.PostalCode = "8000";

            // Assert
            validation.IsValid.Should().BeTrue();
            raised.Should().Contain(nameof(ViewModelValidation.IsValid), "a Save button bound to the parent follows the child");
        }

        [Fact]
        public async Task ShouldFollowAChildsErrors_InHasErrorsAndTheSummary()
        {
            // Arrange
            var viewModel = CreateProfileForm();
            viewModel.UserName = "superdev";
            viewModel.Address = new AddressViewModel { PostalCode = "8000" };
            await viewModel.Validation.ValidateAsync();
            viewModel.Validation.Errors.All.Should().ContainSingle("the street is missing");
            var raised = new List<string?>();
            viewModel.Validation.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
            var summary = new List<string?>();
            viewModel.Validation.Errors.PropertyChanged += (_, e) => summary.Add(e.PropertyName);

            // Act - the child re-validates on change, since the form was validated
            viewModel.Address.Street = "Main Street 1";

            // Assert
            viewModel.Validation.HasErrors.Should().BeFalse();
            viewModel.Validation.Errors.All.Should().BeEmpty();
            raised.Should().Contain(nameof(ViewModelValidation.HasErrors));
            summary.Should().Contain(nameof(ValidationErrors.All));
        }

        [Fact]
        public void ShouldBeDirty_WhenOnlyAChildChanged()
        {
            // Arrange
            var viewModel = CreateProfileForm();
            viewModel.Address = new AddressViewModel();
            var validation = viewModel.Validation;
            validation.IsDirty.Should().BeFalse();
            var raised = new List<string?>();
            validation.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            viewModel.Address.Street = "Main Street 1";

            // Assert
            validation.IsDirty.Should().BeTrue();
            raised.Should().Contain(nameof(ViewModelValidation.IsDirty));
        }

        [Fact]
        public void ShouldFollowAReplacedChild()
        {
            // Arrange
            var viewModel = CreateProfileForm();
            viewModel.UserName = "superdev";
            var first = new AddressViewModel { Street = "Main Street 1", PostalCode = "8000" };
            viewModel.Address = first;
            viewModel.Validation.IsValid.Should().BeTrue();

            // Act
            var second = new AddressViewModel();
            viewModel.Address = second;

            // Assert
            viewModel.Validation.IsValid.Should().BeFalse("the new address is empty");
            second.Street = "Station Road 2";
            second.PostalCode = "9000";
            viewModel.Validation.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldApplyTheParentsPolicy_ToAChild_UnlessTheChildSetsItsOwn()
        {
            // Arrange
            var time = new FakeTimeProvider();
            var viewModel = CreateProfileForm();
            viewModel.Address = new AddressViewModel();
            var validation = viewModel.Validation;

            // Act
            validation.Mode = ValidationMode.OnChange;
            validation.TimeProvider = time;
            viewModel.Address.Street = " ";

            // Assert
            viewModel.Address.Validation.Errors["Street"].Should().ContainSingle("the child validates on change, as its parent says");
            viewModel.Address.Validation.TimeProvider.Should().BeSameAs(time);

            viewModel.Address.Validation.Mode = ValidationMode.Manual;
            viewModel.Address.PostalCode = "1";
            viewModel.Address.Validation.Errors["PostalCode"].Should().BeEmpty("the child's own setting wins");
        }

        #endregion

        #region Conditions, async timing, edits during a validation

        [Fact]
        public async Task ShouldDropTheError_WhenItsConditionTurnsOff()
        {
            // Arrange
            var viewModel = CreateDiscountForm();
            viewModel.HasDiscount = true;
            await viewModel.Validation.ValidateAsync();
            viewModel.Validation.Errors["DiscountPercent"].Should().ContainSingle();

            // Act
            viewModel.HasDiscount = false;

            // Assert
            viewModel.Validation.Errors["DiscountPercent"].Should().BeEmpty();
            viewModel.Validation.HasErrors.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldValidateTheField_WhenItsConditionTurnsOnAfterAValidation()
        {
            // Arrange
            var viewModel = CreateDiscountForm();
            (await viewModel.Validation.ValidateAsync()).Should().BeTrue("the rule does not apply yet");

            // Act
            viewModel.HasDiscount = true;

            // Assert - the form was validated, so the field is judged at once
            viewModel.Validation.Errors["DiscountPercent"].Should().ContainSingle();
        }

        [Fact]
        public void ShouldNotValidateTheField_WhenItsConditionTurnsOnBeforeAnyValidation()
        {
            // Arrange
            var viewModel = CreateDiscountForm();
            _ = viewModel.Validation;

            // Act
            viewModel.HasDiscount = true;

            // Assert
            viewModel.Validation.Errors["DiscountPercent"].Should().BeEmpty("nothing is shown before the first validation in manual mode");
        }

        [Fact]
        public async Task ShouldShowTheSyncVerdictAtOnce_WhileTheAsyncRuleWaits()
        {
            // Arrange
            var time = new FakeTimeProvider();
            var rule = new ScriptedRule { IsValid = false };
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation { TimeProvider = time, Mode = ValidationMode.OnChange };
            validation.Property(nameof(viewModel.UserName), () => viewModel.UserName)
                .MinimumLength(3)
                .MustAsync(rule.EvaluateAsync, debounce: TimeSpan.FromMilliseconds(400));
            var pendingChanges = new List<string?>();
            validation.Fields["UserName"].PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(FieldState.IsPending))
                {
                    pendingChanges.Add(e.PropertyName);
                }
            };

            viewModel.UserName = "ab";
            await validation.HandlePropertyChangeAsync("UserName");
            validation.Errors["UserName"].Should().ContainSingle().Which.Should().Contain("at least 3");
            pendingChanges.Should().BeEmpty("the asynchronous rule does not run while a synchronous one fails");

            // Act - the third character satisfies the length
            viewModel.UserName = "abc";
            var change = validation.HandlePropertyChangeAsync("UserName");

            // Assert - the length error is gone before the clock moves
            validation.Errors["UserName"].Should().BeEmpty();
            validation.Fields["UserName"].IsPending.Should().BeTrue();
            validation.IsValid.Should().BeFalse("a field waiting for its asynchronous rule is not known to be valid");

            time.Advance(TimeSpan.FromMilliseconds(400));
            await change;
            validation.Errors["UserName"].Should().ContainSingle("the asynchronous rule said no");
            validation.Fields["UserName"].IsPending.Should().BeFalse();
            rule.CallCount.Should().Be(1);
        }

        [Fact]
        public async Task ShouldValidateTheDependents_WithoutWaitingForAnAsyncRule()
        {
            // Arrange
            var time = new FakeTimeProvider();
            var rule = new ScriptedRule { IsValid = true };
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation { TimeProvider = time, Mode = ValidationMode.OnChange };
            validation.Property(nameof(viewModel.Password), () => viewModel.Password)
                .MustAsync(rule.EvaluateAsync, debounce: TimeSpan.FromMilliseconds(400));
            validation.Property(nameof(viewModel.PasswordConfirm), () => viewModel.PasswordConfirm)
                .EqualTo(() => viewModel.Password);
            viewModel.PasswordConfirm = "secret";

            // Act
            viewModel.Password = "other";
            var change = validation.HandlePropertyChangeAsync("Password");

            // Assert - the confirmation is judged before the password's debounce has passed
            validation.Errors["PasswordConfirm"].Should().ContainSingle();
            time.Advance(TimeSpan.FromMilliseconds(400));
            await change;
        }

        [Fact]
        public async Task ShouldValidateAFieldThatChangedDuringTheRules_OnceTheyAreDone()
        {
            // Arrange - a full validation is held inside its rule
            var rule = new ScriptedRule { IsValid = true };
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            validation.Property(nameof(viewModel.UserName), () => viewModel.UserName).MustAsync(rule.EvaluateAsync);
            await validation.ValidateAsync();
            rule.Hold();
            var fullValidation = validation.ValidateAsync();

            // Act
            await validation.HandlePropertyChangeAsync("UserName");
            rule.CallCount.Should().Be(2, "the running validation covers the field for now");
            rule.Release();
            await fullValidation;

            // Assert
            rule.CallCount.Should().Be(3, "the change is validated once the explicit run is done");
        }

        [Fact]
        public async Task ShouldTrackEdits_MadeWhileTheActionRuns()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            viewModel.UserName = "superdev";
            var validation = viewModel.Validation;
            var gate = new TaskCompletionSource<ValidationResult>();
            var save = validation.ValidateAsync(_ => gate.Task);

            // Act - the user edits while the save is in flight
            viewModel.UserName = "superdev2";

            // Assert
            validation.Fields["UserName"].IsDirty.Should().BeTrue("the edit counts at once, not after the save");
            gate.SetResult(ValidationResult.FromError("UserName", "taken"));
            (await save).Should().BeFalse();
            validation.Errors["UserName"].Should().BeEmpty("the server judged the old value, which the user has already replaced");
        }

        #endregion

        #region Cancellation, error codes, dirty text, unknown fields, failures

        [Fact]
        public async Task ShouldThrow_WhenTheValidationIsCancelledBeforeItStarts()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            // Act
            var act = () => viewModel.Validation.ValidateAsync(cancellationToken: cancellation.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
            viewModel.Validation.HasValidated.Should().BeFalse("nothing ran");
        }

        [Fact]
        public async Task ShouldThrow_WhenTheValidationIsCancelledWhileItRuns()
        {
            // Arrange
            var rule = new ScriptedRule { IsValid = true };
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation();
            validation.Property(nameof(viewModel.UserName), () => viewModel.UserName).MustAsync(rule.EvaluateAsync);
            var focused = new List<string>();
            validation.FocusRequested += (_, e) => focused.Add(e.PropertyName);
            using var cancellation = new CancellationTokenSource();
            rule.Hold();

            // Act
            var run = validation.ValidateAsync(cancellationToken: cancellation.Token);
            cancellation.Cancel();
            rule.Release();

            // Assert
            await run.Awaiting(r => r).Should().ThrowAsync<OperationCanceledException>();
            focused.Should().BeEmpty("a cancelled validation reports no verdict");
            validation.IsValidating.Should().BeFalse();
        }

        [Fact]
        public void ShouldKeepTheErrorCodes_OfAResult()
        {
            // Arrange
            var viewModel = CreateUserNameForm();

            // Act
            viewModel.Validation.SetErrors(ValidationResult.FromErrors(new[] { new ValidationError("userName", "DuplicateUserName", "taken") }));

            // Assert
            var error = viewModel.Validation.Fields["UserName"].Errors.Should().ContainSingle().Subject;
            error.ErrorCode.Should().Be("DuplicateUserName");
            error.PropertyName.Should().Be("UserName", "the key is routed to the field in its canonical casing");
        }

        [Fact]
        public void ShouldTreatClearedText_AsUnchanged()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            _ = viewModel.Validation;
            viewModel.UserName = "a";
            viewModel.Validation.IsDirty.Should().BeTrue();

            // Act
            viewModel.UserName = "";

            // Assert
            viewModel.Validation.IsDirty.Should().BeFalse("typed into and cleared again is the value it started with");
        }

        [Fact]
        public void ShouldKeepAnUnknownField_OutOfRoutingAndFocus()
        {
            // Arrange - a binding in the parent's scope names a field only the child has
            var viewModel = CreateProfileForm();
            viewModel.Address = new AddressViewModel();
            var detached = viewModel.Validation.Fields["Street"];

            // Act
            viewModel.Validation.SetErrors(new Dictionary<string, string[]> { ["Street"] = new[] { "unknown" } });

            // Assert
            viewModel.Address.Validation.Errors["Street"].Should().Equal(new[] { "unknown" }, "the child still owns the key");
            detached.HasErrors.Should().BeFalse();
            viewModel.Validation.Fields.Should().ContainSingle("the detached state is not a field of the form");
        }

        [Fact]
        public async Task ShouldRaiseValidationFailed_WhenARuleThrowsOnAChange()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var validation = new ViewModelValidation { Mode = ValidationMode.OnChange };
            validation.Property(nameof(viewModel.UserName), () => viewModel.UserName)
                .Must(_ => throw new InvalidOperationException("The rule threw"));
            var failures = new List<ValidationFailedEventArgs>();
            validation.ValidationFailed += (_, e) => failures.Add(e);

            // Act
            await validation.HandlePropertyChangeAsync("UserName");

            // Assert
            failures.Should().ContainSingle().Which.PropertyName.Should().Be("UserName");
            failures[0].Exception.Should().BeOfType<InvalidOperationException>();
        }

        #endregion

        #region Verdicts on values which changed meanwhile, late verdicts, failing handlers

        [Theory]
        [InlineData(ReValidationMode.OnChange)]
        [InlineData(ReValidationMode.Manual)]
        public async Task ShouldNotRunTheAction_WhenAFieldEditedDuringTheRulesNowFails(ReValidationMode reValidateMode)
        {
            // Arrange - the user name check is held while the user clears the e-mail
            var rule = new ScriptedRule { IsValid = true };
            var viewModel = CreateAccountForm(rule, reValidateMode);
            viewModel.UserName = "superdev";
            viewModel.Email = "info@superdev.ch";
            var actionRuns = 0;
            rule.Hold();
            var save = viewModel.Validation.ValidateAsync(_ =>
            {
                actionRuns++;
                return Task.FromResult(ValidationResult.Success);
            });

            // Act
            viewModel.Email = string.Empty;
            rule.Release();
            var saved = await save;

            // Assert
            saved.Should().BeFalse();
            actionRuns.Should().Be(0, "the action would save an e-mail which fails its rules");
            viewModel.Validation.Errors["Email"].Should().ContainSingle();
        }

        [Fact]
        public async Task ShouldRunTheAction_WhenTheFieldWhichFailedWasFixedDuringTheRules()
        {
            // Arrange - the e-mail fails the first pass; the user fixes it while the user name check is held
            var rule = new ScriptedRule { IsValid = true };
            var viewModel = CreateAccountForm(rule, ReValidationMode.OnChange);
            viewModel.UserName = "superdev";
            var actionRuns = 0;
            rule.Hold();
            var save = viewModel.Validation.ValidateAsync(_ =>
            {
                actionRuns++;
                return Task.FromResult(ValidationResult.Success);
            });

            // Act
            viewModel.Email = "info@superdev.ch";
            rule.Release();
            var saved = await save;

            // Assert
            saved.Should().BeTrue("the verdict is on the values the action saves");
            actionRuns.Should().Be(1);
            viewModel.Validation.Errors["Email"].Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldJudgeAChildFieldEditedDuringTheRules_EvenWhenReValidationIsManual()
        {
            // Arrange
            var rule = new ScriptedRule { IsValid = true };
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation { ReValidateMode = ReValidationMode.Manual };
                v.Property(() => vm.UserName).MustAsync(rule.EvaluateAsync);
                v.Include(nameof(vm.Address), () => vm.Address?.Validation);
                return v;
            });
            viewModel.Address = new AddressViewModel { Street = "Bahnhofstrasse 1", PostalCode = "8001" };
            viewModel.UserName = "superdev";
            rule.Hold();
            var validation = viewModel.Validation.ValidateAsync();

            // Act
            viewModel.Address.Street = string.Empty;
            rule.Release();
            var passed = await validation;

            // Assert
            passed.Should().BeFalse();
            viewModel.Address.Validation.Errors["Street"].Should().ContainSingle();
        }

        [Fact]
        public async Task ShouldValidateAChildViewModel_AssignedWhileTheRulesRun()
        {
            // Arrange - the user name check is held while the view model swaps in an empty address
            var rule = new ScriptedRule { IsValid = true };
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).MustAsync(rule.EvaluateAsync);
                v.Include(nameof(vm.Address), () => vm.Address?.Validation);
                return v;
            });
            viewModel.UserName = "superdev";
            var actionRuns = 0;
            rule.Hold();
            var save = viewModel.Validation.ValidateAsync(_ =>
            {
                actionRuns++;
                return Task.FromResult(ValidationResult.Success);
            });

            // Act
            viewModel.Address = new AddressViewModel();
            rule.Release();
            var saved = await save;

            // Assert
            saved.Should().BeFalse();
            actionRuns.Should().Be(0, "the new address fails its rules");
            viewModel.Address.Validation.Errors["Street"].Should().ContainSingle();
        }

        [Fact]
        public async Task ShouldLeaveAnEdit_ToThePolicy_WhenTheValidationIsCancelled()
        {
            // Arrange
            var rule = new ScriptedRule { IsValid = true };
            var viewModel = CreateAccountForm(rule, ReValidationMode.OnChange);
            viewModel.UserName = "superdev";
            viewModel.Email = "info@superdev.ch";
            (await viewModel.Validation.ValidateAsync()).Should().BeTrue();
            using var cancellation = new CancellationTokenSource();
            rule.Hold();
            var validation = viewModel.Validation.ValidateAsync(cancellationToken: cancellation.Token);

            // Act - the user clears the e-mail, then the validation is cancelled
            viewModel.Email = string.Empty;
            cancellation.Cancel();
            rule.Release();
            var act = () => validation;
            await act.Should().ThrowAsync<OperationCanceledException>();

            // Assert
            viewModel.Validation.Errors["Email"].Should().ContainSingle("the policy validates the edit once the cancelled validation is over");
        }

        [Fact]
        public async Task ShouldValidateAgain_AfterAChildAccessorThrew()
        {
            // Arrange - the accessor assumes an address, which the view model has dropped
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                v.Include(nameof(vm.Address), () => vm.Address!.Validation);
                return v;
            });
            viewModel.Address = new AddressViewModel { Street = "Bahnhofstrasse 1", PostalCode = "8001" };
            viewModel.UserName = "superdev";
            var validation = viewModel.Validation;
            viewModel.Address = null;
            var first = () => validation.ValidateAsync();
            await first.Should().ThrowAsync<NullReferenceException>();

            // Act
            viewModel.Address = new AddressViewModel { Street = "Bahnhofstrasse 1", PostalCode = "8001" };
            var passed = await validation.ValidateAsync();

            // Assert
            validation.IsValidating.Should().BeFalse();
            passed.Should().BeTrue("the failed validation is over, and the accessor works again");
        }

        [Fact]
        public async Task ShouldDropALateAsyncVerdict_WhenItsConditionTurnedOffMeanwhile()
        {
            // Arrange
            var rule = new ScriptedRule { IsValid = false };
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).MustAsync(rule.EvaluateAsync).When(() => vm.HasDiscount);
                return v;
            });
            viewModel.HasDiscount = true;
            viewModel.UserName = "taken";
            var validation = viewModel.Validation;
            rule.Hold();
            var check = validation.ValidatePropertyAsync("UserName");

            // Act - the condition turns off while the check is out
            viewModel.HasDiscount = false;
            rule.Release();
            await check;

            // Assert
            validation.Errors["UserName"].Should().BeEmpty("the rule no longer applies");
            validation.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldDropALateAsyncVerdict_WhenItsGroupWasDeselectedMeanwhile()
        {
            // Arrange
            var rule = new ScriptedRule { IsValid = false };
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Group("Submit", g => g.Property(() => vm.UserName).MustAsync(rule.EvaluateAsync));
                return v;
            });
            viewModel.UserName = "taken";
            var validation = viewModel.Validation;
            validation.ActiveGroups = new HashSet<string> { "Submit" };
            rule.Hold();
            var check = validation.ValidatePropertyAsync("UserName");

            // Act
            validation.ActiveGroups = new HashSet<string>();
            rule.Release();
            await check;

            // Assert
            validation.Errors["UserName"].Should().BeEmpty("the group of the rule is no longer active");
        }

        [Fact]
        public async Task ShouldNotCountAnAsyncVerdict_ForIsValid_OnceTheValueChanged()
        {
            // Arrange - under manual re-validation, the check's error stays until the next validation
            var rule = new ScriptedRule { IsValid = false };
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation { ReValidateMode = ReValidationMode.Manual };
                v.Property(() => vm.UserName).MustAsync(rule.EvaluateAsync);
                return v;
            });
            viewModel.UserName = "taken";
            await viewModel.Validation.ValidateAsync();
            viewModel.Validation.IsValid.Should().BeFalse("the check said no to this value");

            // Act
            viewModel.UserName = "free";

            // Assert
            viewModel.Validation.Errors["UserName"].Should().ContainSingle("manual re-validation leaves the message up");
            viewModel.Validation.IsValid.Should().BeTrue("the check has not judged the new value, so a Save button bound to IsValid stays usable");
        }

        [Fact]
        public void ShouldKeepValidatingOnChange_WhenAConditionThrows()
        {
            // Arrange - the condition reads a child the view model has not loaded yet
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new RecordingValidation { Mode = ValidationMode.OnChange };
                v.Property(() => vm.UserName).NotEmpty();
                v.Property(() => vm.Email).NotEmpty().When(() => vm.Address!.Street != null);
                return v;
            });
            var validation = (RecordingValidation)viewModel.Validation;

            // Act
            viewModel.UserName = string.Empty;

            // Assert
            validation.Errors["UserName"].Should().ContainSingle("a condition which cannot be evaluated yet blocks nothing else");
            validation.Failures.Should().BeEmpty();
        }

        [Fact]
        public void ShouldValidateTheField_OnTheFirstFlipOfItsCondition()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation { Mode = ValidationMode.OnChange };
                v.Property(() => vm.DiscountPercent).NotNull().When(() => vm.HasDiscount);
                return v;
            });
            _ = viewModel.Validation;

            // Act - the very first change after setup turns the condition on
            viewModel.HasDiscount = true;

            // Assert
            viewModel.Validation.Errors["DiscountPercent"].Should().ContainSingle();
        }

        [Fact]
        public void ShouldValidateTheField_OnTheFirstFlipOfItsCondition_AfterAReset()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation { Mode = ValidationMode.OnChange };
                v.Property(() => vm.DiscountPercent).NotNull().When(() => vm.HasDiscount);
                return v;
            });
            viewModel.Validation.Reset();

            // Act - the very first change after the reset turns the condition on
            viewModel.HasDiscount = true;

            // Assert
            viewModel.Validation.Errors["DiscountPercent"].Should().ContainSingle();
        }

        [Fact]
        public void ShouldNotKeepAParent_Alive_ThroughAChildWhichOutlivesIt()
        {
            // Arrange - a session-wide form, included by a page's form
            var child = new ViewModelValidation();
            child.Property("Street", () => (string?)null).NotEmpty();

            // Act
            var parent = IncludeInParent(child);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Assert
            parent.IsAlive.Should().BeFalse("the child reaches its parent through weak links only");
            child.IsValid.Should().BeFalse("the child works on without its parent");
        }

        [Fact]
        public async Task ShouldFocusTheFieldWhoseRuleFailed_NotAnEarlierSetError()
        {
            // Arrange - the server's verdict on the user name is still shown; now the password is missing
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                v.Property(() => vm.Password).NotEmpty();
                return v;
            });
            viewModel.UserName = "superdev";
            var validation = viewModel.Validation;
            validation.SetError("UserName", "This user name is taken.");
            var focused = new List<string>();
            validation.FocusRequested += (_, e) => focused.Add(e.PropertyName);

            // Act
            var saved = await validation.ValidateAsync(_ => Task.FromResult(ValidationResult.Success));

            // Assert
            saved.Should().BeFalse();
            focused.Should().Equal(new[] { "Password" }, "the password's rule stopped the save, not the server's old verdict");
        }

        [Fact]
        public async Task ShouldShowTheDefaultMessage_ForAnErrorWithoutText()
        {
            // Arrange - a backend names the field but sends no text
            var viewModel = CreateUserNameForm();
            viewModel.UserName = "superdev";
            var validation = viewModel.Validation;
            validation.MessageProvider = new DefaultValidationMessageProvider();
            var result = ValidationResult.FromErrors(new Dictionary<string, string[]> { ["UserName"] = new[] { string.Empty } });

            // Act
            var saved = await validation.ValidateAsync(_ => Task.FromResult(result));

            // Assert
            saved.Should().BeFalse();
            validation.Errors["UserName"].Should().Equal(new[] { "User Name is not valid." }, "a failed result never shows nothing");
        }

        [Fact]
        public async Task ShouldValidateAgain_AfterAHandlerOfIsValidatingThrew()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            var validation = viewModel.Validation;
            var fail = true;
            validation.PropertyChanged += (_, e) =>
            {
                if (fail && e.PropertyName == nameof(ViewModelValidation.IsValidating))
                {
                    fail = false;
                    throw new InvalidOperationException("A handler failed.");
                }
            };
            var first = () => validation.ValidateAsync();
            await first.Should().ThrowAsync<InvalidOperationException>();

            // Act
            var passed = await validation.ValidateAsync();

            // Assert
            passed.Should().BeFalse();
            validation.Errors["UserName"].Should().ContainSingle("the second validation ran instead of being refused as a concurrent one");
        }

        [Fact]
        public async Task ShouldNotStayPending_AfterAHandlerOfIsPendingThrew()
        {
            // Arrange
            var rule = new ScriptedRule { IsValid = true };
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).MustAsync(rule.EvaluateAsync);
                return v;
            });
            var validation = viewModel.Validation;
            var fail = true;
            validation.Fields["UserName"].PropertyChanged += (_, e) =>
            {
                if (fail && e.PropertyName == nameof(FieldState.IsPending))
                {
                    fail = false;
                    throw new InvalidOperationException("A handler failed.");
                }
            };

            // Act
            var check = () => validation.ValidatePropertyAsync("UserName");
            await check.Should().ThrowAsync<InvalidOperationException>();

            // Assert
            validation.IsPending.Should().BeFalse();
            validation.Fields["UserName"].IsPending.Should().BeFalse();
        }

        #endregion

        #region Collections changed in place, overlapping validations

        [Fact]
        public async Task ShouldValidateACollection_WhenAnItemIsAddedInPlace()
        {
            // Arrange
            var viewModel = CreateTagsForm();
            viewModel.Tags = new ObservableCollection<string>();
            (await viewModel.Validation.ValidateAsync()).Should().BeFalse();

            // Act
            viewModel.Tags.Add("maui");

            // Assert
            viewModel.Validation.Errors["Tags"].Should().BeEmpty();
            viewModel.Validation.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldCountACollection_AsDirty_ByItsItems()
        {
            // Arrange
            var viewModel = CreateTagsForm();
            viewModel.Tags = new ObservableCollection<string> { "maui" };
            var validation = viewModel.Validation;

            // Act
            viewModel.Tags.Add("validation");
            var dirtyWithTheNewItem = validation.IsDirty;
            viewModel.Tags.Remove("validation");

            // Assert
            dirtyWithTheNewItem.Should().BeTrue();
            validation.IsDirty.Should().BeFalse("the collection holds the items it held at setup again");
        }

        [Fact]
        public async Task ShouldFollowTheCollection_WhenThePropertyGetsANewOne()
        {
            // Arrange
            var viewModel = CreateTagsForm();
            var previous = new ObservableCollection<string> { "maui" };
            viewModel.Tags = previous;
            await viewModel.Validation.ValidateAsync();
            viewModel.Tags = new ObservableCollection<string>();
            previous.Add("stale");

            // Act
            viewModel.Tags.Add("maui");

            // Assert
            viewModel.Validation.Errors["Tags"].Should().BeEmpty("the form watches the collection the property holds now");
        }

        [Fact]
        public void ShouldNotKeepAForm_Alive_ThroughAWatchedCollection()
        {
            // Arrange - a collection that outlives the view model, a shared list say
            var shared = new ObservableCollection<string>();

            // Act
            var form = WatchFromTransientForm(shared);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Assert
            form.IsAlive.Should().BeFalse("the collection reaches the form through a weak subscription only");
            shared.Add("after");
        }

        [Fact]
        public async Task ShouldPassBothValidations_WhenAChildValidatesOnItsOwnWhileItsParentValidates()
        {
            // Arrange - the child's check is held while both validations run
            var rule = new ScriptedRule { IsValid = true };
            var (parent, child) = CreateParentWithAsyncChild(rule);
            rule.Hold();
            var parentValidation = parent.Validation.ValidateAsync();

            // Act - app code validates the child on its own meanwhile
            var childValidation = child.Validation.ValidateAsync();
            rule.Release();

            // Assert
            (await childValidation).Should().BeTrue();
            (await parentValidation).Should().BeTrue("both validations judged the same value, and it passed");
            rule.CallCount.Should().Be(1, "the second validation joined the check already running");
        }

        [Fact]
        public async Task ShouldJudgeAnEdit_InEveryValidationRunningOverTheField()
        {
            // Arrange - the parent and the child validate at once, both waiting for the child's check
            var rule = new ScriptedRule { IsValid = true };
            var (parent, child) = CreateParentWithAsyncChild(rule);
            rule.Hold();
            var parentValidation = parent.Validation.ValidateAsync();
            var childValidation = child.Validation.ValidateAsync();

            // Act - the user clears the child's user name meanwhile
            child.UserName = string.Empty;
            rule.Release();

            // Assert
            (await childValidation).Should().BeFalse();
            (await parentValidation).Should().BeFalse("the parent's verdict is on the user name as it is now, too");
        }

        [Fact]
        public async Task ShouldJudgeAFieldAgain_WhenTheRunItJoinedWasCancelled()
        {
            // Arrange - the parent joins the check of the child's own validation, which is then cancelled
            var rule = new ScriptedRule { IsValid = true };
            var (parent, child) = CreateParentWithAsyncChild(rule);
            using var cancellation = new CancellationTokenSource();
            rule.Hold();
            var childValidation = child.Validation.ValidateAsync(cancellationToken: cancellation.Token);
            var parentValidation = parent.Validation.ValidateAsync();

            // Act
            cancellation.Cancel();
            rule.Release();

            // Assert
            var act = () => childValidation;
            await act.Should().ThrowAsync<OperationCanceledException>();
            (await parentValidation).Should().BeTrue("the parent judged the field again once the run it had joined was cancelled");
        }

        [Fact]
        public async Task ShouldNotJoinARun_WhichJudgesUnderOtherGroups()
        {
            // Arrange - a check started without the Submit group, so the group's length rule did not run in it
            var rule = new GatedRule();
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).MinimumLength(10).InGroup("Submit").MustAsync(rule.EvaluateAsync);
                return v;
            });
            viewModel.UserName = "short";
            var validation = viewModel.Validation;
            var check = validation.ValidatePropertyAsync("UserName");

            // Act
            var submit = validation.ValidateAsync(new[] { "Submit" });
            rule.ReleaseAll();
            await check;

            // Assert
            (await submit).Should().BeFalse("the Submit group's length rule fails for 'short'");
        }

        [Fact]
        public async Task ShouldJudgeTheFieldAnew_WhenTheOwnerOfAJoinedRunCancelsIt()
        {
            // Arrange - a second check joins the first, whose owner then cancels it
            var rule = new GatedRule { HonorsCancellation = true };
            var viewModel = CreateAsyncUserNameForm(rule);
            viewModel.UserName = "superdev";
            var validation = viewModel.Validation;
            using var cancellation = new CancellationTokenSource();
            var owner = validation.ValidatePropertyAsync("UserName", cancellation.Token);
            var joiner = validation.ValidatePropertyAsync("UserName");

            // Act
            rule.HoldsCalls = false;
            cancellation.Cancel();

            // Assert
            var act = () => owner;
            await act.Should().ThrowAsync<OperationCanceledException>();
            (await joiner).Should().BeTrue("the joiner was not cancelled, and the value passes");
        }

        [Fact]
        public async Task ShouldNotJoinACancelledRun_WhenTheValidationIsRetried()
        {
            // Arrange - the check ignores its token, so the cancelled run is still running
            var rule = new GatedRule();
            var viewModel = CreateAsyncUserNameForm(rule);
            viewModel.UserName = "superdev";
            var validation = viewModel.Validation;
            using var cancellation = new CancellationTokenSource();
            var cancelled = validation.ValidatePropertyAsync("UserName", cancellation.Token);
            cancellation.Cancel();

            // Act
            var retry = validation.ValidatePropertyAsync("UserName");
            rule.ReleaseAll();

            // Assert
            (await retry).Should().BeTrue("the retry judged the field itself");
            var act = () => cancelled;
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task ShouldNotFailTheParent_WhileAChildsOwnCheckIsStillRunning()
        {
            // Arrange - the parent judges an edit in a second round while the child validates on its own
            var parentRule = new GatedRule();
            var childRule = new GatedRule { HoldsCalls = false };
            var child = CreateAsyncUserNameForm(childRule);
            child.UserName = "child";
            var parent = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).MustAsync(parentRule.EvaluateAsync);
                v.Include("Account", () => child.Validation);
                return v;
            });
            parent.UserName = "parent";
            _ = child.Validation;
            var parentValidation = parent.Validation.ValidateAsync();
            parent.UserName = "parent2";
            parentRule.Release(0);
            childRule.HoldsCalls = true;
            var childValidation = child.Validation.ValidateAsync();

            // Act
            parentRule.Release(1);
            childRule.Release(1);

            // Assert
            (await childValidation).Should().BeTrue();
            (await parentValidation).Should().BeTrue("every field passed; the child's own check was only still running");
        }

        [Fact]
        public async Task ShouldLeaveAnEdit_ToTheValidationStillRunning_WhenAnotherOneIsCancelled()
        {
            // Arrange - the child's own validation joins the parent's check, and the user edits the child
            var rule = new GatedRule();
            var child = CreateAsyncUserNameForm(rule);
            child.UserName = "superdev";
            var parent = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.Email).NotEmpty();
                v.Include("Account", () => child.Validation);
                return v;
            });
            parent.Email = "info@superdev.ch";
            _ = child.Validation;
            var parentValidation = parent.Validation.ValidateAsync();
            using var cancellation = new CancellationTokenSource();
            var childValidation = child.Validation.ValidateAsync(cancellationToken: cancellation.Token);
            child.UserName = "superdev2";

            // Act
            cancellation.Cancel();
            var callsWhileTheParentRuns = rule.CallCount;
            rule.HoldsCalls = false;
            rule.ReleaseAll();

            // Assert
            callsWhileTheParentRuns.Should().Be(1, "the parent's running validation judges the edit, instead of a run of the policy");
            var act = () => childValidation;
            await act.Should().ThrowAsync<OperationCanceledException>();
            (await parentValidation).Should().BeTrue();
        }

        [Fact]
        public void ShouldNotCopyACollection_WhichDoesNotAnnounceItsChanges()
        {
            // Arrange - a photo of 1 MB
            var photo = new byte[1_000_000];
            var validation = new ViewModelValidation();

            // Act
            var before = GC.GetAllocatedBytesForCurrentThread();
            validation.Property("Photo", () => photo).NotNull();
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            // Assert
            allocated.Should().BeLessThan(photo.Length, "nobody hears of the array's changes, so it counts as its instance");
        }

        [Fact]
        public async Task ShouldNotThrowFromIsValid_WhenACollectionCannotBeRead()
        {
            // Arrange - an asynchronous verdict on a collection, which then throws when read, as one changed on another thread does
            var items = new UnreadableItems { "maui" };
            var validation = new ViewModelValidation();
            validation.Property("Items", () => items).MustAsync((_, _) => Task.FromResult(false));
            await validation.ValidatePropertyAsync("Items");
            items.ThrowsWhenRead = true;

            // Act
            var act = () => validation.IsValid;

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region Helpers

        private static TestFormViewModel CreateAsyncUserNameForm(GatedRule rule)
        {
            return new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).MustAsync(rule.EvaluateAsync);
                return v;
            });
        }

        private static TestFormViewModel CreateTagsForm()
        {
            return new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.Tags).NotEmpty();
                return v;
            });
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference WatchFromTransientForm(ObservableCollection<string> collection)
        {
            var form = new ViewModelValidation();
            form.Property("Tags", () => collection).NotEmpty();
            return new WeakReference(form);
        }

        /// <summary>A parent form which includes a child whose user name is required and has an asynchronous rule.</summary>
        private static (TestFormViewModel Parent, TestFormViewModel Child) CreateParentWithAsyncChild(ScriptedRule rule)
        {
            var child = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty().MustAsync(rule.EvaluateAsync);
                return v;
            });
            child.UserName = "superdev";

            var parent = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.Email).NotEmpty();
                v.Include("Account", () => child.Validation);
                return v;
            });
            parent.Email = "info@superdev.ch";
            return (parent, child);
        }

        private static TestFormViewModel CreateAccountForm(ScriptedRule userNameRule, ReValidationMode reValidateMode)
        {
            return new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation { ReValidateMode = reValidateMode };
                v.Property(() => vm.UserName).MustAsync(userNameRule.EvaluateAsync);
                v.Property(() => vm.Email).NotEmpty();
                return v;
            });
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference IncludeInParent(ViewModelValidation child)
        {
            var parent = new ViewModelValidation();
            parent.Include("Address", () => child);
            _ = parent.IsValid;
            return new WeakReference(parent);
        }

        private static TestFormViewModel CreateUserNameForm()
        {
            return new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                return v;
            });
        }

        private static TestFormViewModel CreateDiscountForm()
        {
            return new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.DiscountPercent).NotNull().When(() => vm.HasDiscount);
                return v;
            });
        }

        private static TestFormViewModel CreateProfileForm()
        {
            return new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                v.Include(nameof(vm.Address), () => vm.Address?.Validation);
                return v;
            });
        }

        /// <summary>
        /// A rule which reports whatever <see cref="IsValid"/> says, counts its calls, and can hold its result until released.
        /// </summary>
        private sealed class ScriptedRule
        {
            private TaskCompletionSource? hold;

            public bool IsValid { get; set; }

            public int CallCount { get; private set; }

            public void Hold()
            {
                this.hold = new TaskCompletionSource();
            }

            public void Release()
            {
                this.hold?.TrySetResult();
            }

            public async Task<bool> EvaluateAsync(string? value, CancellationToken cancellationToken)
            {
                this.CallCount++;

                if (this.hold is { } gate)
                {
                    await gate.Task;
                }

                return this.IsValid;
            }
        }

        /// <summary>A rule whose every call can be held and released one by one, and which can honour its token.</summary>
        private sealed class GatedRule
        {
            private readonly List<TaskCompletionSource<bool>> calls = new List<TaskCompletionSource<bool>>();

            public bool HoldsCalls { get; set; } = true;

            public bool HonorsCancellation { get; set; }

            public int CallCount => this.calls.Count;

            public Task<bool> EvaluateAsync(string? value, CancellationToken cancellationToken)
            {
                var call = new TaskCompletionSource<bool>();
                this.calls.Add(call);
                if (!this.HoldsCalls)
                {
                    call.SetResult(true);
                }

                return this.HonorsCancellation ? call.Task.WaitAsync(cancellationToken) : call.Task;
            }

            public void Release(int call)
            {
                this.calls[call].TrySetResult(true);
            }

            public void ReleaseAll()
            {
                foreach (var call in this.calls.ToArray())
                {
                    call.TrySetResult(true);
                }
            }
        }

        /// <summary>A collection which announces its changes and can be made to throw when read, as one changed on another thread does.</summary>
        private sealed class UnreadableItems : IEnumerable<string>, INotifyCollectionChanged
        {
            private readonly List<string> items = new List<string>();

            public event NotifyCollectionChangedEventHandler? CollectionChanged;

            public bool ThrowsWhenRead { get; set; }

            public void Add(string item)
            {
                this.items.Add(item);
                this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }

            public IEnumerator<string> GetEnumerator()
            {
                return this.ThrowsWhenRead
                    ? throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.")
                    : this.items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private sealed class RecordingValidation : ViewModelValidation
        {
            public List<(string? PropertyName, Exception Exception)> Failures { get; } = new List<(string?, Exception)>();

            protected override void OnValidationFailed(string? propertyName, Exception exception)
            {
                this.Failures.Add((propertyName, exception));
            }
        }

        /// <summary>
        /// Stands in for the UI thread's context: everything posted to it runs elsewhere, but with this
        /// context installed, so a callback can tell that it arrived through the context.
        /// </summary>
        private sealed class RecordingSynchronizationContext : SynchronizationContext
        {
            private int postCount;

            public int PostCount => this.postCount;

            public override void Post(SendOrPostCallback d, object? state)
            {
                Interlocked.Increment(ref this.postCount);

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    SetSynchronizationContext(this);
                    d(state);
                });
            }
        }

        #endregion
    }
}
