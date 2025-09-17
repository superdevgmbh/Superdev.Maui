using System.Diagnostics;
using Superdev.Maui.Extensions;

namespace Superdev.Maui.Resources.Styles
{
    public static class ResourceDictionaryExtensions
    {
        /// <summary>
        ///     Gets a resource of the specified type from the current <see cref="Application.Resources" />.
        /// </summary>
        /// <typeparam name="T">The type of the resource object to be retrieved.</typeparam>
        /// <param name="resourceDictionary">The resource dictionary.</param>
        /// <param name="key">
        ///     The key of the resource object. For a list of CrossPlatformLibrary resource keys, see
        ///     <see cref="ThemeConstants" />.
        /// </param>
        /// <exception cref="InvalidCastException" />
        /// <exception cref="ArgumentNullException" />
        public static T GetValue<T>(this ResourceDictionary resourceDictionary, string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            if (resourceDictionary.TryGetValue(key, out var value))
            {
                if (value is T resource)
                {
                    return resource;
                }

                if (value != null)
                {
                    throw new InvalidCastException($"Resource with key='{key}' was not of the type {typeof(T).GetFormattedName()}.");
                }
            }

            throw new InvalidOperationException($"{typeof(T).GetFormattedName()} with key='{key}' could not be found.");
        }

        public static void SetValue(this ResourceDictionary resourceDictionary, string key, object value)
        {
            if (resourceDictionary == null || key == null || value == null)
            {
                return;
            }

#if DEBUG
            if (resourceDictionary.TryGetValue(key, out _))
            {
                Debug.WriteLine($"SetValue: key='{key}' already exists");
            }
#endif

            resourceDictionary[key] = value;
        }
    }
}