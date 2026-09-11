using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using Superdev.Maui.Tests.Extensions;
using Xunit.Abstractions;

namespace Superdev.Maui.Tests.Mvvm
{
    public class ViewModelErrorRegistryTests
    {
        private readonly AutoMocker autoMocker;

        public ViewModelErrorRegistryTests(ITestOutputHelper testOutputHelper)
        {
            this.autoMocker = new AutoMocker();
            this.autoMocker.UseTestOutputHelperLogger<IViewModelErrorHandler>(testOutputHelper);
        }

        [Fact]
        public void ShouldCreateFromException_ReturnsDefault_IfNotConfigured()
        {
            // Arrange
            IViewModelErrorHandler viewModelErrorHandler = this.autoMocker.CreateInstance<ViewModelErrorRegistry>(enablePrivate: true);
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
            viewModelError.Text.Should().NotBeNullOrEmpty();
            viewModelError.CanRetry.Should().BeFalse();
            viewModelError.RetryCommand.Should().BeNull();
        }

        [Fact]
        public void ShouldCreateFromException_ReturnsDefault_IfNoMatch()
        {
            // Arrange
            var viewModelErrorRegistry = this.autoMocker.CreateInstance<ViewModelErrorRegistry>(enablePrivate: true);
            viewModelErrorRegistry.SetDefaultFactory(
                ex => new ViewModelError("default_icon", "default_title", "default_text"));
            viewModelErrorRegistry.RegisterException(
                ex => ex.Message == "message4",
                () => new ViewModelError("icon", "title", "text"));

            var viewModelErrorHandler = (IViewModelErrorHandler)viewModelErrorRegistry;

            var exception = new Exception("message1",
                new InvalidOperationException("message2",
                    new NullReferenceException("message3")));

            // Act
            var viewModelError = viewModelErrorHandler.FromException(exception);

            // Assert
            viewModelError.Should().NotBeNull();
            viewModelError.Icon.Should().Be("default_icon");
            viewModelError.Title.Should().Be("default_title");
            viewModelError.Text.Should().Be("default_text");
        }

        [Fact]
        public void ShouldCreateFromException_UsingRegisterException()
        {
            // Arrange
            var viewModelErrorRegistry = this.autoMocker.CreateInstance<ViewModelErrorRegistry>(enablePrivate: true);
            viewModelErrorRegistry.SetDefaultFactory(
                ex => new ViewModelError("default_icon", "default_title", "default_text"));
            viewModelErrorRegistry.RegisterException(
                ex => ex.Message == "message2",
                () => new ViewModelError("icon", "title", "text"));

            var viewModelErrorHandler = (IViewModelErrorHandler)viewModelErrorRegistry;

            var exception = new Exception("message1",
                                new InvalidOperationException("message2",
                                    new NullReferenceException("message3")));

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
        public void ShouldCreateFromException_WithPriority()
        {
            // Arrange
            IViewModelErrorRegistry viewModelErrorRegistry = this.autoMocker.CreateInstance<ViewModelErrorRegistry>(enablePrivate: true);
            viewModelErrorRegistry.SetDefaultFactory(
                ex => new ViewModelError("default_icon", "default_title", "default_text"));
            viewModelErrorRegistry.RegisterException(
                ex => ex.Message == "message1",
                () => new ViewModelError("icon1", "title1", "text1"),
                priority: 2000);
            viewModelErrorRegistry.RegisterException(
                ex => ex.Message == "message2",
                () => new ViewModelError("icon2", "title2", "text2"),
                priority: 1000);
            viewModelErrorRegistry.RegisterException(
                ex => ex.Message == "message3",
                () => new ViewModelError("icon3", "title3", "text3"));

            var viewModelErrorHandler = (IViewModelErrorHandler)viewModelErrorRegistry;

            var exception = new Exception("message1",
                                new InvalidOperationException("message2",
                                    new NullReferenceException("message3")));

            // Act
            var viewModelError = viewModelErrorHandler.FromException(exception);

            // Assert
            viewModelError.Should().NotBeNull();
            viewModelError.Icon.Should().Be("icon1");
            viewModelError.Title.Should().Be("title1");
            viewModelError.Text.Should().Be("text1");
        }

        [Fact]
        public void ShouldCreateFromException_WithDepthAndPriority()
        {
            // Arrange
            IViewModelErrorRegistry viewModelErrorRegistry = this.autoMocker.CreateInstance<ViewModelErrorRegistry>(enablePrivate: true);
            viewModelErrorRegistry.SetDefaultFactory(
                ex => new ViewModelError("default_icon", "default_title", "default_text"));
            viewModelErrorRegistry.RegisterException(
                ex => ex.Message == "message1",
                () => new ViewModelError("icon1", "title1", "text1"),
                priority: 1000);
            viewModelErrorRegistry.RegisterException(
                ex => ex.Message == "message2",
                () => new ViewModelError("icon2", "title2", "text2"),
                priority: 1000);
            viewModelErrorRegistry.RegisterException(
                ex => ex.Message == "message3",
                () => new ViewModelError("icon3", "title3", "text3"),
                priority: 0);

            var viewModelErrorHandler = (IViewModelErrorHandler)viewModelErrorRegistry;

            var exception = new Exception("message1",
                                new InvalidOperationException("message2",
                                    new NullReferenceException("message3")));

            // Act
            var viewModelError = viewModelErrorHandler.FromException(exception);

            // Assert
            viewModelError.Should().NotBeNull();
            viewModelError.Icon.Should().Be("icon2");
            viewModelError.Title.Should().Be("title2");
            viewModelError.Text.Should().Be("text2");
        }

        [Fact]
        public void ShouldCreateFromException_WithDepth()
        {
            // Arrange
            IViewModelErrorRegistry viewModelErrorRegistry = this.autoMocker.CreateInstance<ViewModelErrorRegistry>(enablePrivate: true);
            viewModelErrorRegistry.SetDefaultFactory(
                ex => new ViewModelError("default_icon", "default_title", "default_text"));
            viewModelErrorRegistry.RegisterException(
                ex => ex is Exception,
                () => new ViewModelError("icon1", "title1", "text1"));
            viewModelErrorRegistry.RegisterException(
                ex => ex is InvalidOperationException,
                () => new ViewModelError("icon2", "title2", "text2"));
            viewModelErrorRegistry.RegisterException(
                ex => ex is InvalidOperationException { Message: "message3" },
                () => new ViewModelError("icon3", "title3", "text3"));

            var viewModelErrorHandler = (IViewModelErrorHandler)viewModelErrorRegistry;

            var exception = new Exception("message1",
                                new InvalidOperationException("message2",
                                    new InvalidOperationException("message3")));

            // Act
            var viewModelError = viewModelErrorHandler.FromException(exception);

            // Assert
            viewModelError.Should().NotBeNull();
            viewModelError.Icon.Should().Be("icon3");
            viewModelError.Title.Should().Be("title3");
            viewModelError.Text.Should().Be("text3");
        }

        [Fact]
        public async Task ShouldCreateFromException_WithRetry()
        {
            // Arrange
            var retryCount = 0;
            IViewModelErrorRegistry viewModelErrorRegistry = this.autoMocker.CreateInstance<ViewModelErrorRegistry>(enablePrivate: true);
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