using Superdev.Maui.Mvvm;

namespace Superdev.Maui.Tests.Mvvm
{
    public class ViewModelErrorRegistryTests
    {
        [Fact]
        public void ShouldReturnDefaultViewModelError_IfNotConfigured()
        {
            // Arrange
            IViewModelErrorHandler viewModelErrorHandler = new ViewModelErrorRegistry();
            ViewModelError viewModelError;

            // Act
            try
            {
                throw new Exception("Test exception");
            }
            catch (Exception ex)
            {
                viewModelError = viewModelErrorHandler.FromException(ex);
            }

            // Assert
            viewModelError.Should().NotBeNull();
            viewModelError.Icon.Should().BeNull();
            viewModelError.Title.Should().Be("Test exception");
            viewModelError.Text.Should().NotBeNull();
            viewModelError.Text.Should().Contain("System.Exception: Test exception\n");
            viewModelError.CanRetry.Should().BeFalse();
            viewModelError.RetryCommand.Should().BeNull();
        }

        [Fact]
        public void ShouldCreateFromException_UsingRegisterException()
        {
            // Arrange
            var viewModelErrorRegistry = new ViewModelErrorRegistry();
            viewModelErrorRegistry.SetDefaultFactory(ex => new ViewModelError("default_icon", "default_title", "default_text"));
            viewModelErrorRegistry.RegisterException(ex => ex.Message == "message2", () => new ViewModelError("icon", "title", "text"));

            var viewModelErrorHandler = (IViewModelErrorHandler)viewModelErrorRegistry;

            var exception = new Exception("message1", new InvalidOperationException("message2", new NullReferenceException("message3")));

            // Act
            var viewModelError = viewModelErrorHandler.FromException(exception);

            // Assert
            viewModelError.Should().NotBeNull();
            viewModelError.Icon.Should().Be("icon");
            viewModelError.Title.Should().Be("title");
            viewModelError.Text.Should().Be("text");
            viewModelError.CanRetry.Should().BeFalse();
            viewModelError.RetryCommand.Should().BeNull();
        }

        [Fact]
        public async Task ShouldCreateFromException_WithRetry()
        {
            // Arrange
            var retryCount = 0;
            IViewModelErrorRegistry viewModelErrorRegistry = new ViewModelErrorRegistry();
            viewModelErrorRegistry.SetDefaultFactory(ex => new ViewModelError("default_icon", "default_title", "default_text", "default_retry"));
            viewModelErrorRegistry.RegisterException(ex => ex.Message == "message2", () => new ViewModelError("icon", "title", "text"));

            var viewModelErrorHandler = (IViewModelErrorHandler)viewModelErrorRegistry;
            var exception = new Exception("unknown error");

            var viewModelError = viewModelErrorHandler.FromException(exception).WithRetry(() => { retryCount++; });

            // Act
            await viewModelError.RetryAsync();

            // Assert
            viewModelError.Should().NotBeNull();
            viewModelError.Icon.Should().Be("default_icon");
            viewModelError.Title.Should().Be("default_title");
            viewModelError.Text.Should().Be("default_text");
            viewModelError.RetryButtonText.Should().Be("default_retry");
            viewModelError.CanRetry.Should().BeTrue();
            viewModelError.RetryCommand.Should().NotBeNull();

            retryCount.Should().Be(1);
        }
    }
}