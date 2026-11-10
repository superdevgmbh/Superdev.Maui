namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// The exception which is thrown when a navigation operation fails.
    /// </summary>
    /// <remarks>
    /// Navigation operations report failures by throwing. If a navigation method returns normally,
    /// the navigation was performed. Exceptions thrown by lifecycle callbacks of views and view models
    /// are not wrapped; they propagate to the caller unchanged.
    /// </remarks>
    public class NavigationException : Exception
    {
        /// <inheritdoc cref="NavigationException"/>
        public NavigationException(string message)
            : base(message)
        {
        }

        /// <inheritdoc cref="NavigationException"/>
        public NavigationException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets the name of the page involved in the failed navigation, if known.
        /// </summary>
        public string? PageName { get; init; }
    }
}
