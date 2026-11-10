using System.Text.RegularExpressions;

namespace Superdev.Maui.Tests.Xaml
{
    /// <summary>
    /// The one part of the validation surface which stays stringly typed is the property name in XAML:
    /// <c>Validation.Errors[UserName]</c>, <c>Validation.Fields[UserName]</c>, <c>s:Validation.PropertyName="UserName"</c>.
    /// Nothing checks it at compile time, so this test does: every such name in the library's and the demo's XAML
    /// must be a property with a rule on a view model the page declares as its <c>x:DataType</c>.
    /// </summary>
    public partial class XamlValidationBindingsTests
    {
        [GeneratedRegex(@"Validation\.(?:Errors|Fields)\[(\w+)\]|(?<![\w.])Errors\[(\w+)\]|Validation\.PropertyName=""(\w+)""|<s:ValidationErrorLabel[^>]*?\sPropertyName=""(\w+)""")]
        private static partial Regex BindingRegex();

        [GeneratedRegex(@"x:DataType=""(?:\w+:)?(\w+)""")]
        private static partial Regex DataTypeRegex();

        [Fact]
        public void EveryValidationBindingInXaml_NamesAPropertyWithARule()
        {
            // Arrange
            var root = FindRepositoryRoot();
            var sources = LoadViewModelSources(root);
            var xamlFiles = new[] { "Superdev.Maui", Path.Combine("Samples", "SuperdevMauiDemoApp") }
                .SelectMany(dir => Directory.EnumerateFiles(Path.Combine(root, dir), "*.xaml", SearchOption.AllDirectories))
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                .ToArray();

            var violations = new List<string>();

            // Act
            foreach (var file in xamlFiles)
            {
                var xaml = File.ReadAllText(file);
                var viewModels = DataTypeRegex().Matches(xaml).Select(m => m.Groups[1].Value).Distinct().ToArray();

                foreach (System.Text.RegularExpressions.Match match in BindingRegex().Matches(xaml))
                {
                    var propertyName = match.Groups.Cast<Group>().Skip(1).First(g => g.Success).Value;
                    var line = xaml[..match.Index].Count(c => c == '\n') + 1;
                    var relative = Path.GetRelativePath(root, file);

                    if (viewModels.Length == 0)
                    {
                        violations.Add($"{relative}:{line} binds '{propertyName}' but declares no x:DataType, so nothing says which view model owns it");
                        continue;
                    }

                    var declared = viewModels.Any(vm => sources.TryGetValue(vm, out var source) && DeclaresRule(source, propertyName));
                    if (!declared)
                    {
                        violations.Add($"{relative}:{line} binds '{propertyName}' but none of [{string.Join(", ", viewModels)}] declares a rule for it");
                    }
                }
            }

            // Assert
            violations.Should().BeEmpty("a validation binding without a rule never shows anything; add the rule or remove the binding:\n" + string.Join("\n", violations));
            xamlFiles.Should().NotBeEmpty();
        }

        [Fact]
        public void EveryRuleOfADemoViewModel_HasAnInputBoundOnItsPage()
        {
            // Arrange - the other direction: a rule nobody can satisfy on the page blocks every save
            var root = FindRepositoryRoot();
            var classBodies = LoadClassBodies(Path.Combine(root, "Samples", "SuperdevMauiDemoApp"));
            var demo = Path.Combine(root, "Samples", "SuperdevMauiDemoApp");
            var pages = Directory.EnumerateFiles(demo, "*.xaml", SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                .ToArray();

            var violations = new List<string>();

            // Act
            foreach (var page in pages)
            {
                var xaml = File.ReadAllText(page);
                var viewModels = DataTypeRegex().Matches(xaml).Select(m => m.Groups[1].Value).Distinct();

                foreach (var viewModel in viewModels)
                {
                    if (!classBodies.TryGetValue(viewModel, out var body))
                    {
                        continue;
                    }

                    foreach (var propertyName in DeclaredRules(body))
                    {
                        if (!Regex.IsMatch(xaml, $@"\{{Binding\s+(?:Path=)?{Regex.Escape(propertyName)}\b"))
                        {
                            violations.Add($"{Path.GetRelativePath(root, page)}: {viewModel} has rules for '{propertyName}', but no input on the page is bound to it");
                        }
                    }
                }
            }

            // Assert
            violations.Should().BeEmpty("the user could never correct such a field:\n" + string.Join("\n", violations));
            pages.Should().NotBeEmpty();
        }

        private static IEnumerable<string> DeclaredRules(string source)
        {
            return Regex.Matches(source, @"\.Property\(\s*\(\)\s*=>\s*(?:\w+\s*\.\s*)?(\w+)\s*\)")
                .Concat(Regex.Matches(source, @"\.Property\(\s*nameof\(\s*(?:this\.)?(\w+)\s*\)"))
                .Select(m => m.Groups[1].Value)
                .Distinct(StringComparer.Ordinal);
        }

        /// <summary>
        /// The text of each class, from its declaration to the next one in the same file, so that the rules of one
        /// view model are not attributed to another declared next to it.
        /// </summary>
        private static Dictionary<string, string> LoadClassBodies(string directory)
        {
            var bodies = new Dictionary<string, string>(StringComparer.Ordinal);
            var classRegex = new Regex(@"\bclass\s+(\w+)\b");

            foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                {
                    continue;
                }

                var content = File.ReadAllText(file);
                var declarations = classRegex.Matches(content);
                for (var i = 0; i < declarations.Count; i++)
                {
                    var start = declarations[i].Index;
                    var end = i + 1 < declarations.Count ? declarations[i + 1].Index : content.Length;
                    bodies[declarations[i].Groups[1].Value] = content[start..end];
                }
            }

            return bodies;
        }

        private static bool DeclaresRule(string source, string propertyName)
        {
            var name = Regex.Escape(propertyName);
            return Regex.IsMatch(source, $@"\.Property\(\s*\(\)\s*=>\s*(?:\w+\s*\.\s*)?{name}\s*\)")
                || Regex.IsMatch(source, $@"\.Property\(\s*nameof\(\s*(?:this\.)?{name}\s*\)")
                || Regex.IsMatch(source, $@"\.Property\(\s*""{name}""");
        }

        private static Dictionary<string, string> LoadViewModelSources(string root)
        {
            var sources = new Dictionary<string, string>(StringComparer.Ordinal);
            var classRegex = new Regex(@"\bclass\s+(\w+)\b");

            foreach (var dir in new[] { "Superdev.Maui", Path.Combine("Samples", "SuperdevMauiDemoApp") })
            {
                foreach (var file in Directory.EnumerateFiles(Path.Combine(root, dir), "*.cs", SearchOption.AllDirectories))
                {
                    if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                    {
                        continue;
                    }

                    var content = File.ReadAllText(file);
                    foreach (System.Text.RegularExpressions.Match match in classRegex.Matches(content))
                    {
                        var className = match.Groups[1].Value;
                        sources[className] = sources.TryGetValue(className, out var existing) ? existing + "\n" + content : content;
                    }
                }
            }

            return sources;
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Superdev.Maui.slnx")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? throw new InvalidOperationException("The repository root (Superdev.Maui.slnx) was not found above the test directory.");
        }
    }
}
