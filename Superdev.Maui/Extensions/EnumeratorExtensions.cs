using System.Collections;

namespace Superdev.Maui.Extensions
{
    public static class EnumeratorExtensions
    {
        public static IEnumerable<T?> ToEnumerable<T>(this IEnumerator? enumerator)
        {
            if (enumerator is null)
            {
                yield break;
            }

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current is T item ? item : default;
            }
        }

        public static T?[] ToArray<T>(this IEnumerator? enumerator)
        {
            return enumerator.ToEnumerable<T>().ToArray();
        }

        public static List<T?> ToList<T>(this IEnumerator? enumerator)
        {
            return enumerator.ToEnumerable<T>().ToList();
        }
    }
}