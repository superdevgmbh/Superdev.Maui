using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Superdev.Maui.Extensions;
using Superdev.Maui.Utils;

namespace Superdev.Maui.Mvvm
{
    public class ViewModelErrorRegistry : IViewModelErrorRegistry, IViewModelErrorHandler
    {
        private readonly ResettableLazy<ILogger<IViewModelErrorHandler>> lazyLogger;
        private ILogger<IViewModelErrorHandler> logger => this.lazyLogger.Value;

        private readonly Dictionary<Func<Exception, bool>, (int Priority, Func<ViewModelError> ViewModelErrorFactory)> viewModelErrorFactories = new Dictionary<Func<Exception, bool>, (int, Func<ViewModelError>)>();

        private Func<Exception, ViewModelError> defaultViewModelErrorFactory = ex => new ViewModelError(null, ex.Message, $"{ex}");

        private static readonly Lazy<ViewModelErrorRegistry> Implementation = new Lazy<ViewModelErrorRegistry>(
            () => new ViewModelErrorRegistry(),
            LazyThreadSafetyMode.PublicationOnly);

        private ViewModelErrorRegistry()
        {
            this.lazyLogger = new ResettableLazy<ILogger<IViewModelErrorHandler>>(() =>
            {
                try
                {
                    var serviceProvider = IPlatformApplication.Current.Services;
                    return serviceProvider.GetRequiredService<ILogger<IViewModelErrorHandler>>();
                }
                catch
                {
                    this.lazyLogger?.Reset();
                    return new NullLogger<IViewModelErrorHandler>();
                }
            }, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        internal ViewModelErrorRegistry(ILogger<IViewModelErrorHandler> logger)
        {
            this.lazyLogger = new ResettableLazy<ILogger<IViewModelErrorHandler>>(
                () => logger,
                LazyThreadSafetyMode.ExecutionAndPublication);
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
            var innerExceptionsWithDepth = exception.GetInnerExceptionsWithDepth().ToArray();

            var matches = this.viewModelErrorFactories
                .Select(factory =>
                {
                    var matchedExceptions = innerExceptionsWithDepth
                        .Where(e => factory.Key(e.Exception))
                        .ToList();

                    // MatchCount: fewer matches = more specific
                    var matchCount = matchedExceptions.Count;
                    var hasMatches = matchCount > 0;
                    var maxDepth = hasMatches ? matchedExceptions.Max(e => e.Depth) : 0;
                    return (Factory: factory, HasMatch: hasMatches, Depth: maxDepth, MatchCount: matchCount);
                })
                .Where(x => x.HasMatch)
                .ToList();

            if (matches.Count > 0)
            {
                // Order by Priority, then Depth, then Specificity
                var orderedMatches = matches
                    .OrderByDescending(x => x.Factory.Value.Priority)
                    .ThenByDescending(x => x.Depth)
                    .ThenBy(x => x.MatchCount)
                    .ToArray();

                this.logger.LogDebug(
                    $"FromException found {matches.Count} " +
                    $"{(matches.Count == 1 ? "viewModelErrorFactory" : "viewModelErrorFactories")} " +
                    $"for exception of type {exception.GetType().GetFormattedName()}" +
                    $"{(matches.Count == 1 ? "" : $"{Environment.NewLine}{string.Join(Environment.NewLine, orderedMatches.Select(m => $"> Priority={m.Factory.Value.Priority}, Depth={m.Depth}, MatchCount={m.MatchCount}"))}")}");

                var selectedMatch = orderedMatches.First();
                var viewModelError = selectedMatch.Factory.Value.ViewModelErrorFactory();
                return viewModelError;
            }

            if (this.defaultViewModelErrorFactory is Func<Exception, ViewModelError> defaultFactory)
            {
                return defaultFactory(exception);
            }

            throw new InvalidOperationException(
                $"Could not find a ViewModelError factory for exception of type {exception.GetType().Name}. " +
                $"Use methods {nameof(this.SetDefaultFactory)} and {nameof(this.RegisterException)} to register factories.");
        }
    }
}