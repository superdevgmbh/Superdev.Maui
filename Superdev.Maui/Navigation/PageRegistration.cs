namespace Superdev.Maui.Navigation
{
    internal class PageRegistration
    {
        public required Type PageType { get; init; }

        public Type? ViewModelType { get; init; }

        public string? Name { get; init; }
    }
}