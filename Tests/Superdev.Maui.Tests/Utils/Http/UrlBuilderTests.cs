using Superdev.Maui.Utils.Http;

namespace Superdev.Maui.Tests.Utils.Http
{
    public class UrlBuilderTests
    {
        [Fact]
        public void ToString_BaseUrl_ReturnsBaseUrl()
        {
            // Arrange
            UrlBuilder urlBuilder = "https://localhost/test";

            // Act
            var url = urlBuilder.ToString();

            // Assert
            url.Should().Be("https://localhost/test");
            urlBuilder.Url.Should().Be(new Uri("https://localhost/test"));
        }

        [Fact]
        public void AddQuery_StringValues_AppendsQueryParameters()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("https://localhost/test");

            // Act
            var url = urlBuilder
                .AddQuery("name1", "value1")
                .AddQuery("name2", "value2")
                .ToString();

            // Assert
            url.Should().Be("https://localhost/test?name1=value1&name2=value2");
        }

        [Fact]
        public void AddQuery_EnumerableValues_AppendsRepeatedKeys()
        {
            // Arrange
            var values = new[] { "code1", "code2", "code3" };
            var urlBuilder = new UrlBuilder("https://localhost/test");

            // Act
            var url = urlBuilder
                .AddQuery("codes", values)
                .ToString();

            // Assert
            url.Should().Be("https://localhost/test?codes=code1&codes=code2&codes=code3");
        }

        [Fact]
        public void AddQuery_DateTime_AppendsUtcRoundTripValue()
        {
            // Arrange
            var startDate = new DateTime(638398708340000010L);
            var urlBuilder = new UrlBuilder("https://localhost/test");

            // Act
            var url = urlBuilder
                .AddQuery("startDate", startDate)
                .ToString();

            // Assert
            url.Should().Be("https://localhost/test?startDate=2024-01-03T08%3A27%3A14.0000010Z");
        }

        [Fact]
        public void Url_AfterAddingPathAndQuery_ReturnsUpdatedUri()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("https://localhost/test")
                .AddPath("path")
                .AddQuery("name1", "value1");

            // Act
            var url = urlBuilder.Url;

            // Assert
            url.AbsoluteUri.Should().Be("https://localhost/test/path?name1=value1");
        }

        [Fact]
        public void AddQuery_ExistingQueryParameters_AppendsQueryParameter()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("https://localhost/test?name1=value1");

            // Act
            var url = urlBuilder
                .AddQuery("name2", "value2")
                .ToString();

            // Assert
            url.Should().Be("https://localhost/test?name1=value1&name2=value2");
        }

        [Fact]
        public void AddQuery_Flag_AppendsQueryParameterWithoutValue()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("https://localhost/test?name1=value1");

            // Act
            var url = urlBuilder
                .AddQuery("name2")
                .ToString();

            // Assert
            url.Should().Be("https://localhost/test?name1=value1&name2");
        }

        [Fact]
        public void AddPath_BaseUrl_AppendsPath()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("https://localhost");

            // Act
            var url = urlBuilder
                .AddPath("test")
                .ToString();

            // Assert
            url.Should().Be("https://localhost/test");
        }

        [Fact]
        public void AddPath_ExistingQueryParameters_PreservesQueryParameters()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("https://localhost/test?name1=value1");

            // Act
            var url = urlBuilder
                .AddPath("/api")
                .ToString();

            // Assert
            url.Should().Be("https://localhost/test/api?name1=value1");
        }

        [Fact]
        public void AddQuery_EncodesUsingQueryBuilderRules()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("https://localhost/test");

            // Act
            var url = urlBuilder
                .AddQuery("name", "Lake Zurich")
                .AddQuery("return/url", "https://example.test/a#b")
                .ToString();

            // Assert
            url.Should().Be("https://localhost/test?name=Lake%20Zurich&return%2Furl=https%3A%2F%2Fexample.test%2Fa%23b");
        }
    }
}
