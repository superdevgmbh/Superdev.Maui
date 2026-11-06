namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Receives a callback every time the page becomes the active page,
    /// after the platform transition has completed.
    /// </summary>
    public interface INavigatedTo
    {
        /// <summary>
        /// Called when the page became the active page.
        /// </summary>
        /// <param name="navigationDirection">
        /// <see cref="NavigationDirection.Forward"/> if the page was newly navigated to,
        /// <see cref="NavigationDirection.Back"/> if the page was revealed by a backwards navigation.
        /// </param>
        Task NavigatedToAsync(NavigationDirection navigationDirection);
    }

    /// <summary>
    /// Receives a callback every time the page becomes the active page and a compatible
    /// navigation parameter or back navigation result is present.
    /// </summary>
    /// <typeparam name="TParameter">
    /// The type of the navigation parameter. The parameter is delivered if the value passed to the
    /// navigation call is assignable to this type.
    /// </typeparam>
    public interface INavigatedTo<in TParameter>
    {
        /// <summary>
        /// Called when the page became the active page and a compatible parameter is present.
        /// </summary>
        /// <param name="navigationDirection">
        /// <see cref="NavigationDirection.Forward"/> carries the parameter of the forward navigation,
        /// <see cref="NavigationDirection.Back"/> carries the result passed to the pop operation.
        /// </param>
        /// <param name="parameter">The navigation parameter or the back navigation result.</param>
        Task NavigatedToAsync(NavigationDirection navigationDirection, TParameter parameter);
    }
}
