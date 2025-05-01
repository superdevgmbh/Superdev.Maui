using Superdev.Maui.Mvvm;

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

            var propertyChangedCallbacks = new List<string>();
            viewModel.PropertyChanged += (_, args) => { propertyChangedCallbacks.Add(args.PropertyName); };

            // Act
            viewModel.IsBusy = true;
            viewModel.IsBusy = false;
            viewModel.IsInitialized = true;

            // Assert
            propertyChangedCallbacks.Should().HaveCount(10);
            propertyChangedCallbacks.Should().ContainInOrder(new []
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

            var propertyChangedCallbacks = new List<string>();
            viewModel.PropertyChanged += (_, args) => { propertyChangedCallbacks.Add(args.PropertyName); };

            // Act
            viewModel.IsBusy = true;
            viewModel.IsBusy = false;
            viewModel.IsInitialized = true;

            // Assert
            propertyChangedCallbacks.Should().HaveCount(10);
            propertyChangedCallbacks.Should().ContainInOrder(new []
            {
                "IsBusy", "IsNotBusy", "IsContentReady",
                "IsBusy", "IsNotBusy", "IsContentReady", "IsInitialized",
                "IsBusy", "IsNotBusy", "IsContentReady"
            });
        }


        private class TestViewModel : BaseViewModel
        {
        }
    }
}