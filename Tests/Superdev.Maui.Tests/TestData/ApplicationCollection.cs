namespace Superdev.Maui.Tests.TestData
{
    /// <summary>
    /// Tests which replace the global <c>Application.Current</c> or <c>IPlatformApplication.Current</c> belong to this
    /// collection, so they never run in parallel with other tests.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class ApplicationCollection
    {
        public const string Name = "Application.Current";
    }
}
