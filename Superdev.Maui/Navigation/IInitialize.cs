namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Receives a one-shot initialization callback.
    /// The callback is invoked exactly once per page instance, after the page and its view model
    /// have been created, and before the page becomes visible.
    /// </summary>
    /// <remarks>
    /// The callback is awaited before the platform transition starts, so a long-running operation
    /// delays the transition. Keep it fast and move awaited I/O to <see cref="INavigatedTo"/>.
    /// </remarks>
    public interface IInitialize
    {
        /// <summary>
        /// Initializes the view or view model.
        /// </summary>
        Task InitializeAsync();
    }

    /// <summary>
    /// Receives a one-shot initialization callback together with the navigation parameter.
    /// </summary>
    /// <typeparam name="TParameter">
    /// The type of the navigation parameter. The parameter is delivered if the value passed to the
    /// navigation call is assignable to this type.
    /// </typeparam>
    /// <remarks>
    /// If a navigation parameter is present and a compatible typed implementation exists,
    /// only <see cref="InitializeAsync(TParameter)"/> is invoked and <see cref="IInitialize.InitializeAsync"/>
    /// is skipped. If no parameter is passed, the parameterless callback is invoked instead (if implemented).
    /// </remarks>
    public interface IInitialize<in TParameter>
    {
        /// <summary>
        /// Initializes the view or view model with the given <paramref name="parameter"/>.
        /// </summary>
        /// <param name="parameter">The navigation parameter.</param>
        Task InitializeAsync(TParameter parameter);
    }
}
