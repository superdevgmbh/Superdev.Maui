using System.Collections.ObjectModel;
using Superdev.Maui.Converters;

namespace Superdev.Maui.Tests.Converters
{
    public class EnumerableToFirstOrDefaultConverterTests
    {
        [Theory]
        [ClassData(typeof(EnumerableToFirstOrDefaultConverterTestdata))]
        public void ShouldConvert(object input, object expectedOutput)
        {
            // Arrange
            IValueConverter converter = new EnumerableToFirstOrDefaultConverter();

            // Act
            var convertedOutput = converter.Convert(input, null, null, null);

            // Assert
            Assert.Equal(expectedOutput, convertedOutput);
        }

        public class EnumerableToFirstOrDefaultConverterTestdata : TheoryData<object, object>
        {
            public EnumerableToFirstOrDefaultConverterTestdata()
            {
                this.Add(null, null);
                this.Add("null", 'n');
                this.Add(new List<string>(), null);
                this.Add(new[] { "first" }, "first");
                this.Add(new[] { "first", "second" }, "first");
                this.Add(new List<string> { "first" }, "first");
                this.Add(new List<string> { "first", "second" }, "first");
                this.Add(new ReadOnlyCollection<string>(new List<string> { "first", "second" }), "first");
            }
        }

        [Fact]
        public void ShouldConvertBack_ThrowsNotSupportedException()
        {
            // Arrange
            IValueConverter converter = new EnumerableToFirstOrDefaultConverter();

            // Act
            Action action = () => converter.ConvertBack(null, null, null, null);

            // Assert
            Assert.Throws<NotSupportedException>(action);
        }
    }
}