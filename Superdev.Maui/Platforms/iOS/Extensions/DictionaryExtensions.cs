using Foundation;

namespace Superdev.Maui.Platforms.Extensions
{
    public static class DictionaryExtensions
    {
        public static NSDictionary ToNSDictionary(this IDictionary<string, string> dict)
        {
            return NSDictionary.FromObjectsAndKeys(dict.Values.ToArray(), dict.Keys.ToArray());
        }
    }
}