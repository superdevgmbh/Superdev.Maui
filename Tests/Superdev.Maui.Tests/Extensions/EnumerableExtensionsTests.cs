using System.Collections.ObjectModel;
using Superdev.Maui.Extensions;

namespace Superdev.Maui.Tests.Extensions
{
    public partial class EnumerableExtensionsTests
    {
        [Fact]
        public void SortExtensionTest()
        {
            // Arrange.
            IList<int> expectedSortCollection = new Collection<int> { -1, 2, 3, 4, 5, 6, 10 };
            IList<int> sourceCollection = new Collection<int> { 5, 6, 10, 4, 2, 3, -1 };

            // Act.
            sourceCollection.Sort(i => i);

            // Assert.
            sourceCollection.Should().Equal(expectedSortCollection);
        }

        [Fact]
        public void ForEachExtensionTest()
        {
            // Arrange.
            IList<int> expectedCollection = new Collection<int>();
            IList<int> sourceCollection = new Collection<int> { 5, 6, 10, 4, 2, 3, -1 };
            const int AddedOffset = 1;

            // Act.
            sourceCollection.ForEach(x => expectedCollection.Add(x + AddedOffset));

            // Assert.
            expectedCollection.Should().Equal(sourceCollection.Select(x => x + AddedOffset));
        }

        [Fact]
        public void ToObservableCollectionExtensionTest()
        {
            // Arrange.
            var expectedCollection = new ObservableCollection<int> { 5, 6, 10, 4, 2, 3, -1 };
            var sourceCollection = new List<int> { 5, 6, 10, 4, 2, 3, -1 };

            // Act.
            var resultCollection = sourceCollection.ToObservableCollection();

            // Assert.
            resultCollection.Should().Equal(expectedCollection);
        }

        [Fact]
        public void ShouldAppendToList()
        {
            // Arrange.
            var sourceCollection = new List<int> { 1, 2, 3 };

            // Act.
            var resultCollection = sourceCollection.Append(99);

            // Assert
            resultCollection.Should().ContainInOrder(new List<int> { 1, 2, 3, 99 });
        }

        [Fact]
        public void ShouldFindDuplicates_AtLeastTwoDuplicates()
        {
            // Arrange.
            var sourceCollection = new List<Person>
            {
                new Person { Name = "A" },
                new Person { Name = "B" },
                new Person { Name = "B" },
                new Person { Name = "C" },
            };

            // Act
            var duplicates = sourceCollection.FindDuplicates(p => p.Name);

            // Assert
            duplicates.Should().ContainInOrder(new List<Person>
            {
                new Person { Name = "B" },
                new Person { Name = "B" }
            });
        }

        [Fact]
        public void ShouldFindDuplicates_AtLeastThreeDuplicates()
        {
            // Arrange.
            var sourceCollection = new List<Person>
            {
                new Person { Name = "A" },
                new Person { Name = "B" },
                new Person { Name = "B" },
                new Person { Name = "C" },
            };

            // Act
            var duplicates = sourceCollection.FindDuplicates(p => p.Name, numberOfDuplicates: 3);

            // Assert
            duplicates.Should().BeEmpty();
        }
    }
}