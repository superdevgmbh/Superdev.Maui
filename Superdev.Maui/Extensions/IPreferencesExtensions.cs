using System.Text.Json;

namespace Superdev.Maui.Extensions
{
    public static class IPreferencesExtensions
    {
        /// <summary>
        /// Extends the <see cref="Microsoft.Maui.Storage.IPreferences"/> interface with the capability
        /// to store any object <typeparamref name="T"/> in the preferences.
        /// This method uses JsonSerializer (System.Text.Json) to serialize <paramref name="value"/>.
        /// </summary>
        public static void SetAsJson<T>(this Microsoft.Maui.Storage.IPreferences preferences, string key, T value, string? sharedName = null, JsonSerializerOptions? options = null)
        {
            var jsonString = JsonSerializer.Serialize(value, options);
            preferences.Set(key, jsonString, sharedName);
        }

        /// <summary>
        /// Extends the <see cref="Microsoft.Maui.Storage.IPreferences"/> interface with the capability
        /// to read objects of any type <typeparamref name="T"/> from preferences.
        /// This method uses JsonSerializer (System.Text.Json) to deserialize the value stored in <paramref name="key"/> into an object of type <typeparam name="T" />.
        /// </summary>
        public static T? GetFromJson<T>(this Microsoft.Maui.Storage.IPreferences preferences, string key, T? defaultValue = default, string? sharedName = null, JsonSerializerOptions? options = null)
        {
            var stringValue = preferences.Get<string>(key, null!, sharedName);
            if (string.IsNullOrEmpty(stringValue))
            {
                return defaultValue;
            }

            try
            {
                var result = JsonSerializer.Deserialize<T>(stringValue, options);
                return result;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}