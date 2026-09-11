namespace Superdev.Maui.Services
{
    public class SystemDateTime : IDateTime
    {
        private static readonly Lazy<IDateTime> Implementation = new Lazy<IDateTime>(() => new SystemDateTime(), LazyThreadSafetyMode.PublicationOnly);

        public static IDateTime Current => Implementation.Value;

        private SystemDateTime()
        {
        }

        public DateTime Now => DateTime.Now;

        public DateTime UtcNow => DateTime.UtcNow;
    }
}