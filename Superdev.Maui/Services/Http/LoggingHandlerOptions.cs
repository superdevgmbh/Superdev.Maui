namespace Superdev.Maui.Services.Http
{
    /// <summary>
    /// Configures what <see cref="LoggingHandler" /> writes to the log.
    /// </summary>
    public class LoggingHandlerOptions
    {
        /// <summary>
        /// Media types whose content is logged inline as text. Everything else
        /// (PDFs, images, octet-streams, …) is logged as a <c>{Type, Size}</c> placeholder.
        /// <para>Each entry is matched case-insensitively and can be:</para>
        /// <list type="bullet">
        ///   <item><description>an exact media type, e.g. <c>application/json</c>;</description></item>
        ///   <item><description>a prefix wildcard, e.g. <c>text/*</c>;</description></item>
        ///   <item><description>a structured-suffix wildcard, e.g. <c>*+json</c> or <c>*+xml</c>.</description></item>
        /// </list>
        /// <para>Default: <c>application/json</c>, <c>application/problem+json</c>.</para>
        /// </summary>
        public IList<string> SupportedMediaTypes { get; set; } = new List<string>
        {
            "application/json",
            "application/problem+json"
        };

        /// <summary>
        /// Header names that are omitted from the log output entirely.
        /// Matching is case-insensitive.
        /// <para>Default: <c>Server</c>.</para>
        /// </summary>
        public IList<string> FilteredHeaderNames { get; set; } = new List<string>
        {
            "Server",
        };

        /// <summary>
        /// Header names whose values are replaced with <c>{suppressed}</c> in the log output
        /// (e.g. to keep secrets out of the logs). Matching is case-insensitive.
        /// <para>Default: <c>Authorization</c>.</para>
        /// </summary>
        public IList<string> SuppressedHeaderNames { get; set; } = new List<string>
        {
            "Authorization",
        };
    }
}
