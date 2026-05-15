using Microsoft.Extensions.Primitives;
using Superdev.Maui.Utils.Http;

namespace Superdev.Maui.Tests.Utils.Http
{
    public class QueryBuilderTests
    {
        [Fact]
        public void ToString_CollectionInitializer_EncodesKeysAndValues()
        {
            // Arrange
            var queryBuilder = new QueryBuilder
            {
                { "id", "1" },
                { "name", "Lake Zurich" },
                { "return/url", "https://example.test/a#b" },
            };

            // Act
            var queryString = queryBuilder.ToString();

            // Assert
            queryString.Should().Be("?id=1&name=Lake%20Zurich&return%2Furl=https%3A%2F%2Fexample.test%2Fa%23b");
        }

        [Fact]
        public void ToString_MultipleValuesForSameKey_AppendsRepeatedKey()
        {
            // Arrange
            var queryBuilder = new QueryBuilder
            {
                { "id", ["1", "2", "3"] },
            };

            // Act
            var queryString = queryBuilder.ToString();

            // Assert
            queryString.Should().Be("?id=1&id=2&id=3");
        }

        [Fact]
        public void Constructor_StringValues_ExpandsValues()
        {
            // Arrange
            var parameters = new[]
            {
                new KeyValuePair<string, StringValues>("id", new StringValues(["1", "2"])),
                new KeyValuePair<string, StringValues>("name", "test value"),
            };

            // Act
            var queryString = new QueryBuilder(parameters).ToString();

            // Assert
            queryString.Should().Be("?id=1&id=2&name=test%20value");
        }

        [Fact]
        public void ToQueryString_EmptyBuilder_ReturnsEmptyQueryString()
        {
            // Arrange
            var queryBuilder = new QueryBuilder();

            // Act
            var queryString = queryBuilder.ToQueryString();

            // Assert
            queryString.Should().Be(QueryString.Empty);
            queryString.ToString().Should().BeEmpty();
        }

        [Fact]
        public void ToString_ReservedCharacters_PercentEncodesKeyAndValue()
        {
            // Arrange
            var queryBuilder = new QueryBuilder
            {
                { "key&", "value#" },
            };

            // Act
            var queryString = queryBuilder.ToString();

            // Assert
            queryString.Should().Be("?key%26=value%23");
        }

        [Fact]
        public void Constructor_StringValues_SkipsEmptyStringValues()
        {
            // Arrange
            var parameters = new[]
            {
                new KeyValuePair<string, StringValues>("key1", new StringValues(["value1", string.Empty, "value3"])),
                new KeyValuePair<string, StringValues>("key2", string.Empty),
                new KeyValuePair<string, StringValues>("key3", StringValues.Empty),
            };

            // Act
            var queryString = new QueryBuilder(parameters).ToString();

            // Assert
            queryString.Should().Be("?key1=value1&key1=&key1=value3&key2=");
        }
    }
}
