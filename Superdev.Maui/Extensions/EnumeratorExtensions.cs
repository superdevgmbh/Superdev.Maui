using System.Collections;
using Superdev.Maui.Extensions;

namespace Superdev.Maui.Extensions
{
    public static class EnumeratorExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator enumerator)
        {
            while (enumerator?.MoveNext() == true)
            {
                yield return (T)enumerator.Current;
            }
        }

        public static IEnumerable<T> ToArray<T>(this IEnumerator iterator)
        {
            return iterator.ToEnumerable<T>().ToArray();
        }

        public static IEnumerable<T> ToList<T>(this IEnumerator iterator)
        {
            return iterator.ToEnumerable<T>().ToList();
        }
    }
}