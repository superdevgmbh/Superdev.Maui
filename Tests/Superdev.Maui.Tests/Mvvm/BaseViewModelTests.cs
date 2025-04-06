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

        public class TestViewModel : BaseViewModel
        {

        }
    }
}