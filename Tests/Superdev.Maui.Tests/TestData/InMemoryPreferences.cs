using System.Text.Json;
using IPreferences = Superdev.Maui.Services.IPreferences;

namespace Superdev.Maui.Tests.TestData
{
    /// <summary>
    /// An <see cref="IPreferences"/> which keeps its values in memory, one store per instance and shared container.
    /// </summary>
    public sealed class InMemoryPreferences : IPreferences
    {
        private readonly Dictionary<string, object?> values = new Dictionary<string, object?>();

        public bool ContainsKey(string key, string? sharedName = null)
        {
            return this.values.ContainsKey(StoreKey(key, sharedName));
        }

        public void Remove(string key, string? sharedName = null)
        {
            this.values.Remove(StoreKey(key, sharedName));
        }

        public void Clear(string? sharedName = null)
        {
            var prefix = StoreKey(string.Empty, sharedName);
            foreach (var key in this.values.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList())
            {
                this.values.Remove(key);
            }
        }

        public void Set<T>(string key, T? value)
        {
            this.Set(key, value, sharedName: null);
        }

        public void Set<T>(string key, T? value, string? sharedName = null, JsonSerializerOptions? options = null)
        {
            this.values[StoreKey(key, sharedName)] = value;
        }

        public T? Get<T>(string key, T? defaultValue = default)
        {
            return this.Get(key, defaultValue, sharedName: null);
        }

        public T? Get<T>(string key, T? defaultValue = default, string? sharedName = null, JsonSerializerOptions? options = null)
        {
            return this.values.TryGetValue(StoreKey(key, sharedName), out var value) && value is T typedValue
                ? typedValue
                : defaultValue;
        }

        private static string StoreKey(string key, string? sharedName)
        {
            return $"{sharedName}/{key}";
        }
    }
}
