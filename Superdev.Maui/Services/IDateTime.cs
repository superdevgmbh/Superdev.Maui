namespace Superdev.Maui.Services
{
    public interface IDateTime
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IDateTime"/>.
        /// </summary>
        public static IDateTime Current => SystemDateTime.Current;

        DateTime Now { get; }

        DateTime UtcNow { get; }
    }
}