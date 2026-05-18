using System.Collections;
using System.Collections.ObjectModel;
using System.Net;
using Superdev.Maui.Extensions;
using Superdev.Maui.Internals;

namespace Superdev.Maui.Extensions
{
    public static partial class EnumerableExtensions
    {
        private static readonly Random Rng = new Random();

        public static IList CreateList(this IEnumerable enumerable)
        {
            ArgumentNullException.ThrowIfNull(enumerable);

            var list = new Collection<object>();

            foreach (var item in enumerable)
            {
                list.Add(item);
            }

            return list;
        }

        public static void Sort<TSource, TKey>(this ICollection<TSource> source, Func<TSource, TKey> keySelector)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));

            IList<TSource> sortedList = source.OrderBy(keySelector).ToList();
            source.Clear();
            foreach (var sortedItem in sortedList)
            {
                source.Add(sortedItem);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(action);

            foreach (var item in source)
            {
                action(item);
            }
        }

        /// <summary>
        ///     To the observable collection.
        /// </summary>
        /// <typeparam name="T">Generic type T.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns>The resulting ObservableCollection&lt;T&gt;.</returns>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ObservableCollection<T>(enumerable);
        }

        /// <summary>
        ///     Adds a collection of <typeparamref name="T" /> to the given list <paramref name="list" />.
        /// </summary>
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
        {
            ArgumentNullException.ThrowIfNull(list);
            ArgumentNullException.ThrowIfNull(collection);

            foreach (var item in collection)
            {
                list.Add(item);
            }
        }

        /// <summary>
        ///     Updates all items in the specified source which match with selectorFunc with the specified updateAction.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selectorFunc">The selector function.</param>
        /// <param name="updateAction">The update action.</param>
        public static void Update<T>(this IEnumerable<T> source, Func<T, bool> selectorFunc, Action<T> updateAction)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selectorFunc);
            ArgumentNullException.ThrowIfNull(updateAction);

            foreach (var item in source.Where(selectorFunc))
            {
                updateAction(item);
            }
        }

        /// <summary>
        ///     Updates a single item in the given source using the selector function to find the item
        ///     and the update action to update it accordingly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selectorFunc">The selector function.</param>
        /// <param name="updateAction">The update action.</param>
        public static void UpdateSingle<T>(this IEnumerable<T> source, Func<T, bool> selectorFunc, Action<T> updateAction)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(selectorFunc, nameof(selectorFunc));
            ArgumentNullException.ThrowIfNull(updateAction, nameof(updateAction));

            var selected = source.Single(selectorFunc);
            updateAction(selected);
        }

        /// <summary>
        ///     Determines whether the specified search list contains duplicates.
        /// </summary>
        /// <typeparam name="T">The generic type T.</typeparam>
        /// <typeparam name="TResult">The type of the T result.</typeparam>
        /// <param name="searchList">The search list.</param>
        /// <param name="selectionCriteria">The selection criteria.</param>
        /// <returns><c>true</c> if the specified search list contains duplicates; otherwise, <c>false</c>.</returns>
        public static bool AnyDuplicates<T, TResult>(this IEnumerable<T> searchList, Func<T, TResult> selectionCriteria)
        {
            ArgumentNullException.ThrowIfNull(searchList);

            return searchList.Select(selectionCriteria)
                .GroupBy(x => x)
                .Where(y => y.Count() > 1)
                .Select(z => z.Key)
                .Any();
        }

        /// <summary>
        ///     Returns the last object of source enumerable.
        /// </summary>
        /// <exception cref="ArgumentNullException">The source enumerable is null.</exception>
        /// <exception cref="InvalidOperationException">The source enumerable does not contain any elements.</exception>
        public static object Last(this IEnumerable source)
        {
            ArgumentNullException.ThrowIfNull(source);

            var lastOrDefault = source.LastOrDefault();
            if (lastOrDefault != null)
            {
                return lastOrDefault;
            }

            throw new InvalidOperationException("The source enumerable does not contain any elements.");
        }

        /// <summary>
        ///     Returns the last object of source enumerable.
        ///     If there are no items in source enumerable, it returns null.
        /// </summary>
        /// <exception cref="ArgumentNullException">The source enumerable is null.</exception>
        /// <exception cref="InvalidOperationException">The source enumerable does not contain any elements.</exception>
        public static object? LastOrDefault(this IEnumerable source)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (source is IList list)
            {
                var count = list.Count;
                if (count > 0)
                {
                    return list[count - 1];
                }
            }
            else
            {
                var e = source.GetEnumerator();
                using (e as IDisposable)
                {
                    if (e.MoveNext())
                    {
                        object result;
                        do
                        {
                            result = e.Current;
                        } while (e.MoveNext());

                        return result;
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Appends element <paramref name="item" /> to enumerable <paramref name="source" />.
        /// </summary>
        public static IEnumerable<T> Add<T>(this IEnumerable<T> source, T item)
        {
            ArgumentNullException.ThrowIfNull(source);

            foreach (var sourceItem in source)
            {
                yield return sourceItem;
            }

            yield return item;
        }

        /// <summary>
        ///     Returns the number of items in <paramref name="enumerable" />.
        /// </summary>
        public static int GetCount(this IEnumerable enumerable)
        {
            ArgumentNullException.ThrowIfNull(enumerable);

            var enumerator = enumerable.GetEnumerator();
            var num = 0;
            while (enumerator.MoveNext())
            {
                ++num;
            }

            return num;
        }

        public static T RandomElement<T>(this IEnumerable<T> source)
        {
            return source.RandomElement(Rng);
        }

        public static T RandomElement<T>(this IEnumerable<T> source, Random rng)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(rng);

            T? current = default;
            var count = 0;

            foreach (var element in source)
            {
                count++;
                if (rng.Next(count) == 0)
                {
                    current = element;
                }
            }

            if (count == 0)
            {
                throw new InvalidOperationException("Sequence was empty");
            }

            // Here current is guaranteed to be assigned by the reservoir algorithm.
            return current!;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(Rng);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            ArgumentNullException.ThrowIfNull(source);

            var elements = source.ToArray();
            for (var i = elements.Length - 1; i >= 0; i--)
            {
                var swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

        /// <summary>
        ///     Finds duplicates in a given collection <seealso cref="source" />.
        /// </summary>
        /// <typeparam name="T">The collection item type.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="propertySelector">Property selector.</param>
        /// <param name="numberOfDuplicates">The least number of duplicates.</param>
        /// <returns></returns>
        public static IEnumerable<T> FindDuplicates<T>(this IEnumerable<T> source, Func<T, object> propertySelector, int numberOfDuplicates = 2)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(propertySelector);

            var skip = numberOfDuplicates - 1;
            return source
                .GroupBy(propertySelector)
                .Where(g => g.Skip(skip).Any())
                .SelectMany(g => g);
        }

        /// <summary>
        ///     Concatenates all <paramref name="parameters" /> into a ampersand-separated string, e.g. param1=value1&param2=value2
        /// </summary>
        /// <param name="parameters">List of parameters and values.</param>
        /// <returns>Ampersand-separated URI parameter string.</returns>
        public static string ToQueryString(this IEnumerable<KeyValuePair<string, string>> parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);

            return string.Join("&", parameters.Select(p => $"{WebUtility.UrlEncode(p.Key)}={WebUtility.UrlEncode(p.Value)}"));
        }

        public static T? FirstOrDefault<T>(this IEnumerable source)
        {
            ArgumentNullException.ThrowIfNull(source);

            foreach (var item in source.OfType<T>())
            {
                return item;
            }

            return default;
        }

        /// <summary>
        ///     Returns the single element if the sequence contains exactly one element; otherwise <c>default</c>.
        /// </summary>
        public static T? SingleOrNone<T>(this IEnumerable<T>? source)
        {
            if (source is null)
            {
                return default;
            }

            using var enumerator = source.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return default;
            }

            var item = enumerator.Current;

            return enumerator.MoveNext()
                ? default
                : item;
        }

        public static T[] Replace<T>(this T[] list, T oldItem, T newItem)
        {
            ArgumentNullException.ThrowIfNull(list);

            list.Replace(i => Equals(i, oldItem), newItem);
            return list;
        }

        public static void Replace<T>(this T[] list, Predicate<T> oldItemSelector, T newItem)
        {
            ArgumentNullException.ThrowIfNull(list);
            ArgumentNullException.ThrowIfNull(oldItemSelector);
            ArgumentNullException.ThrowIfNull(newItem);

            //check for different situations here and throw exception
            //if list contains multiple items that match the predicate
            //or check for nullability of list and etc ...
            var oldItemIndex = Array.FindIndex(list, oldItemSelector);
            list[oldItemIndex] = newItem;
        }

        /// <summary>
        ///     Removes all items from <paramref name="source" /> which match with the given <paramref name="predicate" />.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="predicate">The condition for which items will be removed from the source collection.</param>
        /// <typeparam name="T">Generic type T.</typeparam>
        /// <returns>The number of removed items.</returns>
        public static int RemoveBy<T>(this ICollection<T> source, Func<T, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            var itemsToRemove = source.Where(predicate).ToList();
            var removedCount = 0;
            foreach (var itemToRemove in itemsToRemove)
            {
                source.Remove(itemToRemove);
                removedCount++;
            }

            return removedCount;
        }
    }
}
