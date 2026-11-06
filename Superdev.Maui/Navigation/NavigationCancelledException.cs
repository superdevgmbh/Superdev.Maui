namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// The exception which is thrown when a navigation was vetoed by a navigation guard.
    /// </summary>
    /// <remarks>
    /// This is an expected outcome rather than a defect. Log it at a lower severity than
    /// other <see cref="NavigationException"/>s.
    /// </remarks>
    /// <seealso cref="IConfirmNavigation"/>
    public class NavigationCancelledException : NavigationException
    {
        /// <inheritdoc cref="NavigationCancelledException"/>
        public NavigationCancelledException(string message)
            : base(message)
        {
        }
    }
}