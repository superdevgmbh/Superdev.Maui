namespace Superdev.Maui.Tests.TestData
{
    /// <summary>
    /// Tests which change the culture of the whole process, as the localizer does, belong to this collection, so they
    /// never run in parallel with other tests.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class ProcessCultureCollection
    {
        public const string Name = "Process culture";
    }
}
