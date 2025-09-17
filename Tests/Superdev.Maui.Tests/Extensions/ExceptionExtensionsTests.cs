using Superdev.Maui.Extensions;

namespace Superdev.Maui.Tests.Extensions
{
    public class ExceptionExtensionsTests
    {
        [Fact]
        public void ShouldGetInnerExceptions_WithoutAnyExceptions()
        {
            // Act
            var result = ((Exception)null).GetInnerExceptions().ToList();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ShouldGetInnerExceptions_WithSingleException()
        {
            // Arrange
            var exception = new Exception("message1");

            // Act
            var result = exception.GetInnerExceptions().ToList();

            // Assert
            result.Should().HaveCount(1);
            result.ElementAt(0).Message.Should().Be("message1");
        }

        [Fact]
        public void ShouldGetInnerExceptions_WithInnerException()
        {
            // Arrange
            var exception = new Exception("message1",
                                new InvalidOperationException("message2",
                                    new NullReferenceException("message3")));

            // Act
            var result = exception.GetInnerExceptions().ToList();

            // Assert
            result.Should().HaveCount(3);
            result.ElementAt(0).Message.Should().Be("message1");
            result.ElementAt(1).Message.Should().Be("message2");
            result.ElementAt(2).Message.Should().Be("message3");
        }

        [Fact]
        public void ShouldGetInnerExceptions_WithAggregateException()
        {
            // Arrange
            var exception = new Exception("message1",
                                new AggregateException("message2",
                                    new NullReferenceException("inner1"),
                                    new NullReferenceException("inner2")));

            // Act
            var result = exception.GetInnerExceptions().ToList();

            // Assert
            result.Should().HaveCount(4);
            result.ElementAt(0).Message.Should().Be("message1");
            result.ElementAt(1).Message.Should().Be("message2 (inner1) (inner2)");
            result.ElementAt(2).Message.Should().Be("inner1");
            result.ElementAt(3).Message.Should().Be("inner2");
        }
    }
}