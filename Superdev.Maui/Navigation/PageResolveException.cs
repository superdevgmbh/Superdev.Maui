namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// The exception which is thrown when a page cannot be resolved,
    /// e.g. if no page is registered for the given navigation name.
    /// </summary>
    public class PageResolveException : NavigationException
    {
        /// <inheritdoc cref="PageResolveException"/>
        public PageResolveException(string message)
            : base(message)
        {
        }

        /// <inheritdoc cref="PageResolveException"/>
        public PageResolveException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}