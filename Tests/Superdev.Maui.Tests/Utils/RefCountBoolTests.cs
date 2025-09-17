using Moq.AutoMock;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Utils;

namespace Superdev.Maui.Tests.Utils
{
    public class RefCountBoolTests
    {
        private readonly AutoMocker autoMocker;

        public RefCountBoolTests()
        {
            this.autoMocker = new AutoMocker();
        }

        [Fact]
        public void IsBusyTest_TrueTrueFalse()
        {
            // Arrange
            var propertyChangedList = new List<string>();
            var viewModel = this.autoMocker.CreateInstance<TestViewModel>();
            viewModel.PropertyChanged += (sender, args) => { propertyChangedList.Add(args.PropertyName); };

            // Act
            viewModel.IsScannerBusy = true;
            viewModel.IsScannerBusy = true;
            viewModel.IsScannerBusy = false;

            // Assert
            viewModel.IsScannerBusy.Should().BeTrue();
            propertyChangedList.Should().HaveCount(1);
            propertyChangedList.Should().ContainInOrder(new []
            {
                "IsScannerBusy",
            });
        }

        [Fact]
        public void IsBusyTest_TrueTrueFalseFalse()
        {
            // Arrange
            var propertyChangedList = new List<string>();
            var viewModel = this.autoMocker.CreateInstance<TestViewModel>();
            viewModel.PropertyChanged += (sender, args) => { propertyChangedList.Add(args.PropertyName); };

            // Act
            viewModel.IsScannerBusy = true;
            viewModel.IsScannerBusy = true;
            viewModel.IsScannerBusy = false;
            viewModel.IsScannerBusy = false;

            // Assert
            viewModel.IsScannerBusy.Should().BeFalse();
            propertyChangedList.Should().HaveCount(2);
            propertyChangedList.Should().ContainInOrder(new []
            {
                "IsScannerBusy",
                "IsScannerBusy",
            });
        }

        [Fact]
        public void IsBusyTest_FalseFalseTrue()
        {
            // Arrange
            var propertyChangedList = new List<string>();
            var viewModel = this.autoMocker.CreateInstance<TestViewModel>();
            viewModel.PropertyChanged += (sender, args) => { propertyChangedList.Add(args.PropertyName); };

            // Act
            viewModel.IsScannerBusy = false;
            viewModel.IsScannerBusy = false;
            viewModel.IsScannerBusy = true;

            // Assert
            viewModel.IsScannerBusy.Should().BeTrue();
            propertyChangedList.Should().HaveCount(1);
            propertyChangedList.Should().ContainInOrder(new []
            {
                "IsScannerBusy",
            });
        }

        [Fact]
        public void IsBusyTest_TrueFalseFalse()
        {
            // Arrange
            var propertyChangedList = new List<string>();
            var viewModel = this.autoMocker.CreateInstance<TestViewModel>();
            viewModel.PropertyChanged += (sender, args) => { propertyChangedList.Add(args.PropertyName); };

            // Act
            viewModel.IsScannerBusy = true;
            viewModel.IsScannerBusy = false;
            viewModel.IsScannerBusy = false;

            // Assert
            viewModel.IsScannerBusy.Should().BeFalse();
            propertyChangedList.Should().HaveCount(2);
            propertyChangedList.Should().ContainInOrder(new []
            {
                "IsScannerBusy",
                "IsScannerBusy",
            });
        }
    }

    public class TestViewModel : BaseViewModel
    {
        private readonly RefCountBool isScannerBusy = new RefCountBool();

        public bool IsScannerBusy
        {
            get => this.isScannerBusy;
            set
            {
                if (this.SetProperty(this.isScannerBusy, value))
                {

                }
            }
        }
    }
}
