using Microsoft.Extensions.Logging;
using Superdev.Maui.Extensions;

namespace Superdev.Maui.Mvvm
{
    public class ViewModelErrorRegistry : IViewModelErrorRegistry, IViewModelErrorHandler
    {
        private readonly ILogger logger;
        private readonly Dictionary<Func<Exception, bool>, (int Priority, Func<ViewModelError> ViewModelErrorFactory)> viewModelErrorFactories = new Dictionary<Func<Exception, bool>, (int, Func<ViewModelError>)>();

        private Func<Exception, ViewModelError> defaultViewModelErrorFactory = ex => new ViewModelError(null, ex.Message, $"{ex}");

        private static readonly Lazy<ViewModelErrorRegistry> Implementation = new Lazy<ViewModelErrorRegistry>(
            () => new ViewModelErrorRegistry(),
            LazyThreadSafetyMode.PublicationOnly);

        private ViewModelErrorRegistry()
            : this(IPlatformApplication.Current.Services.GetRequiredService<ILogger<IViewModelErrorHandler>>())
        {
        }

        internal ViewModelErrorRegistry(ILogger<IViewModelErrorHandler> logger)
        {
            this.logger = logger;
        }

        public static ViewModelErrorRegistry Current => Implementation.Value;

        /// <inheritdoc />
        public void SetDefaultFactory(Func<Exception, ViewModelError> viewModelErrorFactory)
        {
            this.defaultViewModelErrorFactory = viewModelErrorFactory ?? throw new ArgumentNullException(nameof(viewModelErrorFactory));
        }

        /// <inheritdoc />
        public void RegisterException(Func<Exception, bool> exceptionFilter, Func<ViewModelError> viewModelErrorFactory)
        {
            this.RegisterException(exceptionFilter, viewModelErrorFactory, priority: 0);
        }

        /// <inheritdoc />
        public void RegisterException(Func<Exception, bool> exceptionFilter, Func<ViewModelError> viewModelErrorFactory, int priority)
        {
            this.viewModelErrorFactories.Remove(exceptionFilter);
            this.viewModelErrorFactories.Add(exceptionFilter, (priority, viewModelErrorFactory));
        }

        /// <inheritdoc />
        public ViewModelError FromException(Exception exception)
        {
            var factories = this.viewModelErrorFactories
                .Where(f => exception.GetInnerExceptions().Any(e => f.Key(e)))
                .ToArray();

            if (factories.Length > 0)
            {
                this.logger.LogDebug($"FromException found {factories.Length} {(factories.Length == 1 ? "viewModelErrorFactory": "viewModelErrorFactories")} " +
                                     $"for exception of type {exception.GetType().GetFormattedName()}");

                var viewModelErrorFactory = factories.OrderByDescending(f => f.Value.Priority).First().Value.ViewModelErrorFactory;
                var viewModelError = viewModelErrorFactory();
                return viewModelError;
            }

            if (this.defaultViewModelErrorFactory is Func<Exception, ViewModelError> defaultFactory)
            {
                var viewModelError = defaultFactory(exception);
                return viewModelError;
            }

            throw new InvalidOperationException(
                $"Could not find a ViewModelError factory for exception of type {exception.GetType().Name}. " +
                $"Use methods {nameof(this.SetDefaultFactory)} and {nameof(this.RegisterException)} to register factories.");
        }
    }
}