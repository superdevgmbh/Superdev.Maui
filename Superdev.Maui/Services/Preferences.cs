using System.Text.Json;
using MauiPreferences = Microsoft.Maui.Storage.Preferences;
using Superdev.Maui.Extensions;

namespace Superdev.Maui.Services
{
    public class Preferences : IPreferences
    {
        private static readonly Lazy<IPreferences> Implementation = new Lazy<IPreferences>(CreatePreferences, LazyThreadSafetyMode.PublicationOnly);

        private static IPreferences CreatePreferences() => new Preferences();

        public static IPreferences Current => Implementation.Value;

        private Preferences()
        {
        }

        public void Clear(string? sharedName = null)
        {
            MauiPreferences.Clear(sharedName);
        }

        public bool ContainsKey(string key, string? sharedName = null)
        {
            return MauiPreferences.ContainsKey(key, sharedName);
        }

        public void Remove(string key, string? sharedName = null)
        {
            MauiPreferences.Remove(key, sharedName);
        }

        public T? Get<T>(string key, T? defaultValue = default)
        {
            return this.Get(key, defaultValue, sharedName: null);
        }

        public T? Get<T>(string key, T? defaultValue = default, string? sharedName = null, JsonSerializerOptions? options = null)
        {
            return MauiPreferences.Default.GetFromJson(key, defaultValue, sharedName, options);
        }

        public void Set<T>(string key, T value)
        {
            this.Set(key, value, sharedName: null);
        }

        public void Set<T>(string key, T? value, string? sharedName = null, JsonSerializerOptions? options = null)
        {
            MauiPreferences.Default.SetAsJson(key, value, sharedName, options);
        }
    }
}
