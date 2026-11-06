using System.Globalization;
using Superdev.Maui.Extensions;

namespace Superdev.Maui.Tests.Extensions
{
    public class StringExtensionsTests
    {
        [Fact]
        public void ShouldCheckIfStringContains_ReturnsTrue_InvariantCultureIgnoreCase()
        {
            // Arrange
            var inputString = "ABCDefg";

            // Act
            var contains = StringExtensions.Contains(inputString, "cde", StringComparison.InvariantCultureIgnoreCase);

            // Assert
            contains.Should().BeTrue();
        }

        [Fact]
        public void ShouldCheckIfStringContains_ReturnsFalse_InvariantCultureIgnoreCase()
        {
            // Arrange
            var inputString = "ABCDefg";

            // Act
            var contains = StringExtensions.Contains(inputString, "xxx", StringComparison.InvariantCultureIgnoreCase);

            // Assert
            contains.Should().BeFalse();
        }

        [Fact]
        public void ShouldCheckIfStringContainsAny_ReturnsTrue()
        {
            // Arrange
            var inputString = "ABCDefg";

            // Act
            var contains = inputString.ContainsAny(new[] { "CDe", "xxx" });

            // Assert
            contains.Should().BeTrue();
        }

        [Fact]
        public void ShouldCheckIfStringContainsAny_ReturnsFalse()
        {
            // Arrange
            var inputString = "ABCDefg";

            // Act
            var contains = inputString.ContainsAny(new[] { "cde", "xxx" });

            // Assert
            contains.Should().BeFalse();
        }

        [Fact]
        public void ShouldCheckIfStringContainsAny_ReturnsTrue_InvariantCultureIgnoreCase()
        {
            // Arrange
            var inputString = "ABCDefg";

            // Act
            var contains = inputString.ContainsAny(new[] { "cde", "xxx" }, StringComparison.InvariantCultureIgnoreCase);

            // Assert
            contains.Should().BeTrue();
        }

        [Fact]
        public void ShouldCheckIfStringContainsAny_ReturnsFalse_InvariantCultureIgnoreCase()
        {
            // Arrange
            var inputString = "ABCDefg";

            // Act
            var contains = inputString.ContainsAny(new[] { "xxx", "yyy" }, StringComparison.InvariantCultureIgnoreCase);

            // Assert
            contains.Should().BeFalse();
        }

        [Theory]
        [ClassData(typeof(ToUpperFirstTestData))]
        public void ShouldToUpperFirst(string? input, string? expectedOutput)
        {
            // Act
            var output = input.ToUpperFirst();

            // Assert
            output.Should().Be(expectedOutput);
        }

        public class ToUpperFirstTestData : TheoryData<string?, string?>
        {
            public ToUpperFirstTestData()
            {
                this.Add(null, null);
                this.Add("", "");
                this.Add("t", "T");
                this.Add("test", "Test");
            }
        }

        [Fact]
        public void ToUpperFirst_UnderTurkishCulture_UsesInvariantCasing()
        {
            // Arrange
            using var cultureScope = new CultureScope("tr-TR");

            // Act
            var output = "italiano".ToUpperFirst();

            // Assert
            output.Should().Be("Italiano", "Turkish casing would turn the i into a dotted İ");
        }

        [Fact]
        public void ToUpperFirst_WithCulture_UsesItsCasing()
        {
            // Act
            var output = "istanbul".ToUpperFirst(CultureInfo.GetCultureInfo("tr-TR"));

            // Assert
            output.Should().Be("İstanbul");
        }

        [Fact]
        public void StartsWithAny_WithMatchingPrefix_ReturnsTrue()
        {
            // Act
            var startsWith = "Superdev.Maui".StartsWithAny(new[] { "Xamarin", "Superdev" });

            // Assert
            startsWith.Should().BeTrue();
        }

        [Fact]
        public void StartsWithAny_WithCanonicallyEquivalentPrefix_ComparesOrdinally()
        {
            // Arrange
            const string decomposed = "été";

            // Act
            var startsWith = decomposed.StartsWithAny(new[] { "é" });

            // Assert
            startsWith.Should().BeFalse("a culture-sensitive comparison would treat e + combining accent as é");
        }

        [Fact]
        public void StartsWithAny_WithNullSource_ReturnsFalse()
        {
            // Arrange
            const string? source = null;

            // Act
            var startsWith = source.StartsWithAny(new[] { "a" });

            // Assert
            startsWith.Should().BeFalse();
        }

        [Theory]
        [InlineData("Superdev.Maui", "Super%", true)]
        [InlineData("Superdev.Maui", "%Maui", true)]
        [InlineData("Superdev.Maui", "%dev%", true)]
        [InlineData("Superdev.Maui", "Maui%", false)]
        [InlineData("cat", "c_t", true)]
        [InlineData("coat", "c_t", false)]
        [InlineData("line1\nline2", "line1%line2", true)]
        [InlineData("Maui", "maui", false)]
        public void Like_WithPattern_ReturnsExpectedMatch(string source, string pattern, bool expected)
        {
            // Act
            var matches = source.Like(pattern);

            // Assert
            matches.Should().Be(expected);
        }

        [Theory]
        [InlineData(@"a.b(c)*d+e?[f]{g}|h$^i\j#k l")]
        [InlineData(@"\A.*\z")]
        public void Like_WithRegexMetacharacters_MatchesThemLiterally(string text)
        {
            // Act
            var matchesItself = text.Like(text);
            var matchesOther = "x".Like(text);

            // Assert
            matchesItself.Should().BeTrue();
            matchesOther.Should().BeFalse();
        }

        [Fact]
        public void Like_WithNullSource_ThrowsArgumentNullException()
        {
            // Arrange
            const string? source = null;

            // Act
            var act = () => source.Like("%");

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("source");
        }

        [Theory]
        [ClassData(typeof(TrimStartAndEndTestData))]
        public void ShouldTrimStartAndEnd(string? input, string? expectedOutput)
        {
            // Act
            var output = input.TrimStartAndEnd();

            // Assert
            output.Should().Be(expectedOutput);
        }

        public class TrimStartAndEndTestData : TheoryData<string?, string?>
        {
            public TrimStartAndEndTestData()
            {
                this.Add(null, null);
                this.Add($"{Environment.NewLine}", "");
                this.Add($"test", "test");
                this.Add($"{Environment.NewLine}test{Environment.NewLine}{Environment.NewLine}", "test");
                this.Add($"{Environment.NewLine}test{Environment.NewLine}test2{Environment.NewLine}", $"test{Environment.NewLine}test2");
            }
        }

        [Theory]
        [ClassData(typeof(TrimWhitespacesTestData))]
        public void ShouldTrimWhitespaces(string? input, string? expectedOutput)
        {
            // Act
            var output = input.TrimWhitespaces();

            // Assert
            output.Should().Be(expectedOutput);
        }

        public class TrimWhitespacesTestData : TheoryData<string?, string?>
        {
            public TrimWhitespacesTestData()
            {
                this.Add(null, null);
                this.Add("", "");
                this.Add(" ", "");
                this.Add("  ", "");
                this.Add("test", "test");
                this.Add("  A  and     B   ", "A and B");
            }
        }

        [Theory]
        [ClassData(typeof(RemoveEmptyLinesTestData))]
        public void ShouldRemoveEmptyLines(string? input, string? expectedOutput)
        {
            // Act
            var output = input.RemoveEmptyLines();

            // Assert
            output.Should().Be(expectedOutput);
        }

        public class RemoveEmptyLinesTestData : TheoryData<string?, string?>
        {
            public RemoveEmptyLinesTestData()
            {
                this.Add(null, null);
                this.Add($"{Environment.NewLine}", "");
                this.Add($"test", "test");
                this.Add($"{Environment.NewLine}test{Environment.NewLine}{Environment.NewLine}", "test");
            }
        }

         [Theory]
        [ClassData(typeof(TruncateTestData))]
        public void Truncate_WithMaxLength_ReturnsExpectedString(string? input, int maxLength, string? truncationIndicator, string? expectedOutput)
        {
            // Act
            var truncatedString = input.Truncate(maxLength, truncationIndicator);

            // Assert
            truncatedString.Should().Be(expectedOutput);
            truncatedString!.Length.Should().BeLessThanOrEqualTo(maxLength);
        }

        public class TruncateTestData : TheoryData<string?, int, string?, string?>
        {
            public TruncateTestData()
            {
                this.Add("abc", 10, "(...)", "abc");
                this.Add("abc", 3, "(...)", "abc");
                this.Add("abcdef", 5, "(...)", "(...)");
                this.Add("abcdef", 4, "(...)", "(...");
                this.Add("abcdef", 5, "...", "ab...");
                this.Add("abcdef", 3, string.Empty, "abc");
                this.Add("abcdef", 3, null, "abc");
            }
        }

        [Fact]
        public void Truncate_WithNullInput_ReturnsNull()
        {
            // Arrange
            const string? input = null;

            // Act
            var truncatedString = input.Truncate(3, "(...)");

            // Assert
            truncatedString.Should().BeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Truncate_WithInvalidMaxLength_ThrowsArgumentOutOfRangeException(int maxLength)
        {
            // Arrange
            const string input = "abcdef";

            // Act
            var act = () => input.Truncate(maxLength, "(...)");

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName(nameof(maxLength));
        }

        [Fact]
        public void Truncate_WithoutTruncationIndicator_TruncatesWithoutIndicator()
        {
            // Arrange
            const string input = "abcdef";

            // Act
            var truncatedString = input.Truncate(3);

            // Assert
            truncatedString.Should().Be("abc");
        }
    }
}