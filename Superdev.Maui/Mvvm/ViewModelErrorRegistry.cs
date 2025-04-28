using System.Diagnostics;
using Superdev.Maui.Extensions;

namespace Superdev.Maui.Mvvm
{
    public class ViewModelErrorRegistry : IViewModelErrorRegistry, IViewModelErrorHandler
    {
        private readonly Dictionary<Func<Exception, bool>, Func<ViewModelError>> viewModelErrorFactories =
            new Dictionary<Func<Exception, bool>, Func<ViewModelError>>();

        private Func<Exception, ViewModelError> defaultViewModelErrorFactory;

        private static readonly Lazy<ViewModelErrorRegistry> Implementation = new Lazy<ViewModelErrorRegistry>(
            () => new ViewModelErrorRegistry(),
            LazyThreadSafetyMode.PublicationOnly);

        internal ViewModelErrorRegistry()
        {
        }

        public static ViewModelErrorRegistry Current => Implementation.Value;

        /// <inheritdoc />
        public void SetDefaultFactory(Func<Exception, ViewModelError> viewModelErrorFactory)
        {
            this.defaultViewModelErrorFactory = viewModelErrorFactory;
        }

        /// <inheritdoc />
        public void RegisterException(Func<Exception, bool> exceptionFilter, Func<ViewModelError> viewModelErrorFactory)
        {
            if (this.viewModelErrorFactories.ContainsKey(exceptionFilter))
            {
                this.viewModelErrorFactories.Remove(exceptionFilter);
            }

            this.viewModelErrorFactories.Add(exceptionFilter, viewModelErrorFactory);
        }

        /// <inheritdoc />
        public ViewModelError FromException(Exception exception)
        {
            var factories = this.viewModelErrorFactories
                .Where(f => exception.GetInnerExceptions().Any(e => f.Key(e)))
                .ToArray();

            if (factories.Length > 0)
            {
                Debug.WriteLineIf(factories.Length > 1, exception,
                    $"FromException found {factories.Length} viewModelErrorFactories for exception: {Environment.NewLine}" +
                    $"{exception}");

                var viewModelErrorFactory = factories.Last().Value;
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