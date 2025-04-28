namespace Superdev.Maui.Mvvm
{
    public interface IViewModelErrorHandler
    {
        /// <summary>
        /// Attempts to map <paramref name="exception"/> to one of the registered error messages.
        /// </summary>
        /// <remarks>
        /// Use <seealso cref="IViewModelErrorRegistry.RegisterException"/> to map exceptions to translated error messages.
        /// </remarks>
        /// <param name="exception"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// If no ViewModelError factory can be found for a given <paramref name="exception"/>.
        /// Use <seealso cref="IViewModelErrorRegistry.SetDefaultFactory"/> to avoid this exception.
        /// </exception>
        ViewModelError FromException(Exception exception);
    }
}