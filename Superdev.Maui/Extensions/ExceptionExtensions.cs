using System.ComponentModel;

namespace Superdev.Maui.Extensions
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Recursively enumerates all <see cref="Exception.InnerException"/> and <see cref="AggregateException.InnerExceptions"/>.
        /// </summary>
        /// <param name="exception">The source exception.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<Exception> GetInnerExceptions(this Exception exception)
        {
            if (exception == null)
            {
                yield break;
            }

            yield return exception;

            if (exception is AggregateException aggregateException)
            {
                foreach (var aggregateInnerException in aggregateException.InnerExceptions)
                {
                    foreach (var ex in GetInnerExceptions(aggregateInnerException))
                    {
                        yield return ex;
                    }
                }
            }
            else if (exception.InnerException is Exception innerException)
            {
                foreach (var ex in GetInnerExceptions(innerException))
                {
                    yield return ex;
                }
            }
        }

        /// <summary>
        /// Recursively enumerates all <see cref="Exception.InnerException"/> and
        /// <see cref="AggregateException.InnerExceptions"/> along with their nesting depth.
        /// </summary>
        /// <param name="exception">The source exception.</param>
        /// <returns>An enumerable of (Exception, int).</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<(Exception Exception, int Depth)> GetInnerExceptionsWithDepth(this Exception exception)
        {
            return GetInnerExceptionsWithDepth(exception, 0);
        }

        private static IEnumerable<(Exception Exception, int Depth)> GetInnerExceptionsWithDepth(Exception exception, int depth)
        {
            if (exception == null)
            {
                yield break;
            }

            yield return (exception, depth);

            if (exception is AggregateException aggregateException)
            {
                foreach (var aggregateInnerException in aggregateException.InnerExceptions)
                {
                    foreach (var ex in GetInnerExceptionsWithDepth(aggregateInnerException, depth + 1))
                    {
                        yield return ex;
                    }
                }
            }
            else if (exception.InnerException is Exception innerException)
            {
                foreach (var ex in GetInnerExceptionsWithDepth(innerException, depth + 1))
                {
                    yield return ex;
                }
            }
        }
    }
}