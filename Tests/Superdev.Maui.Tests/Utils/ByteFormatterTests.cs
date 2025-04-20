using Superdev.Maui.Utils;
using Xunit.Abstractions;

namespace Superdev.Maui.Tests.Utils
{
    public class ByteFormatterTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public ByteFormatterTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Theory]
        [ClassData(typeof(GetNamedSizeTestdata))]
        public void ShouldGetNamedSize(long byteLength, int decimalPlaces, string expectedNamedSize)
        {
            // Act
            var namedSize = ByteFormatter.GetNamedSize(byteLength, decimalPlaces);

            // Assert
            this.testOutputHelper.WriteLine($"{byteLength} bytes = {namedSize}");

            Assert.Equal(expectedNamedSize, namedSize);
        }

        public class GetNamedSizeTestdata : TheoryData<long, int, string>
        {
            public GetNamedSizeTestdata()
            {
                {
                    const int decimalPlaces = 0;
                    this.Add(0, decimalPlaces, "0 bytes");
                    this.Add(1024, decimalPlaces, "1 KB");
                    this.Add((long)Math.Pow(1024, 2), decimalPlaces, "1 MB");
                    this.Add((long)(Math.Pow(1024, 2) * 1.375d), decimalPlaces, "1 MB");
                    this.Add((long)Math.Pow(1024, 3), decimalPlaces, "1 GB");
                    this.Add((long)Math.Pow(1024, 4), decimalPlaces, "1 TB");
                }
                {
                    const int decimalPlaces = 1;
                    this.Add(0, decimalPlaces, "0.0 bytes");
                    this.Add(1024, decimalPlaces, "1.0 KB");
                    this.Add((long)Math.Pow(1024, 2), decimalPlaces, "1.0 MB");
                    this.Add((long)(Math.Pow(1024, 2) * 1.375d), decimalPlaces, "1.4 MB");
                    this.Add((long)Math.Pow(1024, 3), decimalPlaces, "1.0 GB");
                    this.Add((long)Math.Pow(1024, 4), decimalPlaces, "1.0 TB");
                }
            }
        }
    }
}