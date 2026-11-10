using System.Reflection;

namespace Superdev.Maui.Tests.Resources
{
    /// <summary>
    /// Locates the source directories of this repository, for the tests which inspect source files rather
    /// than compiled behaviour.
    /// </summary>
    /// <remarks>
    /// The paths are baked in by the test project at build time rather than guessed by walking up from the
    /// output directory, so a test which cannot find the sources says so instead of silently passing on an
    /// empty file set.
    /// </remarks>
    internal static class SourcePaths
    {
        public static string LibraryDirectory { get; } = GetDirectory("LibraryDirectory");

        public static string SamplesDirectory { get; } = GetDirectory("SamplesDirectory");

        /// <summary>
        /// Returns every XAML file below <paramref name="directory" />, skipping build output.
        /// </summary>
        public static IEnumerable<string> EnumerateXamlFiles(string directory)
        {
            return Directory
                .EnumerateFiles(directory, "*.xaml", SearchOption.AllDirectories)
                .Where(path => !IsBuildOutput(path));
        }

        private static bool IsBuildOutput(string path)
        {
            var relativePath = Path.GetRelativePath(Path.GetPathRoot(path) ?? string.Empty, path);
            return relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(segment => segment is "obj" or "bin");
        }

        private static string GetDirectory(string key)
        {
            var value = typeof(SourcePaths).Assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(a => a.Key == key)?.Value;

            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException(
                    $"The assembly metadata '{key}' is missing. It is written by Superdev.Maui.Tests.csproj.");
            }

            var directory = Path.GetFullPath(value);
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException(
                    $"The source directory '{directory}' from assembly metadata '{key}' does not exist.");
            }

            return directory;
        }
    }
}
