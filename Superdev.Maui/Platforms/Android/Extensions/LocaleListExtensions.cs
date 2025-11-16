using Android.OS;

namespace Superdev.Maui.Platforms.Extensions
{
    public static class LocaleListExtensions
    {
        public static IEnumerable<Java.Util.Locale> ToEnumerable(this LocaleList localeList)
        {
            var localeListSize = localeList.Size();
            for (var i = 0; i < localeListSize; i++)
            {
                yield return localeList.Get(i);
            }
        }
    }
}