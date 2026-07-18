using Superdev.Maui.Extensions;

namespace Superdev.Maui.Tests.Extensions
{
    public partial class EnumerableExtensionsTests
    {
        // Tests for the seeded Shuffle(source, Random) overload. This overload is provided by the library
        // on all target frameworks (unlike the parameterless overload, which is delegated to the BCL on .NET 10+).

        [Fact]
        public void Shuffle_WithRandom_ShouldNotAlterTheSizeOfTheEnumerable()
        {
            var source = Enumerable.Range(1, 1000);

            source.Shuffle(new Random(42))
                .Count()
                .Should().Be(source.Count());
        }

        [Fact]
        public void Shuffle_WithRandom_ShouldContainAllElementsFromTheBaseEnumerable()
        {
            var source = Enumerable.Range(1, 1000).ToArray();

            var shuffled = source.Shuffle(new Random(42)).ToArray();

            shuffled.OrderBy(x => x).Should().Equal(source);
        }

        [Fact]
        public void Shuffle_WithRandom_ShouldNotReturnElementsInTheSameOrderAsBaseEnumerable()
        {
            var source = Enumerable.Range(1, 1000).ToArray();

            source.Shuffle(new Random(42))
                .SequenceEqual(source)
                .Should().BeFalse();
        }

        [Fact]
        public void Shuffle_WithSameSeed_ShouldProduceTheSameOrder()
        {
            var source = Enumerable.Range(1, 1000).ToArray();

            var first = source.Shuffle(new Random(1234)).ToArray();
            var second = source.Shuffle(new Random(1234)).ToArray();

            first.SequenceEqual(second).Should().BeTrue();
        }

        [Fact]
        public void Shuffle_WithRandom_ShouldReturnDifferentOrderOnEachEnumeration()
        {
            var source = Enumerable.Range(1, 1000).ToArray();

            // The returned sequence is deferred and reshuffles on each enumeration (single Random instance).
            var shuffled = source.Shuffle(new Random(42));

            shuffled
                .ToArray()
                .SequenceEqual(shuffled.ToArray())
                .Should().BeFalse();
        }

        [Fact]
        public void Shuffle_WithNullSource_ShouldThrowArgumentNullException()
        {
            IEnumerable<int> source = null!;

            Action action = () => source.Shuffle(new Random(42)).ToArray();

            action.Should().Throw<ArgumentNullException>();
        }
    }
}
