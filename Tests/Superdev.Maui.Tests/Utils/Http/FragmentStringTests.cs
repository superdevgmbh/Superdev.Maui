using Superdev.Maui.Utils.Http;

namespace Superdev.Maui.Tests.Utils.Http
{
    public class FragmentStringTests
    {
        [Fact]
        public void Constructor_NonEmptyValueWithoutHash_ThrowsArgumentException()
        {
            // Act
            Action action = () => _ = new FragmentString("section");

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithParameterName("value");
        }

        [Fact]
        public void Constructor_NullOrEmpty_ReturnsEmptyFragment()
        {
            // Act
            var defaultFragment = new FragmentString();
            var nullFragment = new FragmentString(null);
            var emptyFragment = new FragmentString(string.Empty);

            // Assert
            defaultFragment.HasValue.Should().BeFalse();
            defaultFragment.Value.Should().BeNull();
            nullFragment.HasValue.Should().BeFalse();
            nullFragment.Value.Should().BeNull();
            emptyFragment.HasValue.Should().BeFalse();
            emptyFragment.Value.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_HashOnly_ReturnsFragmentWithValue()
        {
            // Act
            var fragmentString = new FragmentString("#");

            // Assert
            fragmentString.HasValue.Should().BeTrue();
            fragmentString.Value.Should().Be("#");
        }

        [Fact]
        public void FromUriComponent_Uri_ReturnsEscapedFragment()
        {
            // Arrange
            var uri = new Uri("https://localhost/test#hello%20world");

            // Act
            var fragmentString = FragmentString.FromUriComponent(uri);

            // Assert
            fragmentString.ToString().Should().Be("#hello%20world");
        }

        [Fact]
        public void Equals_EmptyFragmentStringAndDefaultFragmentString()
        {
            // Act and Assert
            FragmentString.Empty.Should().Be(default(FragmentString));
            (FragmentString.Empty == default(FragmentString)).Should().BeTrue();
            (default(FragmentString) == FragmentString.Empty).Should().BeTrue();
        }

        [Fact]
        public void NotEquals_DefaultFragmentStringAndNonNullFragmentString()
        {
            // Arrange
            var fragmentString = new FragmentString("#col=1");

            // Act and Assert
            default(FragmentString).Should().NotBe(fragmentString);
        }

        [Fact]
        public void NotEquals_EmptyFragmentStringAndNonNullFragmentString()
        {
            // Arrange
            var fragmentString = new FragmentString("#col=1");

            // Act and Assert
            FragmentString.Empty.Should().NotBe(fragmentString);
        }
    }
}
