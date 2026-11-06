using System.Reflection;
using System.Text.RegularExpressions;
using Superdev.Maui.Resources.Styles;

namespace Superdev.Maui.Tests.Resources
{
    /// <summary>
    /// Asserts that every resource key referenced from XAML is actually defined somewhere.
    /// </summary>
    /// <remarks>
    /// A <c>StaticResource</c> or <c>DynamicResource</c> pointing at a key which does not exist does not
    /// fail the build and does not fail at runtime either: the property is simply never set and the element
    /// falls back to its style or to a platform default. Five such typos were found by reading the XAML by
    /// hand; this test is what finds the sixth.
    /// </remarks>
    public class XamlResourceKeyTests
    {
        /// <summary>
        /// Matches <c>{StaticResource Key}</c> and <c>{DynamicResource Key}</c>, including nested ones such
        /// as <c>{Binding X, Converter={StaticResource Key}}</c>.
        /// </summary>
        private static readonly Regex ResourceReferenceRegex = new Regex(
            @"\{\s*(?:Static|Dynamic)Resource\s+(?<key>[^},\s]+)",
            RegexOptions.Compiled);

        /// <summary>
        /// Matches <c>BaseResourceKey="Key"</c>, which resolves against the same dictionaries.
        /// </summary>
        private static readonly Regex BaseResourceKeyRegex = new Regex(
            @"BaseResourceKey\s*=\s*""(?<key>[^""]+)""",
            RegexOptions.Compiled);

        private static readonly Regex KeyDefinitionRegex = new Regex(
            @"x:Key\s*=\s*""(?<key>[^""]+)""",
            RegexOptions.Compiled);

        /// <summary>
        /// Style keys which MAUI itself registers in the application's resource dictionary, so they are
        /// legitimately referenced without being defined in this repository.
        /// </summary>
        private static readonly string[] FrameworkResourceKeys =
        {
            "BodyStyle",
            "CaptionStyle",
            "ListItemDetailTextStyle",
            "ListItemTextStyle",
            "SubtitleStyle",
            "TitleStyle"
        };

        [Fact]
        public void ShouldDefineEveryResourceKeyReferencedByTheLibrary()
        {
            AssertAllResourceKeysAreDefined(SourcePaths.LibraryDirectory);
        }

        [Fact]
        public void ShouldDefineEveryResourceKeyReferencedByTheDemoApp()
        {
            AssertAllResourceKeysAreDefined(SourcePaths.SamplesDirectory);
        }

        private static void AssertAllResourceKeysAreDefined(string directory)
        {
            // Arrange
            var definedKeys = GetDefinedKeys();
            var xamlFiles = SourcePaths.EnumerateXamlFiles(directory).ToArray();
            xamlFiles.Should().NotBeEmpty("the XAML files must be found for this test to mean anything");

            // Act
            var undefinedReferences = new List<string>();

            foreach (var xamlFile in xamlFiles)
            {
                var xaml = File.ReadAllText(xamlFile);
                var references = ResourceReferenceRegex.Matches(xaml)
                    .Concat(BaseResourceKeyRegex.Matches(xaml))
                    .Select(match => match.Groups["key"].Value)
                    .Distinct();

                undefinedReferences.AddRange(references
                    .Where(key => !definedKeys.Contains(key))
                    .Select(key => $"{Path.GetFileName(xamlFile)}: {key}"));
            }

            // Assert
            undefinedReferences.Should().BeEmpty(
                "every referenced resource key must be defined in XAML or in {0}; an undefined key fails silently at runtime",
                nameof(ThemeConstants));
        }

        /// <summary>
        /// Collects the keys defined in XAML, the keys the theme configuration writes into the application's
        /// resources at runtime, and the framework's own style keys.
        /// </summary>
        private static HashSet<string> GetDefinedKeys()
        {
            var definedKeys = new HashSet<string>(FrameworkResourceKeys, StringComparer.Ordinal);

            foreach (var directory in new[] { SourcePaths.LibraryDirectory, SourcePaths.SamplesDirectory })
            {
                foreach (var xamlFile in SourcePaths.EnumerateXamlFiles(directory))
                {
                    var xaml = File.ReadAllText(xamlFile);
                    definedKeys.UnionWith(KeyDefinitionRegex.Matches(xaml).Select(match => match.Groups["key"].Value));
                }
            }

            foreach (var themeConstant in GetThemeConstants(typeof(ThemeConstants)))
            {
                definedKeys.Add(themeConstant);
            }

            return definedKeys;
        }

        /// <summary>
        /// Returns every string constant declared by <see cref="ThemeConstants" /> and its nested classes.
        /// </summary>
        private static IEnumerable<string> GetThemeConstants(Type type)
        {
            var constants = type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue()!);

            return constants.Concat(type.GetNestedTypes().SelectMany(GetThemeConstants));
        }
    }
}
