using System.Text.Json;

namespace Superdev.Maui.Extensions
{
    public static class IPreferencesExtensions
    {
        /// <summary>
        /// Extends the <see cref="Microsoft.Maui.Storage.IPreferences"/> interface with the capability
        /// to store any object <typeparamref name="T"/> in the preferences.
        /// This method uses the .NET json serializer to serialize <paramref name="value"/>.
        /// </summary>
        public static void SetAsJson<T>(this Microsoft.Maui.Storage.IPreferences preferences, string key, T value, string? sharedName = null, JsonSerializerOptions? options = null)
        {
            var jsonString = JsonSerializer.Serialize(value, options);
            preferences.Set(key, jsonString, sharedName);
        }

        /// <summary>
        /// Extends the <see cref="Microsoft.Maui.Storage.IPreferences"/> interface with the capability
        /// to read objects of any type <typeparamref name="T"/> from preferences.
        /// This method uses the .NET json serializer to deserialize the value stored for <paramref name="key"/>.
        /// </summary>
        public static T? GetFromJson<T>(this Microsoft.Maui.Storage.IPreferences preferences, string key, T? defaultValue = default, string? sharedName = null, JsonSerializerOptions? options = null)
        {
            var stringValue = preferences.Get<string>(key, null, sharedName);
            if (string.IsNullOrEmpty(stringValue))
            {
                return defaultValue;
            }

            try
            {
                var result = JsonSerializer.Deserialize<T>(stringValue, options);
                return result;
            }
            catch (Exception ex)
            {
                // JsonConvert serializes values with leading and trailing quotes.
                // Values in ISecurePreferencesService might be stored without these quotes
                // which leads to a JsonReaderException. In such cases, we try to wrap the string
                // with quotes and attempt to deserialize again. If again no luck, return default value.
                //try
                //{
                //    var result = JsonConvert.DeserializeObject<T>($"\"{stringValue}\"", jsonSerializerSettings);
                //    return result;
                //}
                //catch
                {
                    return defaultValue;
                }
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
