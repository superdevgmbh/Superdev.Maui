namespace Superdev.Maui.Services
{
    /// <summary>
    /// A static implementation of <see cref="IDateTime"/> used exclusively for unit testing.
    /// Allows for deterministic and repeatable tests by providing a fixed point in time.
    /// </summary>
    public class StaticDateTime : IDateTime
    {
        public StaticDateTime(DateTime dateTime)
        {
            this.Now = dateTime;
            this.UtcNow = dateTime.ToUniversalTime();
        }

        public DateTime Now { get; }

        public DateTime UtcNow { get; }
    }
}