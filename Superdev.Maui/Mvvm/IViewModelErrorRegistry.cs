namespace Superdev.Maui.Mvvm
{
    public interface IViewModelErrorRegistry
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IViewModelErrorHandler"/>.
        /// </summary>
        public static IViewModelErrorRegistry Current { get; } = ViewModelErrorRegistry.Current;

        /// <summary>
        /// The default <see cref="ViewModelError"/> factory to be used as last resort
        /// (if no matching factory can be found).
        /// </summary>
        /// <param name="viewModelErrorFactory"></param>
        void SetDefaultFactory(Func<Exception, ViewModelError> viewModelErrorFactory);

        /// <summary>
        /// Adds a mapping from a certain exception filtered in <paramref name="exceptionFilter"/>
        /// and constructs a <see cref="ViewModelError"/> if the exception matches.
        /// </summary>
        /// <param name="exceptionFilter">The exception filter which probes for a match.</param>
        /// <param name="viewModelErrorFactory">The factory method which constructs the <see cref="ViewModelError"/>.</param>
        void RegisterException(Func<Exception, bool> exceptionFilter, Func<ViewModelError> viewModelErrorFactory);

        /// <summary>
        /// Adds a mapping from a certain exception filtered in <paramref name="exceptionFilter"/>
        /// and constructs a <see cref="ViewModelError"/> if the exception matches.
        /// </summary>
        /// <param name="exceptionFilter">The exception filter which probes for a match.</param>
        /// <param name="viewModelErrorFactory">The factory method which constructs the <see cref="ViewModelError"/>.</param>
        /// <param name="priority">The priority of this exception filter. Higher values take precedence if multiple filters match.</param>
        public void RegisterException(Func<Exception, bool> exceptionFilter, Func<ViewModelError> viewModelErrorFactory, int priority);
    }
}