namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// The exception which is thrown when a navigation parameter cannot be delivered,
    /// e.g. if the target implements no compatible <see cref="IInitialize{TParameter}"/>
    /// or <see cref="INavigatedTo{TParameter}"/>, or if the target is ambiguous.
    /// </summary>
    /// <remarks>
    /// This exception is thrown before the navigation stack is modified.
    /// </remarks>
    public class NavigationParameterException : NavigationException
    {
        /// <inheritdoc cref="NavigationParameterException"/>
        public NavigationParameterException(string message)
            : base(message)
        {
        }
    }
}