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
    }
}