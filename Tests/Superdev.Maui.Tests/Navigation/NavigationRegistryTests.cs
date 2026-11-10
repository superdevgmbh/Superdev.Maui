using Superdev.Maui.Navigation;
using Superdev.Maui.Tests.Navigation.TestDoubles;

namespace Superdev.Maui.Tests.Navigation
{
    /// <summary>
    /// Covers the registry on its own. Lookups run on the caller's thread, before the navigation pipeline
    /// marshals to the UI thread, so the registry has to tolerate being read from several threads at once.
    /// </summary>
    public class NavigationRegistryTests
    {
        [Fact]
        public void ShouldCacheConventionResolvedRegistration()
        {
            // Arrange - the page is not registered explicitly, so it is found by scanning the assemblies
            using var fixture = NavigationServiceFixture.Create(conventionBasedResolution: true);

            // Act
            var first = fixture.NavigationRegistry.GetRegistration(nameof(RegistryConventionPage));
            var second = fixture.NavigationRegistry.GetRegistration(nameof(RegistryConventionPage));

            // Assert
            first.PageType.Should().Be(typeof(RegistryConventionPage));
            second.Should().BeSameAs(first, "the assembly scan happens once per name and its result is cached");
        }

        [Fact]
        public void ShouldNotResolveTheSameMissTwice()
        {
            // Arrange
            using var fixture = NavigationServiceFixture.Create(conventionBasedResolution: true);

            // Act
            var first = fixture.NavigationRegistry.TryGetRegistration("NoSuchPage", out _);
            var second = fixture.NavigationRegistry.TryGetRegistration("NoSuchPage", out _);

            // Assert
            first.Should().BeFalse();
            second.Should().BeFalse("a name which cannot be resolved stays unresolvable");
        }

        [Fact]
        public async Task ShouldResolveFromManyThreadsAtOnce()
        {
            // Arrange - explicit registrations, a convention hit and misses, looked up concurrently.
            // With unsynchronized dictionaries this threw or returned corrupt results; it must do neither.
            using var fixture = NavigationServiceFixture.Create(conventionBasedResolution: true);
            var registry = fixture.NavigationRegistry;
            var names = new[] { "HomePage", "DetailPage", nameof(RegistryConventionPage), "NoSuchPage", "OtherMissingPage" };

            // Act
            var lookups = Enumerable.Range(0, 200)
                .Select(i => names[i % names.Length])
                .Select(name => Task.Run(() => registry.TryGetRegistration(name, out var registration) ? registration : null))
                .ToArray();

            var results = await Task.WhenAll(lookups);

            // Assert
            for (var i = 0; i < results.Length; i++)
            {
                var name = names[i % names.Length];
                if (name.EndsWith("MissingPage") || name == "NoSuchPage")
                {
                    results[i].Should().BeNull($"{name} is not registered and not resolvable");
                }
                else
                {
                    results[i]!.PageType.Name.Should().Be(name);
                }
            }
        }
    }

    /// <summary>
    /// Not registered anywhere, so it can only be found by the convention scan.
    /// </summary>
    internal sealed class RegistryConventionPage : ContentPage
    {
    }
}
