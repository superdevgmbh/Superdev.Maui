using System.Text.Json;

namespace Superdev.Maui.Services
{
    public interface IPreferences
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IPreferences"/>.
        /// </summary>
        public static IPreferences Current => Preferences.Current;

        /// <summary>
		/// Checks for the existence of a given key.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <param name="sharedName">Shared container name.</param>
		/// <returns><see langword="true"/> if the key exists in the preferences, otherwise <see langword="false"/>.</returns>
		bool ContainsKey(string key, string? sharedName = null);

        /// <summary>
        /// Removes a key and its associated value if it exists.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <param name="sharedName">Shared container name.</param>
        void Remove(string key, string? sharedName = null);

        /// <summary>
        /// Clears all keys and values.
        /// </summary>
        /// <param name="sharedName">Shared container name.</param>
        void Clear(string? sharedName = null);

        /// <summary>
        /// Sets a value for a given key.
        /// </summary>
        /// <typeparam name="T">Type of the object that is stored in this preference.</typeparam>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="sharedName">Shared container name.</param>
        void Set<T>(string key, T value);

        /// <summary>
        /// Sets a value for a given key.
        /// </summary>
        /// <typeparam name="T">Type of the object that is stored in this preference.</typeparam>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">Value to set.</param>
        void Set<T>(string key, T? value, string? sharedName = null, JsonSerializerOptions? options = null);

        /// <summary>
        /// Gets the value for a given key, or the default specified if the key does not exist.
        /// </summary>
        /// <typeparam name="T">The type of the object stored for this preference.</typeparam>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return when no existing value for <paramref name="key"/> exists.</param>
        /// <returns>Value for the given key, or the value in <paramref name="defaultValue"/> if it does not exist.</returns>
        T? Get<T>(string key, T? defaultValue = default);

        /// <summary>
        /// Gets the value for a given key, or the default specified if the key does not exist.
        /// </summary>
        /// <typeparam name="T">The type of the object stored for this preference.</typeparam>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return when no existing value for <paramref name="key"/> exists.</param>
        /// <param name="sharedName">Shared container name.</param>
        /// <returns>Value for the given key, or the value in <paramref name="defaultValue"/> if it does not exist.</returns>
        T? Get<T>(string key, T? defaultValue = default, string? sharedName = null, JsonSerializerOptions? options = null);
    }
}