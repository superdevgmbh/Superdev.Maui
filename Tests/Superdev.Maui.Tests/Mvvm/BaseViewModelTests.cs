using Superdev.Maui.Mvvm;
using Superdev.Maui.Validation;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Mvvm
{
    public class BaseViewModelTests
    {
        private readonly AutoMocker autoMocker;

        public BaseViewModelTests()
        {
            this.autoMocker = new AutoMocker();
        }

        [Fact]
        public void ShouldReturnTheSameRefreshCommandInstance()
        {
            // Arrange
            var viewModel = this.autoMocker.CreateInstance<TestViewModel>();

            // Act
            var first = viewModel.RefreshCommand;
            var second = viewModel.RefreshCommand;

            // Assert
            second.Should().BeSameAs(
                first, "a new command per get hands bindings and CanExecuteChanged subscribers throwaway objects");
        }

        [Fact]
        public void ShouldNotThrow_WhenRefreshingFails()
        {
            // Arrange
            var viewModel = new ThrowingRefreshViewModel { IsRefreshing = true };

            // Act - a command has no caller which could observe the exception, so an unhandled one here
            // used to terminate the application.
            var act = () => viewModel.RefreshCommand.Execute(null);

            // Assert
            act.Should().NotThrow();
            viewModel.RefreshFailure.Should().BeOfType<InvalidOperationException>();
            viewModel.IsRefreshing.Should().BeFalse("the refresh indicator has to be reset either way");
        }

        [Fact]
        public void ShouldSetUpValidation_AfterTheDerivedConstructorRan()
        {
            // Arrange - the override reads a constructor-injected dependency directly, not through a captured lambda.
            var viewModel = new ValidatedViewModel(new LengthLimit(3));

            // Act
            var validation = viewModel.Validation;

            // Assert
            validation.Should().NotBeNull();
            viewModel.Validation.Should().BeSameAs(validation, "the validation is created once");
            viewModel.MaxLength.Should().Be(3);
            viewModel.SetupValidationCalls.Should().Be(1);
        }

        [Fact]
        public void ShouldNotSetUpValidation_ForAPropertyChangeDuringConstruction()
        {
            // Arrange & Act - the constructor sets a property, which must not trigger the setup
            // while the dependencies of the derived class are not assigned yet.
            var act = () => new ValidatedViewModel(new LengthLimit(3)) { Title = "changed after construction" };

            // Assert
            var viewModel = act.Should().NotThrow().Subject;
            viewModel.SetupValidationCalls.Should().Be(0, "nothing accessed the validation yet");
        }

        private sealed class LengthLimit
        {
            public LengthLimit(int max)
            {
                this.Max = max;
            }

            public int Max { get; }
        }

        private sealed class ValidatedViewModel : BaseViewModel
        {
            private readonly LengthLimit lengthLimit;

            public ValidatedViewModel(LengthLimit lengthLimit)
            {
                this.lengthLimit = lengthLimit;
                this.Subtitle = "set in the constructor";
            }

            public int MaxLength { get; private set; }

            public int SetupValidationCalls { get; private set; }

            protected override ViewModelValidation SetupValidation()
            {
                this.SetupValidationCalls++;

                // Throws a NullReferenceException when called from the base constructor.
                this.MaxLength = this.lengthLimit.Max;
                return new ViewModelValidation();
            }
        }

        private class ThrowingRefreshViewModel : BaseViewModel
        {
            public Exception? RefreshFailure { get; private set; }

            protected override Task OnRefreshing()
            {
                throw new InvalidOperationException("boom");
            }

            protected override void OnRefreshFailed(Exception exception)
            {
                this.RefreshFailure = exception;
            }
        }

        [Fact]
        public void ShouldCreateViewModel()
        {
            // Act
            var viewModel = this.autoMocker.CreateInstance<TestViewModel>();

            // Assert
            viewModel.IsInitialized.Should().BeFalse();
            viewModel.IsBusy.Should().BeTrue();
            viewModel.ViewModelError.Should().Be(ViewModelError.None);
            viewModel.IsContentReady.Should().BeFalse();
        }

        [Fact]
        public void ShouldSetInitialized()
        {
            // Arrange
            var viewModel = this.autoMocker.CreateInstance<TestViewModel>();

            // Act
            viewModel.IsInitialized = true;

            // Assert
            viewModel.IsInitialized.Should().BeTrue();
            viewModel.IsBusy.Should().BeFalse();
            viewModel.IsContentReady.Should().BeTrue();
        }

        [Fact]
        public void ShouldUpdateIsBusy_BusyRefCountEnabled()
        {
            // Arrange
            var viewModel = this.autoMocker.CreateInstance<TestViewModel>();
            viewModel.EnableBusyRefCount = true;

            var propertyChangedCallbacks = new List<string?>();
            viewModel.PropertyChanged += (_, args) => { propertyChangedCallbacks.Add(args.PropertyName); };

            // Act
            viewModel.IsBusy = true;
            viewModel.IsBusy = false;
            viewModel.IsInitialized = true;

            // Assert
            propertyChangedCallbacks.Should().HaveCount(10);
            propertyChangedCallbacks.Should().ContainInOrder(new[]
            {
                "IsBusy", "IsNotBusy", "IsContentReady",
                "IsBusy", "IsNotBusy", "IsContentReady", "IsInitialized",
                "IsBusy", "IsNotBusy", "IsContentReady"
            });
        }

        [Fact]
        public void ShouldUpdateIsBusy_BusyRefCountDisabled()
        {
            // Arrange
            var viewModel = this.autoMocker.CreateInstance<TestViewModel>();
            viewModel.EnableBusyRefCount = false;

            var propertyChangedCallbacks = new List<string?>();
            viewModel.PropertyChanged += (_, args) => { propertyChangedCallbacks.Add(args.PropertyName); };

            // Act
            viewModel.IsBusy = true;
            viewModel.IsBusy = false;
            viewModel.IsInitialized = true;

            // Assert
            propertyChangedCallbacks.Should().HaveCount(10);
            propertyChangedCallbacks.Should().ContainInOrder(new[]
            {
                "IsBusy", "IsNotBusy", "IsContentReady",
                "IsBusy", "IsNotBusy", "IsContentReady", "IsInitialized",
                "IsBusy", "IsNotBusy", "IsContentReady"
            });
        }

        [Fact]
        public void ShouldThrow_SettingIsInitializedBackToFalse()
        {
            // Arrange
            var viewModel = this.autoMocker.CreateInstance<TestViewModel>();
            viewModel.IsInitialized = true;

            var propertyChangedCallbacks = new List<string?>();
            viewModel.PropertyChanged += (_, args) => { propertyChangedCallbacks.Add(args.PropertyName); };

            // Act
            Action action = () => viewModel.IsInitialized = false;

            // Assert
            action.Should().Throw<InvalidOperationException>();

            viewModel.IsInitialized.Should().BeTrue("the rejected value must not be written before the throw");
            viewModel.IsBusy.Should().BeFalse();
            propertyChangedCallbacks.Should().BeEmpty();
        }
        [Fact]
        public void ShouldNotCallSetupValidation_FromTheConstructor()
        {
            // Act
            var viewModel = this.autoMocker.CreateInstance<ValidatingViewModel>();

            // Assert
            viewModel.SetupCalls.Should().Be(0);
        }

        [Fact]
        public void ShouldCreateTheValidationLazily_OnceTheDependenciesAreThere()
        {
            // Arrange
            var viewModel = this.autoMocker.CreateInstance<ValidatingViewModel>();

            // Act
            var first = viewModel.Validation;
            var second = viewModel.Validation;

            // Assert
            first.Should().BeSameAs(second);
            viewModel.SetupCalls.Should().Be(1);
            viewModel.DependencyAtSetup.Should().NotBeNull("the constructor-injected dependency is assigned by the time the rules are set up");
        }

        [Fact]
        public async Task ShouldNeverReturnANullValidation()
        {
            // Arrange
            var viewModel = this.autoMocker.CreateInstance<TestViewModel>();

            // Act
            var validation = viewModel.Validation;

            // Assert
            validation.Should().NotBeNull();
            (await validation.ValidateAsync()).Should().BeTrue("a view model without rules is valid");
        }

        [Fact]
        public void ShouldForwardPropertyChanges_ToTheValidation()
        {
            // Arrange
            var viewModel = this.autoMocker.CreateInstance<ValidatingViewModel>();
            viewModel.Validation.Mode = Superdev.Maui.Validation.ValidationMode.OnChange;

            // Act
            viewModel.UserName = "";

            // Assert
            viewModel.Validation.Errors["UserName"].Should().ContainSingle();
        }

        public interface ITestDependency
        {
        }

        private class ValidatingViewModel : BaseViewModel
        {
            private readonly ITestDependency dependency;
            private string? userName;

            public ValidatingViewModel(ITestDependency dependency)
            {
                this.dependency = dependency;
            }

            public int SetupCalls { get; private set; }

            public ITestDependency? DependencyAtSetup { get; private set; }

            public string? UserName
            {
                get => this.userName;
                set => this.SetProperty(ref this.userName, value);
            }

            protected override Superdev.Maui.Validation.ViewModelValidation SetupValidation()
            {
                this.SetupCalls++;
                this.DependencyAtSetup = this.dependency;

                var validation = new Superdev.Maui.Validation.ViewModelValidation();
                validation.Property(() => this.UserName).NotEmpty();
                return validation;
            }
        }


        private class TestViewModel : BaseViewModel
        {
        }
    }
}