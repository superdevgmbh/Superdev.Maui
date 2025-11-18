using System.Collections.ObjectModel;
using Superdev.Maui.Converters;

namespace Superdev.Maui.Tests.Converters
{
    public class IsFirstItemToBoolConverterTests
    {
        [Theory]
        [ClassData(typeof(IsFirstItemToBoolConverterTestData))]
        public void ShouldConvert(object[] input, object expectedOutput)
        {
            // Arrange
            IMultiValueConverter converter = new IsFirstItemToBoolConverter();

            // Act
            var convertedOutput = converter.Convert(input, null, null, null);

            // Assert
            Assert.Equal(expectedOutput, convertedOutput);
        }

        public class IsFirstItemToBoolConverterTestData : TheoryData<object[], object>
        {
            public IsFirstItemToBoolConverterTestData()
            {
                this.Add(null, false);
                this.Add(new[] { "null", "null" }, false);
                this.Add(new[] { new List<string>(), null }, false);
                this.Add(new object[] { new[] { "1st", "2nd" }, "1st" }, true);
                this.Add(new object[] { new List<string> { "1st", "2nd" }, "1st" }, true);
                this.Add(new object[] { new List<string> { "1st", "2nd" }, "1st" }, true);
                this.Add(new object[] { new List<string> { "2st", "3rd" }, "1st" }, false);
                this.Add(new object[] { new ReadOnlyCollection<string>(new List<string> { "1st", "2nd" }), "1st" }, true);
            }
        }

        [Fact]
        public void ShouldConvertBack_ThrowsNotSupportedException()
        {
            // Arrange
            IMultiValueConverter converter = new IsFirstItemToBoolConverter();

            // Act
            Action action = () => converter.ConvertBack(null, null, null, null);

            // Assert
            Assert.Throws<NotSupportedException>(action);
        }
    }
}