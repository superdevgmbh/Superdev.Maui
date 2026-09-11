using Microsoft.Extensions.Primitives;
using Superdev.Maui.Utils.Http;

namespace Superdev.Maui.Tests.Utils.Http
{
    public class QueryStringTests
    {
        [Fact]
        public void Constructor_NonEmptyValueWithoutQuestionMark_ThrowsArgumentException()
        {
            // Act
            Action action = () => _ = new QueryString("id=1");

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithParameterName("value");
        }

        [Fact]
        public void Create_NameAndValue_EncodesNameAndValue()
        {
            // Act
            var queryString = QueryString.Create("return/url", "https://example.test/a#b");

            // Assert
            queryString.ToString().Should().Be("?return%2Furl=https%3A%2F%2Fexample.test%2Fa%23b");
        }

        [Fact]
        public void Constructor_NullOrEmpty_ReturnsEmptyQuery()
        {
            // Act
            var defaultQuery = new QueryString();
            var nullQuery = new QueryString(null);
            var emptyQuery = new QueryString(string.Empty);

            // Assert
            defaultQuery.HasValue.Should().BeFalse();
            defaultQuery.Value.Should().BeNull();
            nullQuery.HasValue.Should().BeFalse();
            nullQuery.Value.Should().BeNull();
            emptyQuery.HasValue.Should().BeFalse();
            emptyQuery.Value.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_QuestionMarkOnly_ReturnsQueryWithValue()
        {
            // Act
            var queryString = new QueryString("?");

            // Assert
            queryString.HasValue.Should().BeTrue();
            queryString.Value.Should().Be("?");
        }

        [Fact]
        public void Create_StringValues_AppendsNullOrEmptyValuesAsEmptyValue()
        {
            // Arrange
            var parameters = new[]
            {
                new KeyValuePair<string, StringValues>("empty", StringValues.Empty),
                new KeyValuePair<string, StringValues>("id", new StringValues(["1", "2"])),
            };

            // Act
            var queryString = QueryString.Create(parameters);

            // Assert
            queryString.ToString().Should().Be("?empty=&id=1&id=2");
        }

        [Fact]
        public void Add_QueryString_ConcatenatesWithAmpersand()
        {
            // Arrange
            var left = QueryString.Create("id", "1");
            var right = QueryString.Create("name", "test value");

            // Act
            var queryString = left.Add(right);

            // Assert
            queryString.ToString().Should().Be("?id=1&name=test%20value");
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", "", "")]
        [InlineData(null, "?name2=value2", "?name2=value2")]
        [InlineData("", "?name2=value2", "?name2=value2")]
        [InlineData("?", "?name2=value2", "?name2=value2")]
        [InlineData("?name1=value1", null, "?name1=value1")]
        [InlineData("?name1=value1", "", "?name1=value1")]
        [InlineData("?name1=value1", "?", "?name1=value1")]
        [InlineData("?name1=value1", "?name2=value2", "?name1=value1&name2=value2")]
        public void Add_QueryString_HandlesEmptyQueryStrings(string? query1, string? query2, string? expected)
        {
            // Arrange
            var left = new QueryString(query1);
            var right = new QueryString(query2);

            // Act
            var queryString = left.Add(right);

            // Assert
            queryString.Value.Should().Be(expected);
            (left + right).Value.Should().Be(expected);
        }

        [Fact]
        public void Equals_EmptyQueryStringAndDefaultQueryString()
        {
            // Act and Assert
            QueryString.Empty.Should().Be(default(QueryString));
            (QueryString.Empty == default(QueryString)).Should().BeTrue();
            (default(QueryString) == QueryString.Empty).Should().BeTrue();
        }

        [Fact]
        public void ToUriComponent_EscapesHashInExistingValue()
        {
            // Arrange
            var queryString = new QueryString("?fragment=a#b");

            // Act
            var uriComponent = queryString.ToUriComponent();

            // Assert
            uriComponent.Should().Be("?fragment=a%23b");
        }
    }
}
